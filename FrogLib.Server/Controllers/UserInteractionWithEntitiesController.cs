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
                if (await _moderationService.CheckTextAsync(request.TitleReview) || await _moderationService.CheckTextAsync(request.PlaintText))
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

                if (await _moderationService.CheckTextAsync(request.TitleCollection) || await _moderationService.CheckTextAsync(request.PlaintDescription))
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

        [Authorize(Roles = "Пользователь")]
        [HttpGet("get-entity-rating")]
        public async Task<ActionResult<int>> GetEntityRatingAsync([FromQuery] EntityRatingDTO request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync(request.TypeEntity, request.IdEntity))
                {
                    return NotFound("Сущность не найдена.");
                }

                var rating = await _context.Entityratings
                    .FirstOrDefaultAsync(r => r.IdUser == request.IdUser && r.IdEntity == request.IdEntity && r.TypeEntity == request.TypeEntity);

                if (rating == null) { return -1; }

                return rating.IsLike;
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("update-entity-rating")]
        public async Task<ActionResult> UpdateEntityRatingAsync([FromBody] EntityRatingDTO request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync(request.TypeEntity, request.IdEntity))
                {
                    return NotFound("Сущность не найдена.");
                }

                var rating = await _context.Entityratings
                    .FirstOrDefaultAsync(r => r.IdUser == request.IdUser && r.IdEntity == request.IdEntity && r.TypeEntity == request.TypeEntity);

                if (rating != null)
                {
                    rating.IsLike = Convert.ToSByte(request.Rating);
                }
                else
                {
                    var newRating = new Entityrating
                    {
                        IdRating = _context.Entityratings.Any() ? _context.Entityratings.Max(u => u.IdRating) + 1 : 1,
                        IdUser = request.IdUser,
                        IdEntity = request.IdEntity,
                        TypeEntity = request.TypeEntity,
                        IsLike = Convert.ToSByte(request.Rating)
                    };

                    _context.Entityratings.Add(newRating);
                }

                await _context.SaveChangesAsync();

                return Ok("Оценка обновлена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpDelete("delete-entity-rating")]
        public async Task<ActionResult> DeleteEntityRatingAsync([FromQuery] EntityRatingDTO request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync(request.TypeEntity, request.IdEntity))
                {
                    return NotFound("Сущность не найдена.");
                }

                var rating = await _context.Entityratings
                    .FirstOrDefaultAsync(r => r.IdUser == request.IdUser && r.IdEntity == request.IdEntity && r.TypeEntity == request.TypeEntity);

                if (rating == null)
                {
                    return NotFound("Оценка не найдена.");
                }

                _context.Entityratings.Remove(rating);

                await _context.SaveChangesAsync();

                return Ok("Оценка обновлена.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpGet("is-collection-saved/{idUser}/{idCollection}")]
        public async Task<ActionResult<bool>> IsCollectionSavedAsync(int idUser, int idCollection)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, idUser) || !await _usersService.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync("Подборка", idCollection))
                {
                    return NotFound("Подборка не найдена.");
                }

                var isLike = await _context.Likedcollections
                    .FirstOrDefaultAsync(c => c.IdUser == idUser && c.IdCollection == idCollection);

                if (isLike == null) { return false; }

                return true;
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("like-collection/{idUser}/{idCollection}")]
        public async Task<ActionResult> LikeCollectionAsync(int idUser, int idCollection)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, idUser) || !await _usersService.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync("Подборка", idCollection))
                {
                    return NotFound("Подборка не найдена.");
                }

                var collection = await _context.Collections
                    .FirstOrDefaultAsync(c => c.IdCollection == idCollection);

                if (collection.IdUser == idUser)
                {
                    return Conflict("Автор не может сохранить свою подборку.");
                }

                var likedCollection = new Likedcollection
                {
                    IdCollection = idCollection,
                    IdUser = idUser,
                    LikedDate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Likedcollections.Add(likedCollection);

                await _context.SaveChangesAsync();

                return Ok("Подборка добавлена в избранные.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpDelete("unlike-collection/{idUser}/{idCollection}")]
        public async Task<ActionResult> UnlikeCollectionAsync(int idUser, int idCollection)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, idUser) || !await _usersService.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync("Подборка", idCollection))
                {
                    return NotFound("Подборка не найдена.");
                }

                var likedCollection = await _context.Likedcollections
                    .FirstOrDefaultAsync(c => c.IdUser == idUser && c.IdCollection == idCollection);

                if (likedCollection == null)
                {
                    return NotFound("Подборка не была добавлена в избранное.");
                }

                _context.Likedcollections.Remove(likedCollection);

                await _context.SaveChangesAsync();

                return Ok("Подборка удалена из избранных.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("add-comment")]
        public async Task<ActionResult> AddCommentAsync([FromQuery] CommentRequest request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync(request.TypeEntity, request.IdEntity))
                {
                    return NotFound("Сущность не найдена.");
                }

                var statusComment = "Новое";
                if (await _moderationService.CheckTextAsync(request.Text))
                {
                    statusComment = "Обнаружено нарушение";

                    request.Text = await _moderationService.HighlightTextAsync(request.Text);
                }

                var newComment = new Comment
                {
                    IdComment = _context.Comments.Any() ? _context.Comments.Max(c => c.IdComment) + 1 : 1,
                    IdUser = request.IdUser,
                    IdEntity = request.IdEntity,
                    TypeEntity = request.TypeEntity,
                    TextComment = request.Text,
                    DateComment = DateTime.Now,
                    StatusComment = statusComment
                };

                _context.Comments.Add(newComment);

                await _context.SaveChangesAsync();

                return Ok("Комментарий добавлен.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [Authorize(Roles = "Пользователь")]
        [HttpPost("add-reply")]
        public async Task<ActionResult> AddReplyCommentAsync([FromBody] CommentRequest request)
        {
            try
            {
                if (!_usersService.IsAuthorizedUser(User, request.IdUser) || !await _usersService.UserExistsAsync(request.IdUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _usersService.ObjectExistsAsync(request.TypeEntity, request.IdEntity))
                {
                    return NotFound("Сущность не найдена.");
                }

                if (!await _usersService.ObjectExistsAsync("Комментарий", request.IdParentComment.Value))
                {
                    return NotFound("Родительский комментарий не найден.");
                }

                var statusComment = "Новое";
                if (await _moderationService.CheckTextAsync(request.Text))
                {
                    statusComment = "Обнаружено нарушение";

                    request.Text = await _moderationService.HighlightTextAsync(request.Text);
                }

                var newReply = new Comment
                {
                    IdComment = _context.Comments.Any() ? _context.Comments.Max(c => c.IdComment) + 1 : 1,
                    IdUser = request.IdUser,
                    IdEntity = request.IdEntity,
                    TypeEntity = request.TypeEntity,
                    TextComment = request.Text,
                    DateComment = DateTime.Now,
                    StatusComment = statusComment,
                    ParentCommentId = request.IdParentComment
                };

                _context.Comments.Add(newReply);

                await _context.SaveChangesAsync();

                return Ok("Ответ на комментарий добавлен.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
