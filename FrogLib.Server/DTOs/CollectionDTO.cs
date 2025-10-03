namespace FrogLib.Server.DTOs
{
    public class CollectionDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int CountBooks { get; set; }
        public List<BookDTO> Books { get; set; }
        public int CountView { get; set; }
        public int CountComments { get; set; }
        public int CountLiked { get; set; }
        public DateOnly CreatedDate { get; set; }
        public string UserName { get; set; }
        public string UserURL { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }
}
