namespace FrogLib.Server.Models;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int IdRecipient { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string MessageNotification { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public bool IsRead { get; set; }

    public virtual User IdRecipientNavigation { get; set; } = null!;
}
