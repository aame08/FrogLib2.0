namespace FrogLib.Server.Models;

public partial class Viewhistory
{
    public int IdUser { get; set; }

    public int IdEntity { get; set; }

    public string TypeEntity { get; set; } = null!;

    public DateOnly ViewDate { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
