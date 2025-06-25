namespace FrogLib.Server.Models;

public partial class Bookrating
{
    public int IdRating { get; set; }

    public int IdUser { get; set; }

    public int IdBook { get; set; }

    public int Rating { get; set; }

    public virtual Book IdBookNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
