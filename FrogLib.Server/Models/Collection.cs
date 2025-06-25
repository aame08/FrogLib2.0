namespace FrogLib.Server.Models;

public partial class Collection
{
    public int IdCollection { get; set; }

    public int IdUser { get; set; }

    public string TitleCollection { get; set; } = null!;

    public string? DescriptionCollection { get; set; }

    public DateOnly CreatedDate { get; set; }

    public string StatusCollection { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual ICollection<Likedcollection> Likedcollections { get; set; } = new List<Likedcollection>();

    public virtual ICollection<Book> IdBooks { get; set; } = new List<Book>();
}
