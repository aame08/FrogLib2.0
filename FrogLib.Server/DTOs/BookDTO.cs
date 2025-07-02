namespace FrogLib.Server.DTOs
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageURL { get; set; }
        public double AverageRating { get; set; }
        public int IdCategory { get; set; }
        public short YearPublication { get; set; }
        public string Authors { get; set; }
        public int TotalRatings { get; set; }
        public int TotalUserBookmarks { get; set; }
        public int CountView { get; set; }
    }
}
