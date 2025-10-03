using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FrogLib.Server.Services
{
    public interface IUsersService
    {
        int? GetCurrentUserId(ClaimsPrincipal user);
        bool IsAuthorizedUser(ClaimsPrincipal user, int idUser);
        Task<bool> UserExistsAsync(int idUser);
        Task<string> GetRoleUserAsync(int idUser);
        Task<bool> ObjectExistsAsync(string typeObject, int idObject);
    }

    public class UsersService(Froglib2Context context) : IUsersService
    {
        private readonly Froglib2Context _context = context;

        public int? GetCurrentUserId(ClaimsPrincipal user)
        {
            var idUserClaim = user.FindFirst("id_user");
            if (idUserClaim != null && int.TryParse(idUserClaim.Value, out int idUser)) { return idUser; }

            return null;
        }

        public bool IsAuthorizedUser(ClaimsPrincipal user, int idUser)
        {
            var currentIdUser = GetCurrentUserId(user);
            return currentIdUser.HasValue && currentIdUser.Value == idUser;
        }

        public async Task<bool> UserExistsAsync(int idUser)
        {
            return await _context.Users.AnyAsync(u => u.IdUser == idUser);
        }

        public async Task<string> GetRoleUserAsync(int idUser)
        {
            if (!await UserExistsAsync(idUser))
            {
                return "";
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == idUser);

            return user.NameRole;
        }

        public async Task<bool> ObjectExistsAsync(string typeObject, int idObject)
        {
            switch (typeObject)
            {
                case "Книга":
                    return await _context.Books.AnyAsync(b => b.IdBook == idObject);
                case "Рецензия":
                    return await _context.Reviews.AnyAsync(r => r.IdReview == idObject);
                case "Подборка":
                    return await _context.Collections.AnyAsync(c => c.IdCollection == idObject);
                case "Комментарий":
                    return await _context.Comments.AnyAsync(c => c.IdComment == idObject);
                default:
                    return false;
            }
        }
    }
}
