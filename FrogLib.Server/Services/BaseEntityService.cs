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
                .CountAsync(er => er.TypeEntity == _typeObject && er.IdEntity == idEntity);
            if (totalRatings == 0)
            {
                return new RatingInfo
                {
                    PositivePercent = 0,
                    Likes = 0,
                    Dislikes = 0
                };
            }

            var likes = await _context.Entityratings
                .CountAsync(er => er.TypeEntity == _typeObject && er.IdEntity == idEntity && er.IsLike == 1);

            var dislikes = totalRatings - likes;

            return new RatingInfo
            {
                PositivePercent = (likes * 100.0) / totalRatings,
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

        //public async Task<List<CommentDTO>> GetCommentsAsync(int idEntity)
        //{
        //    var allComments = await _context.Comments
        //        .Where(c => c.TypeEntity == _typeObject && c.IdEntity == idEntity)
        //        .Include(c => c.IdUserNavigation)
        //        .ToListAsync();

        //    var comments = allComments
        //        .Where(c => c.ParentCommentId == null)
        //        .Select(c => MapComment(c, allComments))
        //        .ToList();

        //    return comments;
        //}

        //private CommentDTO MapComment(Comment comment, List<Comment> allComments)
        //{
        //    return new CommentDTO
        //    {
        //        Id = comment.IdComment,
        //        Author = comment.IdUserNavigation.NameUser,
        //        AuthorURL = comment.IdUserNavigation.ProfileImageUrl,
        //        Date = comment.DateComment,
        //        Content = comment.TextComment,
        //        Status = comment.StatusComment,
        //        IsReply = comment.ParentCommentId != null,
        //        Replies = allComments
        //            .Where(r => r.ParentCommentId == comment.IdComment)
        //            .Select(r => MapComment(r, allComments))
        //            .ToList()
        //    };
        //}
    }
}
