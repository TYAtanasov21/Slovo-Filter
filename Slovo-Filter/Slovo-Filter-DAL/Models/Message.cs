namespace Slovo_Filter_DAL.Models;

public class Message
{
    public Int32 Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsDelivered { get; set; }
    public bool IsRead { get; set; }
    public bool IsFromCurrentUser { get; set; }

}