using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskQueue _taskQueue;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository, 
        ITaskQueue taskQueue,
        IDistributedCache cache,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _taskQueue = taskQueue;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TaskResponse> CreateTaskAsync(Guid userId, CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required");
        }

        // Prevent duplicate tasks: same user, same title, same day
        var today = DateTime.UtcNow.Date;
        var isDuplicate = await _taskRepository.GetQueryable()
            .AnyAsync(t => t.UserId == userId && 
                           t.Title == request.Title && 
                           t.CreatedAt.Date == today);

        if (isDuplicate)
        {
            throw new InvalidOperationException("A task with the same title already exists for today.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _taskRepository.AddAsync(task);
        await _taskQueue.EnqueueAsync(task);

        return MapToDto(task);
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(Guid id, Guid userId)
    {
        var cacheKey = $"task:{id}";
        TaskItem? task = null;

        try
        {
            var cachedTask = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedTask))
            {
                task = JsonSerializer.Deserialize<TaskItem>(cachedTask);
                _logger.LogInformation("Cache hit for task {TaskId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache is unavailable. Falling back to database for task {TaskId}", id);
        }

        if (task == null)
        {
            task = await _taskRepository.GetByIdAsync(id);
            if (task != null)
            {
                try
                {
                    var serializedTask = JsonSerializer.Serialize(task);
                    await _cache.SetStringAsync(
                        cacheKey, 
                        serializedTask, 
                        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
                    _logger.LogInformation("Cache miss. Task {TaskId} loaded from database and cached.", id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache task {TaskId}. Redis cache might be unavailable.", id);
                }
            }
        }
        
        if (task == null || task.UserId != userId)
        {
            return null; // Controller will handle 404/403
        }

        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskResponse>> GetMyTasksAsync(Guid userId)
    {
        // Compose the query to filter and sort before execution
        var tasks = await _taskRepository.GetQueryable()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskResponse?> UpdateTaskStatusAsync(Guid id, Guid userId, UpdateTaskStatusRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        
        if (task == null || task.UserId != userId)
        {
            return null;
        }

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);

        try
        {
            await _cache.RemoveAsync($"task:{id}");
            _logger.LogInformation("Cache invalidated for task {TaskId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for task {TaskId}. Redis cache might be unavailable.", id);
        }

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        
        if (task == null || task.UserId != userId)
        {
            return false;
        }

        await _taskRepository.DeleteAsync(task);
        return true;
    }

    private static TaskResponse MapToDto(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            UserId = task.UserId
        };
    }
}
