using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var task = await _taskService.CreateTaskAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, new { message = "Success", data = task });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var tasks = await _taskService.GetMyTasksAsync(userId.Value);
        return Ok(new { message = "Success", data = tasks });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var task = await _taskService.GetTaskByIdAsync(id, userId.Value);
        if (task == null) return NotFound(new { message = "Task not found or access denied" });

        return Ok(new { message = "Success", data = task });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var task = await _taskService.UpdateTaskStatusAsync(id, userId.Value, request);
        if (task == null) return NotFound(new { message = "Task not found or access denied" });

        return Ok(new { message = "Success", data = task });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _taskService.DeleteTaskAsync(id, userId.Value);
        if (!success) return NotFound(new { message = "Task not found or access denied" });

        return Ok(new { message = "Success", data = new { } });
    }

    private Guid? GetUserId()
    {
        var userIdString = User.FindFirstValue("UserId");
        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }
}
