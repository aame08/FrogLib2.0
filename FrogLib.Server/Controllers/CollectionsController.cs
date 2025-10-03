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
                        Rating = rating.Rating,
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
                        .OrderByDescending(c => c.Rating)
                            .ThenByDescending(c => c.CountComments)
                            .ThenByDescending(c => c.CountView)
                        .Take(10)
                        .ToList();

                return Ok(sortedCollections);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("all-collections")]
        public async Task<ActionResult> GetAllCollectionsAsync()
        {
            try
            {
                var collections = await _context.Collections
                    .Where(c => c.StatusCollection == "Одобрено")
                    .Include(c => c.IdUserNavigation)
                    .Include(c => c.Likedcollections)
                    .Include(c => c.IdBooks)
                    .ToListAsync();

                var allCollections = new List<CollectionDTO>();
                foreach (var c in collections)
                {
                    var rating = await _service.GetRatingAsync(c.IdCollection);
                    var countView = await _service.GetCountViewAsync(c.IdCollection);
                    var countComments = await _service.GetCountCommentsAsync(c.IdCollection);

                    allCollections.Add(new CollectionDTO
                    {
                        Id = c.IdCollection,
                        Title = c.TitleCollection,
                        Description = c.DescriptionCollection,
                        Rating = rating.Rating,
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

                return Ok(allCollections);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("collections/{idCollection}")]
        public async Task<ActionResult> GetCollectionInfoAsync(int idCollection)
        {
            try
            {
                var collection = await _context.Collections
                    .Include(c => c.Likedcollections)
                    .Include(c => c.IdUserNavigation)
                    .Include(c => c.IdBooks)
                    .FirstOrDefaultAsync(c => c.IdCollection == idCollection);

                if (collection == null) { return NotFound("Подборка не найдена."); }

                var ratingInfo = await _service.GetRatingAsync(collection.IdCollection);
                var countView = await _service.GetCountViewAsync(collection.IdCollection);
                var comments = await _service.GetCommentsForCollectionAsync(collection.IdCollection);
                var countComments = await _service.GetCountCommentsAsync(collection.IdCollection);
                var books = await _service.GetBooksForCollectionAsync(collection.IdCollection);

                return Ok(new
                {
                    ID = collection.IdCollection,
                    Title = collection.TitleCollection,
                    Description = collection.DescriptionCollection,
                    Rating = ratingInfo,
                    CountBooks = collection.IdBooks.Count,
                    CountView = countView,
                    CountLiked = collection.Likedcollections.Count,
                    CountComments = countComments,
                    CreatedDate = collection.CreatedDate,
                    UserId = collection.IdUserNavigation.IdUser,
                    UserName = collection.IdUserNavigation.NameUser,
                    UserUrl = collection.IdUserNavigation.ProfileImageUrl,
                    Books = books,
                    Comments = comments
                });
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
