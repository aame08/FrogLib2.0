namespace FrogLib.Server.DTOs
{
    public class ReviewRequest
    {
        public int IdUser { get; set; }
        public int IdBook { get; set; }
        public string TitleReview { get; set; }
        public string ContentReview {  get; set; }
        public string PlaintText { get; set; }
    }
}
