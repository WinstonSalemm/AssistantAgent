namespace Assistant.Core.Entities;

public class Reminder
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime RemindAt { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
