namespace FrogLib.Server.Models;

public partial class Violation
{
    public int IdViolation { get; set; }

    public int IdUser { get; set; }

    public string CategoryViolation { get; set; } = null!;

    public string DescriptionViolation { get; set; } = null!;

    public DateOnly DateViolation { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
