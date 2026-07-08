using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository
{
    IQueryable<TaskItem> GetQueryable();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetAllByUserAsync(Guid userId);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
