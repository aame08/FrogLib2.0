using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Services
{
    public interface IBooksService
    {
        Task<double> GetAverageRatingAsync(int idBook);
        Task<int> GetCountViewAsync(int idBook);
        Task<string> GetAuthorsFullNameAsync(int idBook);
        Task<BookRatingStatisticsDTO> GetRatingStatisticsAsync(int idBook);
        Task<BookmarksStatisticsDTO> GetBookmarksStatisticsAsync(int idBook);
        Task<List<ReviewDTO>> GetReviewsForBookAsync(int idBook);
        Task<List<CollectionDTO>> GetCollectionsForBookAsync(int idBook);
        Task<List<BookDTO>> GetBookRecommendationsAsync(int idUser);
        Task<List<BookDTO>> GetSimilarBooksAsync(int idBook);
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

        public async Task<string> GetAuthorsFullNameAsync(int idBook)
        {
            var authors = await _context.Books
                .Where(b => b.IdBook == idBook)
                .SelectMany(b => b.IdAuthors)
                .Select(a => $"{a.SurnameAuthor} {a.NameAuthor} {a.PatronymicAuthor}")
                .ToListAsync();

            return string.Join(", ", authors);
        }

        public async Task<BookRatingStatisticsDTO> GetRatingStatisticsAsync(int idBook)
        {
            var book = await _context.Books
                .Include(b => b.Bookratings)
                .FirstOrDefaultAsync(b => b.IdBook == idBook);

            var totalRating = book.Bookratings?.Count ?? 0;
            var averageRating = await GetAverageRatingAsync(idBook);

            var ratingDistribution = Enumerable.Range(1, 5)
                .Select(ratingValue => new RatingDistribution
                {
                    RatingValue = ratingValue,
                    Count = book.Bookratings?.Count(r => r.Rating == ratingValue) ?? 0,
                    Percentage = totalRating > 0 ?
                        (double)book.Bookratings?.Count(r => r.Rating == ratingValue) / totalRating * 100 : 0
                })
               .OrderByDescending(r => r.RatingValue)
               .ToList();

            return new BookRatingStatisticsDTO
            {
                TotalRatings = totalRating,
                AverageRating = averageRating,
                RatingDistribution = ratingDistribution
            };
        }

        public async Task<BookmarksStatisticsDTO> GetBookmarksStatisticsAsync(int idBook)
        {
            var book = await _context.Books
                .Include(b => b.Userbooks)
                .FirstOrDefaultAsync(b => b.IdBook == idBook);

            var totalUserBookmarks = book.Userbooks?.Count ?? 0;
            var bookmarkDistribution = new List<string> { "Читаю", "В планах", "Брошено", "Прочитано", "Любимые" }
                .Select(listType => new BookmarkDistribution
                {
                    ListType = listType,
                    Count = book.Userbooks?.Count(ub => ub.ListType == listType) ?? 0,
                    Percentage = totalUserBookmarks > 0 ?
                        ((double)(book.Userbooks?.Count(ub => ub.ListType == listType) ?? 0) / totalUserBookmarks * 100) : 0
                })
                .ToList();

            return new BookmarksStatisticsDTO
            {
                TotalBookmarks = totalUserBookmarks,
                BookmarkDistribution = bookmarkDistribution
            };
        }

        public async Task<List<ReviewDTO>> GetReviewsForBookAsync(int idBook)
        {
            var reviews = await _context.Reviews
                .Where(r => r.IdBook == idBook && r.StatusReview == "Одобрено")
                .Include(r => r.IdBookNavigation)
                .Include(r => r.IdUserNavigation)
                .ToListAsync();

            var result = new List<ReviewDTO>();
            foreach (var r in reviews)
            {
                var rating = await _reviewsService.GetRatingAsync(r.IdReview);
                var book = await _reviewsService.GetBookForReviewAsync(r.IdReview);
                var positiveRating = rating.Rating;

                result.Add(new ReviewDTO
                {
                    Id = r.IdReview,
                    Title = r.TitleReview,
                    Content = r.TextReview,
                    Book = book,
                    Rating = positiveRating,
                    CreatedDate = r.CreatedDate,
                    UserName = r.IdUserNavigation.NameUser,
                    UserURL = r.IdUserNavigation.ProfileImageUrl
                });
            }

            return result;
        }

        public async Task<List<CollectionDTO>> GetCollectionsForBookAsync(int idBook)
        {
            var collections = await _context.Collections
                .Where(c => c.IdBooks.Any(b => b.IdBook == idBook) && c.StatusCollection == "Одобрено")
                .Include(c => c.IdBooks)
                .Include(c => c.IdUserNavigation)
                .ToListAsync();

            var result = new List<CollectionDTO>();
            foreach (var c in collections)
            {
                var rating = await _collectionsService.GetRatingAsync(c.IdCollection);
                var countView = await _collectionsService.GetCountViewAsync(c.IdCollection);
                var positiveRating = rating.Rating;

                result.Add(new CollectionDTO
                {
                    Id = c.IdCollection,
                    Title = c.TitleCollection,
                    Description = c.DescriptionCollection,
                    Rating = positiveRating,
                    Books = c.IdBooks
                        .Select(bc => new BookDTO
                        {
                            Id = bc.IdBook,
                            Title = bc.TitleBook,
                            ImageURL = bc.ImageUrl
                        })
                        .ToList(),
                    CreatedDate = c.CreatedDate,
                    UserName = c.IdUserNavigation?.NameUser,
                    UserURL = c.IdUserNavigation?.ProfileImageUrl
                });
            }

            return result;
        }

        public async Task<List<BookDTO>> GetBookRecommendationsAsync(int idUser)
        {
            var pythonRecommendations = await GetPythonBookRecommendationsAsync(idUser);

            if (!pythonRecommendations.Any()) { return new List<BookDTO>(); }

            var bookIds = pythonRecommendations
                .Select(r => r.book_id).ToList();

            var booksFromDb = await _context.Books
                .Where(b => bookIds.Contains(b.IdBook))
                .ToListAsync();

            var result = new List<BookDTO>();
            foreach (var book in booksFromDb)
            {
                result.Add(new BookDTO
                {
                    Id = book.IdBook,
                    Title = book.TitleBook,
                    ImageURL = book.ImageUrl,
                    AverageRating = await GetAverageRatingAsync(book.IdBook)
                });
            }

            return result.OrderBy(b => bookIds.IndexOf(b.Id)).ToList();
        }

        public async Task<List<BookDTO>> GetSimilarBooksAsync(int idBook)
        {
            var pythonSimilars = await GetPythonSimilarBooks(idBook);
            if (!pythonSimilars.Any()) { return new List<BookDTO>(); }

            var booksIds = pythonSimilars
                .Select(r => r.book_id).ToList();

            var booksFromDb = await _context.Books
                .Where(b => booksIds.Contains(b.IdBook))
                .ToListAsync();

            var result = new List<BookDTO>();
            foreach (var book in booksFromDb)
            {
                result.Add(new BookDTO
                {
                    Id = book.IdBook,
                    Title = book.TitleBook,
                    ImageURL = book.ImageUrl,
                    AverageRating = await GetAverageRatingAsync(book.IdBook)
                });
            }

            return result;
        }

        private async Task<List<PythonBook>> GetPythonBookRecommendationsAsync(int idUser)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:8000/recommend/{idUser}");

            if (!response.IsSuccessStatusCode)
                return new List<PythonBook>();

            return await response.Content.ReadFromJsonAsync<List<PythonBook>>();
        }

        private async Task<List<PythonBook>> GetPythonSimilarBooks(int idBook)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://127.0.0.1:8000/similar_books/{idBook}?top_n=5");

            if (!response.IsSuccessStatusCode)
                return new List<PythonBook>();

            return await response.Content.ReadFromJsonAsync<List<PythonBook>>();
        }

        private record PythonBook(int book_id, string title, string category);
    }
}
