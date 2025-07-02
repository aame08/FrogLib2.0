using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController(Froglib2Context context) : BaseController
    {
        private readonly Froglib2Context _context = context;

        [HttpGet("get-statistics")]
        public async Task<ActionResult<StatisticInfo>> GetStatistics()
        {
            try
            {
                var countUsers = await _context.Users
                    .Where(u => u.NameRole == "Пользователь" && u.StatusUser == "Активен")
                    .CountAsync();

                var countBooks = await _context.Books.CountAsync();

                var countReviews = await _context.Reviews
                    .Where(r => r.StatusReview == "Одобрено")
                    .CountAsync();

                var countCollections = await _context.Collections
                    .Where(c => c.StatusCollection == "Одобрено")
                    .CountAsync();

                return Ok(new StatisticInfo
                {
                    CountUsers = countUsers,
                    CountBooks = countBooks,
                    CountReviews = countReviews,
                    CountCollections = countCollections
                });
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
