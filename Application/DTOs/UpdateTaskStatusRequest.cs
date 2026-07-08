using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class UpdateTaskStatusRequest
{
    public TaskManagement.Domain.Enums.TaskStatus Status { get; set; }
}
