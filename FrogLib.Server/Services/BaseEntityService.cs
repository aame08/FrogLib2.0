using FrogLib.Server.DTOs;
using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Services
{
    public abstract class BaseEntityService(Froglib2Context context, string typeObject)
    {
        private readonly Froglib2Context _context = context;
        private readonly string _typeObject = typeObject;

        public async Task<RatingInfo> GetRatingAsync(int idEntity)
        {
            var totalRatings = await _context.Entityratings
                .Where(er => er.TypeEntity == _typeObject && er.IdEntity == idEntity)
                .ToListAsync();

            if (!totalRatings.Any())
            {
                return new RatingInfo
                {
                    Rating = 0,
                    Likes = 0,
                    Dislikes = 0
                };
            }

            var likes = totalRatings.Count(er => er.IsLike == 1);
            var dislikes = totalRatings.Count(er => er.IsLike == 0);
            var rating = likes - dislikes;

            return new RatingInfo
            {
                Rating = rating,
                Likes = likes,
                Dislikes = dislikes
            };
        }

        public async Task<int> GetCountViewAsync(int idEntity)
        {
            return await _context.Viewhistories
                .CountAsync(v => v.TypeEntity == _typeObject && v.IdEntity == idEntity);
        }

        public async Task<int> GetCountCommentsAsync(int idEntity)
        {
            return await _context.Comments
                .CountAsync(c => c.TypeEntity == _typeObject && c.IdEntity == idEntity);
        }

        public async Task<List<CommentDTO>> GetCommentsAsync(int idEntity)
        {
            var allComments = await _context.Comments
                .Where(c => c.TypeEntity == _typeObject && c.IdEntity == idEntity)
                .Include(c => c.IdUserNavigation)
                .ToListAsync();

            var commentIds = allComments.Select(c => c.IdComment).ToList();

            var commentRatings = await _context.Entityratings
                .Where(er => er.TypeEntity == "Комментарий" && commentIds.Contains(er.IdEntity))
                .ToListAsync();

            var ratingsByComment = commentRatings
                .GroupBy(er => er.IdEntity)
                .ToDictionary(
                    g => g.Key,
                    g => new RatingInfo
                    {
                        Likes = g.Count(er => er.IsLike == 1),
                        Dislikes = g.Count(er => er.IsLike == 0),
                        Rating = g.Count(er => er.IsLike == 1) - g.Count(er => er.IsLike == 0)
                    }
                );

            var commentsById = allComments.ToDictionary(c => c.IdComment);

            var rootComments = allComments.Where(c => c.ParentCommentId == null).ToList();
            var commentDTOs = rootComments.Select(c => BuildCommentTree(c, commentsById, ratingsByComment)).ToList();

            return commentDTOs;
        }

        private CommentDTO BuildCommentTree(Comment comment, Dictionary<int, Comment> commentsById, Dictionary<int, RatingInfo> ratingsByComment)
        {
            var ratingInfo = ratingsByComment.GetValueOrDefault(comment.IdComment, new RatingInfo
            {
                Rating = 0,
                Likes = 0,
                Dislikes = 0
            });

            var replies = commentsById.Values
                .Where(c => c.ParentCommentId == comment.IdComment)
                .Select(c => BuildCommentTree(c, commentsById, ratingsByComment))
                .ToList();

            return new CommentDTO
            {
                Id = comment.IdComment,
                AuthorId = comment.IdUserNavigation.IdUser,
                AuthorName = comment.IdUserNavigation.NameUser,
                AuthorURL = comment.IdUserNavigation.ProfileImageUrl,
                Date = comment.DateComment,
                Text = comment.TextComment,
                Status = comment.StatusComment,
                IsReply = comment.ParentCommentId != null,
                Replies = replies,
                Rating = ratingInfo
            };
        }
    }
}
