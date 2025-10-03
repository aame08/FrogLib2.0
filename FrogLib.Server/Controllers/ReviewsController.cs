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
                    PositiveRating = _service.GetRatingAsync(r.IdReview).Result.Rating,
                    CountView = _service.GetCountViewAsync(r.IdReview).Result,
                    UserName = r.IdUserNavigation.NameUser,
                    UserURL = r.IdUserNavigation.ProfileImageUrl,
                    CreatedDate = r.CreatedDate
                })
                .OrderByDescending(r => r.PositiveRating)
                    .ThenByDescending(r => r.CountView)
                .Take(10)
                .ToList();

                return Ok(popularReviews);
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
