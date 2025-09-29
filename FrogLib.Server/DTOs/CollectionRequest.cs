namespace FrogLib.Server.DTOs
{
    public class CollectionRequest
    {
        public int IdUser { get; set; }
        public string TitleCollection { get; set; }
        public string DescriptionCollection { get; set; }
        public string PlaintDescription { get; set; }
        public List<int> IdBooks { get; set; }
    }
}
