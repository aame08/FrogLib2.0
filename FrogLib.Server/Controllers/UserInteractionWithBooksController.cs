using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInteractionWithBooksController(Froglib2Context context, IUsersService service) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly IUsersService _service = service;

        private const string TypeObject = "Книга";

        [Authorize(Roles = "Пользователь")]
        [HttpGet("book-evaluation/{idUser}/{idBook}")]
        public async Task<ActionResult<int>> GetBookEvaluation(int idUser, int idBook)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var bookEvaluation = await _context.Bookratings
                    .FirstOrDefaultAsync(e => e.IdUser == idUser && e.IdBook == idBook);

                if (bookEvaluation == null) { return 0; }

                return bookEvaluation.Rating;
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("update-book-evaluation/{idUser}/{idBook}/{evaluation}")]
        public async Task<ActionResult> UpdateBookEvaluation(int idUser, int idBook, int evaluation)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                if (evaluation <= 0 || evaluation > 5) { return Conflict("Некорректная оценка."); }

                var userEvalution = await _context.Bookratings
                    .FirstOrDefaultAsync(e => e.IdUser == idUser && e.IdBook == idBook);

                if (userEvalution != null)
                {
                    userEvalution.Rating = evaluation;
                }
                else
                {
                    var newBookEvaluation = new Bookrating
                    {
                        IdRating = _context.Bookratings.Any() ? _context.Bookratings.Max(r => r.IdRating) + 1 : 1,
                        IdUser = idUser,
                        IdBook = idBook,
                        Rating = evaluation
                    };

                    _context.Bookratings.Add(newBookEvaluation);
                }

                await _context.SaveChangesAsync();

                return Ok("Пользовательская оценка добавлена/изменена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpDelete("delete-book-evaluation/{idUser}/{idBook}")]
        public async Task<ActionResult> DeleteBookEvalution(int idUser, int idBook)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var deletingEvaluation = await _context.Bookratings
                    .FirstOrDefaultAsync(e => e.IdUser == idUser && e.IdBook == idBook);

                if (deletingEvaluation == null) { return NotFound("Оценка не найдена."); }

                _context.Bookratings.Remove(deletingEvaluation);

                await _context.SaveChangesAsync();

                return Ok("Оценка удалена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpGet("user-book/{idUser}/{idBook}")]
        public async Task<ActionResult<string>> GetUserBook(int idUser, int idBook)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var userList = await _context.Userbooks
                    .FirstOrDefaultAsync(l => l.IdUser == idUser && l.IdBook == idBook);

                if (userList == null) { return null; }

                return userList.ListType;
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("update-user-book/{idUser}/{idBook}/{listType}")]
        public async Task<ActionResult> UpdateUserBook(int idUser, int idBook, string listType)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var userList = await _context.Userbooks
                    .FirstOrDefaultAsync(l => l.IdUser == idUser && l.IdBook == idBook);

                if (userList != null)
                {
                    if (userList.ListType == listType) { return Ok("Книга уже добавлена."); }
                    else 
                    {
                        userList.ListType = listType;
                        userList.AddedDate = DateOnly.FromDateTime(DateTime.Now);
                    }
                }
                else
                {
                    var newUserList = new Userbook
                    {
                        IdUser = idUser,
                        IdBook = idBook,
                        ListType = listType,
                        AddedDate = DateOnly.FromDateTime(DateTime.Now),
                    };

                    _context.Userbooks.Add(newUserList);
                }

                await _context.SaveChangesAsync();

                return Ok("Книга добавлена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpDelete("delete-user-book/{idUser}/{idBook}")]
        public async Task<ActionResult> DeleteUserBook(int idUser, int idBook)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(TypeObject, idBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var userList = await _context.Userbooks
                    .FirstOrDefaultAsync(l => l.IdUser == idUser && l.IdBook == idBook);

                if (userList == null) { return NotFound("Книга не была добавлена."); }

                _context.Userbooks.Remove(userList);

                await _context.SaveChangesAsync();

                return Ok("Книга удалена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
