namespace FrogLib.Server.Models;

public partial class Author
{
    public int IdAuthor { get; set; }

    public string SurnameAuthor { get; set; } = null!;

    public string NameAuthor { get; set; } = null!;

    public string? PatronymicAuthor { get; set; }

    public virtual ICollection<Book> IdBooks { get; set; } = new List<Book>();
}
