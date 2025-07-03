using FrogLib.Server.DTOs;
using FrogLib.Server.JWTTokens;
using FrogLib.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(JwtProvider provider, Froglib2Context context) : BaseController
    {
        private readonly JwtProvider _provider = provider;
        private readonly Froglib2Context _context = context;

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
                    var fileName = Path.GetFileName(model.ProfileImageUrl.FileName);
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
                const string senderEmail = "vika.korsakova.2016@gmail.com";
                const string senderPassword = "ueyh ozln ivld kxbt";

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
