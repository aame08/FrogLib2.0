using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(Froglib2Context context, IReviewsService service) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly IReviewsService _service = service;

        [HttpGet("popular-reviews")]
        public async Task<ActionResult<List<ReviewDTO>>> GetPopularReviewsAsync()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.StatusReview == "Одобрено")
                    .Include(r => r.IdUserNavigation)
                    .Include(r => r.IdBookNavigation)
                    .ToListAsync();

                var popularReviews = reviews.Select(r => new ReviewDTO
                {
                    Id = r.IdReview,
                    Title = r.TitleReview,
                    Content = r.TextReview,
                    Book = _service.GetBookForReviewAsync(r.IdReview).Result,
                    UserRating = _service.GetBookUserRatingAsync(r.IdBook, r.IdUser).Result,
                    Rating = _service.GetRatingAsync(r.IdReview).Result.Rating,
                    CountView = _service.GetCountViewAsync(r.IdReview).Result,
                    UserName = r.IdUserNavigation.NameUser,
                    UserURL = r.IdUserNavigation.ProfileImageUrl,
                    CreatedDate = r.CreatedDate
                })
                .OrderByDescending(r => r.Rating)
                    .ThenByDescending(r => r.CountView)
                .Take(10)
                .ToList();

                return Ok(popularReviews);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("all-reviews")]
        public async Task<ActionResult> GetAllReviewsAsync()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.StatusReview == "Одобрено")
                    .Include(r => r.IdUserNavigation)
                    .Include(r => r.IdBookNavigation)
                    .ToListAsync();

                var allReviews = reviews.Select(r => new ReviewDTO
                {
                    Id = r.IdReview,
                    Title = r.TitleReview,
                    Content = r.TextReview,
                    Book = _service.GetBookForReviewAsync(r.IdReview).Result,
                    UserRating = _service.GetBookUserRatingAsync(r.IdBook, r.IdUser).Result,
                    Rating = _service.GetRatingAsync(r.IdReview).Result.Rating,
                    CountView = _service.GetCountViewAsync(r.IdReview).Result,
                    UserName = r.IdUserNavigation.NameUser,
                    UserURL = r.IdUserNavigation.ProfileImageUrl,
                    CreatedDate = r.CreatedDate
                })
                .ToList();

                return Ok(allReviews);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("reviews/{idReview}")]
        public async Task<ActionResult> GetReviewInfoAsync(int idReview)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.IdUserNavigation)
                    .Include(r => r.IdBookNavigation)
                    .FirstOrDefaultAsync(r => r.IdReview == idReview);

                if (review == null) { return NotFound("Рецензия не найдена"); }

                var ratingInfo = await _service.GetRatingAsync(review.IdReview);
                var countView = await _service.GetCountViewAsync(review.IdReview);
                var countComments = await _service.GetCountCommentsAsync(review.IdReview);
                var comments = await _service.GetCommentsForReviewAsync(review.IdReview);
                var userEvaluation = await _service.GetBookUserRatingAsync(review.IdBook, review.IdUser);
                var book = await _service.GetBookForReviewAsync(review.IdReview);

                return Ok(new
                {
                    ID = review.IdReview,
                    Title = review.TitleReview,
                    Text = review.TextReview,
                    UserEvaluation = userEvaluation,
                    Book = book,
                    Rating = ratingInfo,
                    CountView = countView,
                    CountComments = countComments,
                    Comments = comments,
                    CreatedDate = review.CreatedDate,
                    UserId = review.IdUserNavigation.IdUser,
                    UserName = review.IdUserNavigation.NameUser,
                    UserUrl = review.IdUserNavigation.ProfileImageUrl
                });
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
