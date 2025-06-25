namespace FrogLib.Server.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string NameRole { get; set; } = null!;

    public string NameUser { get; set; } = null!;

    public string LoginUser { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public DateOnly RegistrationDate { get; set; }

    public string StatusUser { get; set; } = null!;

    public virtual ICollection<Bookrating> Bookratings { get; set; } = new List<Bookrating>();

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Entityrating> Entityratings { get; set; } = new List<Entityrating>();

    public virtual ICollection<Likedcollection> Likedcollections { get; set; } = new List<Likedcollection>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Userbook> Userbooks { get; set; } = new List<Userbook>();

    public virtual ICollection<Viewhistory> Viewhistories { get; set; } = new List<Viewhistory>();

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
