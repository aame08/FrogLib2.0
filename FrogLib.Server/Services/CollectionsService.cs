using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Services
{
    public interface ICollectionsService
    {
        Task<RatingInfo> GetRatingAsync(int idCollection);
        Task<int> GetCountViewAsync(int idCollection);
        Task<int> GetCountCommentsAsync(int idCollection);
        //Task<List<CommentDTO>> GetCommentsForCollectionAsync(int idCollection);
        Task<List<BookDTO>> GetBooksForCollectionAsync(int idCollection);
    }

    public class CollectionsService(Froglib2Context context, Lazy<IBooksService> booksService) : BaseEntityService(context, "Подборка"), ICollectionsService
    {
        private readonly Froglib2Context _context = context;
        private readonly Lazy<IBooksService> _booksService = booksService;

        //public Task<List<CommentDTO>> GetCommentsForCollectionAsync(int idCollection)
        //{
        //    return GetCommentsAsync(idCollection);
        //}

        public async Task<List<BookDTO>> GetBooksForCollectionAsync(int idCollection)
        {
            var collection = await _context.Collections
                .Include(c => c.IdBooks)
                .FirstOrDefaultAsync(c => c.IdCollection == idCollection);

            if (collection == null || collection.IdBooks == null) { return new List<BookDTO>(); }

            var books = new List<BookDTO>();
            foreach (var b in collection.IdBooks)
            {
                var averageRating = await _booksService.Value.GetAverageRatingAsync(b.IdBook);

                books.Add(new BookDTO
                {
                    Id = b.IdBook,
                    Title = b.TitleBook,
                    ImageURL = b.ImageUrl,
                    AverageRating = averageRating
                });
            }

            return books;
        }
    }
}
