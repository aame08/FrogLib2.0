namespace FrogLib.Server.Models;

public partial class Publisher
{
    public int IdPublisher { get; set; }

    public string NamePublisher { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
