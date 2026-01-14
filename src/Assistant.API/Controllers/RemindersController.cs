using Assistant.Core.Entities;
using Assistant.Core.Interfaces;
using Assistant.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly IReminderRepository _reminderRepository;
    private readonly ILogger<RemindersController> _logger;

    public RemindersController(IReminderRepository reminderRepository, ILogger<RemindersController> logger)
    {
        _reminderRepository = reminderRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReminderDto>>> GetAll()
    {
        try
        {
            var reminders = await _reminderRepository.GetAllAsync();
            var reminderDtos = reminders.Select(MapToDto);
            return Ok(reminderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminders");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReminderDto>> GetById(Guid id)
    {
        try
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();

            return Ok(MapToDto(reminder));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder by id");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ReminderDto>>> GetActive()
    {
        try
        {
            var reminders = await _reminderRepository.GetActiveRemindersAsync();
            var reminderDtos = reminders.Select(MapToDto);
            return Ok(reminderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active reminders");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ReminderDto>> Create([FromBody] CreateReminderRequest request)
    {
        try
        {
            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                RemindAt = request.RemindAt,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern,
                CreatedAt = DateTime.UtcNow
            };

            await _reminderRepository.AddAsync(reminder);

            return CreatedAtAction(nameof(GetById), new { id = reminder.Id }, MapToDto(reminder));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminder");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateReminderRequest request)
    {
        try
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();

            if (request.Title != null)
                reminder.Title = request.Title;

            if (request.RemindAt.HasValue)
                reminder.RemindAt = request.RemindAt.Value;

            if (request.IsCompleted.HasValue)
                reminder.IsCompleted = request.IsCompleted.Value;

            await _reminderRepository.UpdateAsync(reminder);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reminder");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();

            await _reminderRepository.DeleteAsync(reminder);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reminder");
            return StatusCode(500, "Internal server error");
        }
    }

    private static ReminderDto MapToDto(Reminder reminder)
    {
        return new ReminderDto
        {
            Id = reminder.Id,
            Title = reminder.Title,
            RemindAt = reminder.RemindAt,
            IsCompleted = reminder.IsCompleted,
            IsRecurring = reminder.IsRecurring,
            RecurrencePattern = reminder.RecurrencePattern,
            CreatedAt = reminder.CreatedAt
        };
    }
}
