using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskQueue
{
    ValueTask EnqueueAsync(TaskItem taskItem);
    ValueTask<TaskItem> DequeueAsync(CancellationToken cancellationToken);
}
