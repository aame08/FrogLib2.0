namespace FrogLib.Server.Models;

public partial class Book
{
    public int IdBook { get; set; }

    public string Isbn13 { get; set; } = null!;

    public int IdPublisher { get; set; }

    public int IdCategory { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string PreviewLink { get; set; } = null!;

    public string TitleBook { get; set; } = null!;

    public string DescriptionBook { get; set; } = null!;

    public short YearPublication { get; set; }

    public int PageCount { get; set; }

    public string LanguageBook { get; set; } = null!;

    public DateOnly AddedDate { get; set; }

    public virtual ICollection<Bookrating> Bookratings { get; set; } = new List<Bookrating>();

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual Publisher IdPublisherNavigation { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();

    public virtual ICollection<Author> IdAuthors { get; set; } = new List<Author>();

    public virtual ICollection<Collection> IdCollections { get; set; } = new List<Collection>();
}
