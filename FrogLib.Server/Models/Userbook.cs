namespace FrogLib.Server.Models;

public partial class Userbook
{
    public int IdUser { get; set; }

    public int IdBook { get; set; }

    public string ListType { get; set; } = null!;

    public DateOnly AddedDate { get; set; }

    public virtual Book IdBookNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
