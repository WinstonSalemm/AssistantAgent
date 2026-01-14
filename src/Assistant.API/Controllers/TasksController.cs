using Assistant.Core.Entities;
using Assistant.Core.Enums;
using Assistant.Core.Interfaces;
using Assistant.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Assistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TasksController> _logger;
    private readonly IConfiguration _configuration;

    public TasksController(
        ITaskRepository taskRepository, 
        ILogger<TasksController> logger,
        IConfiguration configuration)
    {
        _taskRepository = taskRepository;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        try
        {
            var tasks = await _taskRepository.GetAllAsync();
            var taskDtos = tasks.Select(MapToDto);
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound();

            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task by id");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetActive()
    {
        try
        {
            var tasks = await _taskRepository.GetActiveTasksAsync();
            var taskDtos = tasks.Select(MapToDto);
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active tasks");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("completed")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetCompleted()
    {
        try
        {
            var tasks = await _taskRepository.GetCompletedTasksAsync();
            var taskDtos = tasks.Select(MapToDto);
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed tasks");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request)
    {
        try
        {
            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Priority = (TaskPriority)request.Priority,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            
            // В Development показываем детали ошибки для отладки
            var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
            
            if (isDevelopment)
            {
                return StatusCode(500, new { 
                    error = "Internal server error", 
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
            
            // В Production возвращаем более информативную ошибку (без stack trace)
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                hint = "Check Railway logs for details"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound();

            if (request.Title != null)
                task.Title = request.Title;

            if (request.Description != null)
                task.Description = request.Description;

            if (request.IsCompleted.HasValue)
            {
                task.IsCompleted = request.IsCompleted.Value;
                if (task.IsCompleted && !task.CompletedAt.HasValue)
                    task.CompletedAt = DateTime.UtcNow;
            }

            if (request.Priority.HasValue)
                task.Priority = (TaskPriority)request.Priority.Value;

            if (request.DueDate.HasValue)
                task.DueDate = request.DueDate;

            await _taskRepository.UpdateAsync(task);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound();

            await _taskRepository.DeleteAsync(task);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return StatusCode(500, "Internal server error");
        }
    }

    private static TaskDto MapToDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = (int)task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt
        };
    }
}
