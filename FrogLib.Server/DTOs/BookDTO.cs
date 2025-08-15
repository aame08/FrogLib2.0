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

    public class BookRatingStatisticsDTO
    {
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public List<RatingDistribution> RatingDistribution { get; set; }
    }
    public class RatingDistribution
    {
        public int RatingValue { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class BookmarksStatisticsDTO
    {
        public int TotalBookmarks { get; set; }
        public List<BookmarkDistribution> BookmarkDistribution { get; set; }
    }
    public class BookmarkDistribution
    {
        public string ListType { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
