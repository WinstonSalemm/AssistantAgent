using Assistant.Core.Entities;
using Assistant.Core.Interfaces;
using Assistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Assistant.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskEntity>, ITaskRepository
{
    public TaskRepository(AssistantDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskEntity>> GetActiveTasksAsync()
    {
        return await _dbSet
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetCompletedTasksAsync()
    {
        return await _dbSet
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetTasksDueTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        return await _dbSet
            .Where(t => !t.IsCompleted && t.DueDate >= today && t.DueDate < tomorrow)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }
}
