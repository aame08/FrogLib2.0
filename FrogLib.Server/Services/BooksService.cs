using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Services
{
    public interface IBooksService
    {
        Task<double> GetAverageRatingAsync(int idBook);
        Task<int> GetCountViewAsync(int idBook);
        //Task<List<ReviewDTO>> GetReviewsForBookAsync(int idBook);
        //Task<List<CollectionDTO>> GetCollectionsForBookAsync(int idBook);
        Task<string> GetAuthorsFullNameAsync(int idBook);
    }

    public class BooksService(Froglib2Context context, IReviewsService reviewsService, ICollectionsService collectionsService) : IBooksService
    {
        private readonly Froglib2Context _context = context;
        private readonly IReviewsService _reviewsService = reviewsService;
        private readonly ICollectionsService _collectionsService = collectionsService;

        private const string TypeObject = "Книга";

        public async Task<double> GetAverageRatingAsync(int idBook)
        {
            var book = await _context.Books
                .Include(b => b.Bookratings)
                .FirstOrDefaultAsync(b => b.IdBook == idBook);

            if (book == null || !book.Bookratings.Any()) { return 0; }

            return book.Bookratings.Average(r => r.Rating);
        }

        public async Task<int> GetCountViewAsync(int idBook)
        {
            return await _context.Viewhistories
                .CountAsync(v => v.TypeEntity == TypeObject && v.IdEntity == idBook);
        }

        //public async Task<List<ReviewDTO>> GetReviewsForBookAsync(int idBook)
        //{
        //    var reviews = await _context.Reviews
        //        .Where(r => r.IdBook == idBook && r.StatusReview == "Одобрено")
        //        .Include(r => r.IdBookNavigation)
        //        .Include(r => r.IdUserNavigation)
        //        .ToListAsync();

        //    var result = new List<ReviewDTO>();
        //    foreach (var r in reviews)
        //    {
        //        var rating = await _reviewsService.GetRatingAsync(r.IdReview);
        //        var countView = await _reviewsService.GetCountViewAsync(r.IdReview);

        //        result.Add(new ReviewDTO
        //        {
        //            Id = r.IdReview,
        //            Title = r.TitleReview,
        //            Content = r.TextReview,
        //            ImageURL = r.IdBookNavigation.ImageUrl,
        //            Rating = rating.PositivePercent,
        //            CountView = countView,
        //            CreatedDate = r.CreatedDate,
        //            UserName = r.IdUserNavigation.NameUser,
        //            UserURL = r.IdUserNavigation.ProfileImageUrl
        //        });
        //    }

        //    return result;
        //}

        //public async Task<List<CollectionDTO>> GetCollectionsForBookAsync(int idBook)
        //{
        //    var book = await _context.Books
        //        .Include(b => b.IdCollections)
        //            .ThenInclude(c => c.IdBooks)
        //        .Include(b => b.IdCollections)
        //            .ThenInclude(c => c.IdUserNavigation)
        //        .FirstOrDefaultAsync(b => b.IdBook == idBook);

        //    if (book == null || book.IdCollections == null) { return new List<CollectionDTO>(); }

        //    var collections = new List<CollectionDTO>();
        //    foreach (var c in book.IdCollections.Where(c => c.StatusCollection == "Одобрено"))
        //    {
        //        var rating = await _collectionsService.GetRatingAsync(c.IdCollection);
        //        var countView = await _collectionsService.GetCountViewAsync(c.IdCollection);
        //        var countComments = await _collectionsService.GetCountCommentsAsync(c.IdCollection);

        //        collections.Add(new CollectionDTO
        //        {
        //            Id = c.IdCollection,
        //            Title = c.TitleCollection,
        //            Description = c.DescriptionCollection,
        //            Rating = rating.PositivePercent,
        //            CountBooks = c.IdBooks.Count,
        //            Books = c.IdBooks
        //                .Take(3)
        //                .Select(b => new BookDTO
        //                {
        //                    Id = b.IdBook,
        //                    Title = b.TitleBook,
        //                    ImageURL = b.ImageUrl
        //                })
        //                .ToList(),
        //            CountView = countView,
        //            CountComments = countComments,
        //            CountLiked = c.Likedcollections.Count,
        //            CreatedDate = c.CreatedDate,
        //            UserName = c.IdUserNavigation?.NameUser,
        //            UserURL = c.IdUserNavigation?.ProfileImageUrl
        //        });
        //    }

        //    return collections;
        //}

        public async Task<string> GetAuthorsFullNameAsync(int idBook)
        {
            var authors = await _context.Books
                .Where(b => b.IdBook == idBook)
                .SelectMany(b => b.IdAuthors)
                .Select(a => $"{a.SurnameAuthor} {a.NameAuthor} {a.PatronymicAuthor}")
                .ToListAsync();

            return string.Join(", ", authors);
        }
    }
}
