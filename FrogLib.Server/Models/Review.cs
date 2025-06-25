namespace FrogLib.Server.Models;

public partial class Review
{
    public int IdReview { get; set; }

    public int IdUser { get; set; }

    public int IdBook { get; set; }

    public string TitleReview { get; set; } = null!;

    public string TextReview { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public string StatusReview { get; set; } = null!;

    public virtual Book IdBookNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
