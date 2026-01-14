using Assistant.Core.Entities;
using Assistant.Core.Interfaces;
using Assistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assistant.Infrastructure.Repositories;

public class ReminderRepository : Repository<Reminder>, IReminderRepository
{
    public ReminderRepository(AssistantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reminder>> GetActiveRemindersAsync()
    {
        return await _dbSet
            .Where(r => !r.IsCompleted)
            .OrderBy(r => r.RemindAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reminder>> GetDueRemindersAsync()
    {
        var now = DateTime.UtcNow;
        
        return await _dbSet
            .Where(r => !r.IsCompleted && r.RemindAt <= now)
            .ToListAsync();
    }
}
