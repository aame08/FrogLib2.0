namespace FrogLib.Server.Models;

public partial class Category
{
    public int IdCategory { get; set; }

    public string NameCategoryEng { get; set; } = null!;

    public string NameCategoryRu { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
