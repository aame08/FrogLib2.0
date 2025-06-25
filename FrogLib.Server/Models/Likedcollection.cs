namespace FrogLib.Server.Models;

public partial class Likedcollection
{
    public int IdCollection { get; set; }

    public int IdUser { get; set; }

    public DateOnly LikedDate { get; set; }

    public virtual Collection IdCollectionNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
