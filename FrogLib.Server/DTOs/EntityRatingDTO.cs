namespace FrogLib.Server.DTOs
{
    public class EntityRatingDTO
    {
        public int IdUser { get; set; }
        public int IdEntity { get; set; }
        public string TypeEntity { get; set; }
        public int? Rating { get; set; }
    }
}
