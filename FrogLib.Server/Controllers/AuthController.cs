using FrogLib.Server.DTOs;
using FrogLib.Server.JWTTokens;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(JwtProvider provider, Froglib2Context context, IUsersService service) : BaseController
    {
        private readonly JwtProvider _provider = provider;
        private readonly Froglib2Context _context = context;
        private readonly IUsersService _service = service;

        [HttpPost("register")]
        public ActionResult<User> Register([FromForm] UserDTO model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NameUser) || model.NameUser.Length > 100)
                {
                    ModelState.AddModelError("NameUser", "Неверный формат имени пользователя.");
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают.");
                }

                if (model.NewPassword.Length < 8)
                {
                    ModelState.AddModelError("ShortPassword", "Пароль должен состоять от 8 символов.");
                }

                var existingUser = _context.Users.FirstOrDefault(u => u.LoginUser == model.LoginUser);
                if (existingUser != null)
                {
                    ModelState.AddModelError("LoginUser", "Данная электронная почта уже занята.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(new { errors });
                }

                string filePath = null;
                if (model.ProfileImageUrl != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImageUrl.FileName)}";
                    filePath = $"/images/{fileName}";
                    var absolutePath = Path.Combine("wwwroot", "images", fileName);
                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        model.ProfileImageUrl.CopyTo(stream);
                    }
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                var newUser = new User
                {
                    IdUser = _context.Users.Any() ? _context.Users.Max(u => u.IdUser) + 1 : 1,
                    NameRole = "Пользователь",
                    NameUser = model.NameUser,
                    LoginUser = model.LoginUser,
                    PasswordHash = passwordHash,
                    ProfileImageUrl = filePath,
                    RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
                    StatusUser = "Активен"
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return StatusCode(201, newUser);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpPost("login/{email}/{password}")]
        public ActionResult Login(string email, string password)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.LoginUser == email);
                if (user == null)
                {
                    ModelState.AddModelError("Unauthorized", "Пользователь не найден.");
                }
                else if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    ModelState.AddModelError("InvalidPassword", "Неверный пароль.");
                }
                else if (user.StatusUser == "Заблокирован")
                {
                    ModelState.AddModelError("BannedUser", "Вы заблокированы в системе.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(new { errors });
                }

                var accessToken = _provider.GenerateToken(user);
                var refreshToken = _provider.GenerateRefreshToken(user);

                return Ok(new { accessToken, refreshToken, user });
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpPost("reset-password")]
        public ActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                string senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL")
                    ?? configuration["EmailSettings:SenderEmail"];

                string senderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD")
                    ?? configuration["EmailSettings:SenderPassword"];

                var user = _context.Users
                    .FirstOrDefault(u => u.LoginUser == request.UserEmail);

                if (user == null)
                {
                    ModelState.AddModelError("LoginUser", "Пользователь с указанной электронной почтой не найден.");
                    return BadRequest(new
                    {
                        errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                    });
                }

                var newPassword = GenerateRandomPassword(10);
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                user.PasswordHash = passwordHash;
                _context.SaveChanges();

                try
                {
                    SendPasswordResetEmail(senderEmail, senderPassword, user.LoginUser, newPassword);
                    return Ok(new { message = "Новый пароль отправлен на указанную электронную почту." });
                }
                catch (Exception ex)
                {
                    return Ok(new
                    {
                        message = "Пароль был изменен, но не удалось отправить email. " +
                                 "Используйте новый пароль для входа: " + newPassword
                    });
                }
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }
        }

        [HttpPost("refresh-token")]
        public ActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _provider.ValidateToken(request.RefreshToken);
                var claimType = principal.Claims.FirstOrDefault(c => c.Type == "type")?.Value;

                if (claimType != "refresh")
                {
                    return BadRequest("Невалидный токен.");
                }

                var idUser = int.Parse(principal.Claims.First(c => c.Type == "id_user").Value);
                var user = _context.Users.First(u => u.IdUser == idUser);

                var newAccessToken = _provider.GenerateToken(user);
                var newRefreshToken = _provider.GenerateRefreshToken(user);

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize]
        [HttpPut("update-profile/{idUser}")]
        public async Task<ActionResult> UpdateProfileAsync(int idUser, [FromForm] UserDTO model)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser))
                {
                    return Unauthorized("Несоответствие пользователя.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == idUser);
                if (user == null)
                {
                    ModelState.AddModelError("UserNotFound", "Пользователь не найден.");
                }

                if (user.StatusUser == "Заблокирован")
                {
                    ModelState.AddModelError("BannedUser", "Вы заблокированы в системе.");
                }

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.OldPassword))
                        ModelState.AddModelError("OldPassword", "Требуется текущий пароль.");
                    else if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
                        ModelState.AddModelError("OldPassword", "Неверный текущий пароль.");

                    if (model.NewPassword != model.ConfirmPassword)
                        ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают.");
                }

                if (!string.IsNullOrEmpty(model.LoginUser) && model.LoginUser != user.LoginUser)
                {
                    if (await _context.Users.AnyAsync(u => u.LoginUser == model.LoginUser))
                        ModelState.AddModelError("LoginUser", "Почта уже используется.");
                }

                if (model.DeleteImage == "true" && model.ProfileImageUrl != null)
                {
                    ModelState.AddModelError("ProfileImageUrl", "Нельзя одновременно удалять и загружать изображение.");
                }
                else if (model.DeleteImage != "true" && model.ProfileImageUrl == null && string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    ModelState.AddModelError("ProfileImageUrl", "Поле ProfileImageUrl является обязательным, если изображение не удаляется.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(new { errors });
                }

                if (!string.IsNullOrEmpty(model.NameUser))
                    user.NameUser = model.NameUser;

                if (!string.IsNullOrEmpty(model.LoginUser))
                    user.LoginUser = model.LoginUser;

                if (!string.IsNullOrEmpty(model.NewPassword))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                if (model.DeleteImage == "true")
                {
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        var oldPath = Path.Combine("wwwroot", user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        user.ProfileImageUrl = null;
                    }
                }
                else if (model.ProfileImageUrl != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImageUrl.FileName)}";
                    var filePath = Path.Combine("wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImageUrl.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        var oldPath = Path.Combine("wwwroot", user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    user.ProfileImageUrl = $"/images/{fileName}";
                }

                await _context.SaveChangesAsync();

                return Ok(new { profileImageUrl = user.ProfileImageUrl });
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var random = new Random();
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(chars);
        }

        private void SendPasswordResetEmail(string senderEmail, string senderPassword, string recipientEmail, string newPassword)
        {
            var fromAddress = new MailAddress(senderEmail, "FrogLib Support");
            var toAddress = new MailAddress(recipientEmail);

            using var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                Timeout = 10000
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Восстановление пароля FrogLib",
                Body = $@"Уважаемый пользователь FrogLib,

По вашему запросу был сгенерирован новый пароль для вашего аккаунта.

Ваш новый пароль: {newPassword}

Рекомендуем вам изменить этот пароль после входа в систему.

С уважением,
Команда поддержки FrogLib"
            };

            smtp.Send(message);
        }
    }
}
