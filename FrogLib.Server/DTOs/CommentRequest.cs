namespace FrogLib.Server.DTOs
{
    public class CommentRequest
    {
        public int IdUser { get; set; }
        public int IdEntity { get; set; }
        public string TypeEntity { get; set; }
        public string Text { get; set; }
        public int? IdParentComment { get; set; }
    }
}
