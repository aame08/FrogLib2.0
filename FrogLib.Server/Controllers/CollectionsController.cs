using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionsController(Froglib2Context context, ICollectionsService service) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly ICollectionsService _service = service;

        [HttpGet("popular-collections")]
        public async Task<ActionResult<List<CollectionDTO>>> GetPopularCollectionsAsync()
        {
            try
            {
                var collections = await _context.Collections
                    .Where(c => c.StatusCollection == "Одобрено")
                    .Include(c => c.IdUserNavigation)
                    .Include(c => c.Likedcollections)
                    .Include(c => c.IdBooks)
                    .ToListAsync();

                var popularCollections = new List<CollectionDTO>();
                foreach (var c in collections)
                {
                    var rating = await _service.GetRatingAsync(c.IdCollection);
                    var countView = await _service.GetCountViewAsync(c.IdCollection);
                    var countComments = await _service.GetCountCommentsAsync(c.IdCollection);

                    popularCollections.Add(new CollectionDTO
                    {
                        Id = c.IdCollection,
                        Title = c.TitleCollection,
                        Description = c.DescriptionCollection,
                        PositiveRating = rating.PositivePercent,
                        Books = c.IdBooks
                            .Select(bc => new BookDTO
                            {
                                Id = bc.IdBook,
                                Title = bc.TitleBook,
                                ImageURL = bc.ImageUrl
                            })
                            .ToList(),
                        CountView = countView,
                        CountComments = countComments,
                        CountLiked = c.Likedcollections.Count,
                        CreatedDate = c.CreatedDate,
                        UserName = c.IdUserNavigation.NameUser,
                        UserURL = c.IdUserNavigation.ProfileImageUrl
                    });
                }

                var sortedCollections = popularCollections
                        .OrderByDescending(c => c.PositiveRating)
                            .ThenByDescending(c => c.CountComments)
                            .ThenByDescending(c => c.CountView)
                        .Take(10)
                        .ToList();

                return Ok(sortedCollections);
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
