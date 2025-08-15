namespace FrogLib.Server.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public BookDTO Book { get; set; }
        public int UserRating { get; set; }
        public double PositiveRating { get; set; }
        public int CountView { get; set; }
        public DateOnly CreatedDate { get; set; }
        public string UserName { get; set; }
        public string UserURL { get; set; }
        //public List<CommentDTO> Comments { get; set; }
    }
}
