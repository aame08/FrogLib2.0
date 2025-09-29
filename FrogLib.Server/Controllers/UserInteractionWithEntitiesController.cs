using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInteractionWithEntitiesController(Froglib2Context context, IUsersService usersService, ITextModerationService moderationService) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly IUsersService _usersService = usersService;
        private readonly ITextModerationService _moderationService = moderationService;

        //private const string TypeObject = "Рецензия";

        [Authorize(Roles = "Пользователь")]
        [HttpPost("add-review")]
        public async Task<ActionResult> AddReviewAsync([FromBody] ReviewRequest request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync("Книга", request.IdBook))
                {
                    return NotFound("Книга не найдена.");
                }

                var statusReview = "На рассмотрении";
                if (!await _moderationService.CheckTextAsync(request.TitleReview) || !await _moderationService.CheckTextAsync(request.PlaintText))
                {
                    statusReview = "Обнаружено нарушение";

                    request.TitleReview = await _moderationService.HighlightTextAsync(request.TitleReview);
                    request.ContentReview = await _moderationService.HighlightTextAsync(request.ContentReview);
                }

                var newReview = new Review
                {
                    IdReview = _context.Reviews.Any() ? _context.Reviews.Max(r => r.IdReview) + 1 : 1,
                    IdUser = request.IdUser,
                    IdBook = request.IdBook,
                    TitleReview = request.TitleReview,
                    TextReview = request.ContentReview,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    StatusReview = statusReview
                };

                _context.Reviews.Add(newReview);

                await _context.SaveChangesAsync();

                return Ok("Рецензия отправлена на рассмотрение.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("add-collection")]
        public async Task<ActionResult> AddCollectionAsync([FromBody] CollectionRequest request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (request.IdBooks.Count < 5)
                {
                    return Conflict("В подборке должно быть от 5 книг.");
                }

                foreach (var book in request.IdBooks)
                {
                    if (!await _usersService.ObjectExistsAsync("Книга", book))
                    {
                        return NotFound("Книга не найдена.");
                    }
                }

                var statusCollection = "На рассмотрении";
                if (!await _moderationService.CheckTextAsync(request.TitleCollection) || !await _moderationService.CheckTextAsync(request.PlaintDescription))
                {
                    statusCollection = "Обнаружено нарушение";

                    request.TitleCollection = await _moderationService.HighlightTextAsync(request.TitleCollection);
                    request.DescriptionCollection = await _moderationService.HighlightTextAsync(request.DescriptionCollection);
                }

                var books = await _context.Books
                    .Where(b => request.IdBooks.Contains(b.IdBook))
                    .ToListAsync();

                var newCollection = new Collection
                {
                    IdCollection = _context.Collections.Any() ? _context.Collections.Max(c => c.IdCollection) + 1 : 1,
                    IdUser = request.IdUser,
                    TitleCollection = request.TitleCollection,
                    DescriptionCollection = request.DescriptionCollection,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    StatusCollection = statusCollection,
                    IdBooks = books
                };

                _context.Collections.Add(newCollection);

                await _context.SaveChangesAsync();

                return Ok("Подборка отправлена на рассмотрение.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
