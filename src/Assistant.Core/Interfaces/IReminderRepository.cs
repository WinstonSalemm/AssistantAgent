using Assistant.Core.Entities;

namespace Assistant.Core.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
    Task<IEnumerable<Reminder>> GetActiveRemindersAsync();
    Task<IEnumerable<Reminder>> GetDueRemindersAsync();
}
