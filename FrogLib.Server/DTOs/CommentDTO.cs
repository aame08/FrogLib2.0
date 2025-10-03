namespace FrogLib.Server.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorURL { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string Status { get; set; }
        public bool IsReply { get; set; }
        public List<CommentDTO> Replies { get; set; }
        public RatingInfo Rating { get; set; }
    }
}
