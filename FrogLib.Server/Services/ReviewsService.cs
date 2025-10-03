using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Services
{
    public interface IReviewsService
    {
        Task<RatingInfo> GetRatingAsync(int idReview);
        Task<int> GetCountViewAsync(int idReview);
        Task<int> GetCountCommentsAsync(int idReview);
        Task<int> GetBookUserRatingAsync(int idBook, int idUser);
        Task<BookDTO> GetBookForReviewAsync(int idReview);
        Task<List<CommentDTO>> GetCommentsForReviewAsync(int idReview);
    }

    public class ReviewsService(Froglib2Context context, Lazy<IBooksService> booksService) : BaseEntityService(context, "Рецензия"), IReviewsService
    {
        private readonly Froglib2Context _context = context;
        private readonly Lazy<IBooksService> _booksService = booksService;

        public async Task<int> GetBookUserRatingAsync(int idBook, int idUser)
        {
            var rating = await _context.Bookratings
                .FirstOrDefaultAsync(br => br.IdBook == idBook && br.IdUser == idUser);

            return rating?.Rating ?? 0;
        }

        public async Task<BookDTO> GetBookForReviewAsync(int idReview)
        {
            var review = await _context.Reviews
                .Include(r => r.IdBookNavigation)
                .FirstOrDefaultAsync(r => r.IdReview == idReview);

            if (review == null) { return new BookDTO(); }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.IdBook == review.IdBook);

            var averageRatingBook = await _booksService.Value.GetAverageRatingAsync(book.IdBook);

            var authors = await _booksService.Value.GetAuthorsFullNameAsync(book.IdBook);

            return new BookDTO
            {
                Id = book.IdBook,
                Title = book.TitleBook,
                ImageURL = book.ImageUrl,
                AverageRating = averageRatingBook,
                Authors = authors
            };

        }

        public Task<List<CommentDTO>> GetCommentsForReviewAsync(int idReview)
        {
            return GetCommentsAsync(idReview);
        }
    }
}
