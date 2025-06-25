namespace FrogLib.Server.Models;

public partial class Entityrating
{
    public int IdRating { get; set; }

    public int IdUser { get; set; }

    public int IdEntity { get; set; }

    public string TypeEntity { get; set; } = null!;

    public sbyte IsLike { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
