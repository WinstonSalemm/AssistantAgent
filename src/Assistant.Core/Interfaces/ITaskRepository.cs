using Assistant.Core.Entities;

namespace Assistant.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetActiveTasksAsync();
    Task<IEnumerable<TaskEntity>> GetCompletedTasksAsync();
    Task<IEnumerable<TaskEntity>> GetTasksDueTodayAsync();
}
