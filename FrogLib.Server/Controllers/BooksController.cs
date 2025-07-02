using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(Froglib2Context context, IBooksService service) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly IBooksService _service = service;

        [HttpGet("new-books")]
        public async Task<ActionResult<List<BookDTO>>> GetNewBooksAsync()
        {
            try
            {
                var books = await _context.Books.ToListAsync();

                var newBooks = books
                    .Where(book => (DateOnly.FromDateTime(DateTime.Now).DayNumber - book.AddedDate.DayNumber <= 30))
                    .Select(book => new BookDTO
                    {
                        Id = book.IdBook,
                        Title = book.TitleBook,
                        ImageURL = book.ImageUrl,
                        AverageRating = _service.GetAverageRatingAsync(book.IdBook).Result
                    })
                    .ToList();

                return Ok(newBooks);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("best-selling")]
        public async Task<ActionResult<List<BookDTO>>> GetBestSellingAsync()
        {
            try
            {
                var books = await _context.Books.ToListAsync();

                var bestSellingBooks = new List<BookDTO>();

                foreach (var book in books)
                {
                    var viewCount = await _service.GetCountViewAsync(book.IdBook);
                    if (viewCount >= 5)
                    {
                        bestSellingBooks.Add(new BookDTO
                        {
                            Id = book.IdBook,
                            Title = book.TitleBook,
                            ImageURL = book.ImageUrl,
                            AverageRating = await _service.GetAverageRatingAsync(book.IdBook)
                        });
                    }
                }

                return Ok(bestSellingBooks);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        [HttpGet("popular-books")]
        public async Task<ActionResult<List<BookDTO>>> GetPopularBooksAsync()
        {
            try
            {
                var books = await _context.Books
                    .Include(b => b.Bookratings)
                    .Include(b => b.Userbooks)
                    .ToListAsync();

                var popularBooks = books.Select(book => new BookDTO
                {
                    Id = book.IdBook,
                    Title = book.TitleBook,
                    ImageURL = book.ImageUrl,
                    AverageRating = _service.GetAverageRatingAsync(book.IdBook).Result,
                    TotalRatings = book.Bookratings?.Count ?? 0,
                    TotalUserBookmarks = book.Userbooks?.Count ?? 0,
                    CountView = _service.GetCountViewAsync(book.IdBook).Result
                })
                .OrderByDescending(b => b.AverageRating)
                    .ThenByDescending(b => b.TotalRatings)
                    .ThenByDescending(b => b.TotalUserBookmarks)
                    .ThenByDescending(b => b.CountView)
                .Take(10)
                .ToList();

                return Ok(popularBooks);
            }
            catch (Exception ex) { return HandleException(ex); }
        }
    }
}
