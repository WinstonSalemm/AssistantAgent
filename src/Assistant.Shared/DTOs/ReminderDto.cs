namespace Assistant.Shared.DTOs;

public class ReminderDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime RemindAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReminderRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTime RemindAt { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
}

public class UpdateReminderRequest
{
    public string? Title { get; set; }
    public DateTime? RemindAt { get; set; }
    public bool? IsCompleted { get; set; }
}
