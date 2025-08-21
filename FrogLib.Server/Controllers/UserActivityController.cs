using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using FrogLib.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController(Froglib2Context context, IUsersService service) : BaseController
    {
        private readonly Froglib2Context _context = context;
        private readonly IUsersService _service = service;

        [HttpGet("get-statistics")]
        public async Task<ActionResult<StatisticInfo>> GetStatisticsAsync()
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

        [Authorize(Roles = "Пользователь")]
        [HttpPost("add-view/{idUser}/{typeEntity}/{idEntity}")]
        public async Task<ActionResult> AddViewAsync(int idUser, string typeEntity, int idEntity)
        {
            try
            {
                if (!_service.IsAuthorizedUser(User, idUser) || !await _service.UserExistsAsync(idUser))
                {
                    return Conflict("Некорретные данные пользователя.");
                }

                if (!await _service.ObjectExistsAsync(typeEntity, idEntity))
                {
                    return NotFound($"{typeEntity} не найдена.");
                }

                var existingView = await _context.Viewhistories
                    .FirstOrDefaultAsync(v => v.IdUser == idUser && v.IdEntity == idEntity && v.TypeEntity == typeEntity);

                if (existingView == null)
                {
                    var newView = new Viewhistory
                    {
                        IdUser = idUser,
                        IdEntity = idEntity,
                        TypeEntity = typeEntity,
                        ViewDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.Viewhistories.Add(newView);
                    await _context.SaveChangesAsync();

                    return Ok("Просмотр добавлен.");
                }

                return Ok("Просмотр уже существует.");
            }
            catch (Exception ex) { return HandleException(ex); }
        }

    }
}
