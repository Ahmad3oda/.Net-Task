using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateTaskAsync(Guid userId, CreateTaskRequest request);
    Task<TaskResponse?> GetTaskByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TaskResponse>> GetMyTasksAsync(Guid userId);
    Task<TaskResponse?> UpdateTaskStatusAsync(Guid id, Guid userId, UpdateTaskStatusRequest request);
    Task<bool> DeleteTaskAsync(Guid id, Guid userId);
}
