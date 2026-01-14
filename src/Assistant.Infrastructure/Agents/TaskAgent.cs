using Assistant.Core.Entities;
using Assistant.Core.Enums;
using Assistant.Core.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Assistant.Infrastructure.Agents;

public class TaskAgent : IAgent
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAIService _aiService;

    public AgentType Type => AgentType.Task;

    public TaskAgent(ITaskRepository taskRepository, IAIService aiService)
    {
        _taskRepository = taskRepository;
        _aiService = aiService;
    }

    public Task<bool> CanHandleAsync(string input)
    {
        var keywords = new[] { "задач", "todo", "сделать", "выполнить", "список", "дела" };
        return Task.FromResult(keywords.Any(k => input.ToLower().Contains(k)));
    }

    public async Task<string> ProcessAsync(string input, Dictionary<string, object>? context = null)
    {
        input = input.ToLower();

        // Создание задачи
        if (input.Contains("добав") || input.Contains("созда") || input.Contains("новая задача"))
        {
            return await CreateTaskAsync(input);
        }

        // Список задач
        if (input.Contains("список") || input.Contains("покажи") || input.Contains("какие задачи"))
        {
            if (input.Contains("завершен") || input.Contains("выполнен"))
            {
                return await ListCompletedTasksAsync();
            }
            return await ListActiveTasksAsync();
        }

        // Задачи на сегодня
        if (input.Contains("сегодня") || input.Contains("на сегодня"))
        {
            return await ListTodayTasksAsync();
        }

        // Отметить как выполненную
        if (input.Contains("выполн") || input.Contains("завершить") || input.Contains("готово"))
        {
            return await CompleteTaskAsync(input);
        }

        return await ListActiveTasksAsync();
    }

    private async Task<string> CreateTaskAsync(string input)
    {
        try
        {
            var systemPrompt = @"Extract task information from user input. Return JSON with fields:
- title: task title (required, string)
- priority: Low/Medium/High (default: Medium)
- dueDate: ISO 8601 date if mentioned, null otherwise

Example input: 'добавь задачу купить молоко завтра'
Example output: {""title"":""купить молоко"",""priority"":""Medium"",""dueDate"":""2024-01-15""}

Return ONLY valid JSON.";

            var response = await _aiService.GenerateResponseAsync(input, systemPrompt);
            
            // Парсим JSON ответ (упрощенно)
            var title = ExtractJsonField(response, "title");
            var priorityStr = ExtractJsonField(response, "priority") ?? "Medium";
            var dueDateStr = ExtractJsonField(response, "dueDate");

            if (string.IsNullOrEmpty(title))
            {
                return "Не удалось понять название задачи. Попробуйте: 'добавь задачу [название]'";
            }

            var priority = priorityStr?.ToLower() switch
            {
                "low" => TaskPriority.Low,
                "high" => TaskPriority.High,
                _ => TaskPriority.Medium
            };

            DateTime? dueDate = null;
            if (!string.IsNullOrEmpty(dueDateStr) && DateTime.TryParse(dueDateStr, out var parsedDate))
            {
                dueDate = parsedDate;
            }

            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = title,
                Priority = priority,
                DueDate = dueDate,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);

            var dueDateText = dueDate.HasValue ? $" (срок: {dueDate.Value:dd.MM.yyyy})" : "";
            return $"✅ Задача добавлена: '{title}'{dueDateText}";
        }
        catch (Exception ex)
        {
            return $"Ошибка при создании задачи: {ex.Message}";
        }
    }

    private async Task<string> ListActiveTasksAsync()
    {
        var tasks = await _taskRepository.GetActiveTasksAsync();
        
        if (!tasks.Any())
        {
            return "У вас нет активных задач! 🎉";
        }

        var sb = new StringBuilder("📋 **Активные задачи:**\n\n");
        
        foreach (var task in tasks)
        {
            var priorityIcon = task.Priority switch
            {
                TaskPriority.High => "🔴",
                TaskPriority.Medium => "🟡",
                TaskPriority.Low => "🟢",
                _ => "⚪"
            };

            var dueDate = task.DueDate.HasValue 
                ? $" (до {task.DueDate.Value:dd.MM.yyyy})" 
                : "";

            sb.AppendLine($"{priorityIcon} {task.Title}{dueDate}");
        }

        return sb.ToString();
    }

    private async Task<string> ListCompletedTasksAsync()
    {
        var tasks = await _taskRepository.GetCompletedTasksAsync();
        
        if (!tasks.Any())
        {
            return "Нет завершенных задач.";
        }

        var sb = new StringBuilder("✅ **Завершенные задачи:**\n\n");
        
        foreach (var task in tasks.Take(10))
        {
            var completedDate = task.CompletedAt.HasValue 
                ? $" (завершено {task.CompletedAt.Value:dd.MM.yyyy})" 
                : "";

            sb.AppendLine($"• {task.Title}{completedDate}");
        }

        return sb.ToString();
    }

    private async Task<string> ListTodayTasksAsync()
    {
        var tasks = await _taskRepository.GetTasksDueTodayAsync();
        
        if (!tasks.Any())
        {
            return "На сегодня задач нет! 🎉";
        }

        var sb = new StringBuilder("📅 **Задачи на сегодня:**\n\n");
        
        foreach (var task in tasks)
        {
            sb.AppendLine($"• {task.Title}");
        }

        return sb.ToString();
    }

    private async Task<string> CompleteTaskAsync(string input)
    {
        var tasks = await _taskRepository.GetActiveTasksAsync();
        
        if (!tasks.Any())
        {
            return "Нет активных задач для завершения.";
        }

        // Пытаемся найти задачу по названию в input
        var matchedTask = tasks.FirstOrDefault(t => 
            input.ToLower().Contains(t.Title.ToLower()));

        if (matchedTask == null)
        {
            // Берем первую задачу
            matchedTask = tasks.First();
        }

        matchedTask.IsCompleted = true;
        matchedTask.CompletedAt = DateTime.UtcNow;
        
        await _taskRepository.UpdateAsync(matchedTask);

        return $"✅ Задача завершена: '{matchedTask.Title}'";
    }

    private string? ExtractJsonField(string json, string fieldName)
    {
        var pattern = $"\"{fieldName}\"\\s*:\\s*\"([^\"]+)\"";
        var match = Regex.Match(json, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }
}
