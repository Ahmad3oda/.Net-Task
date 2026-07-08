using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services;

public class TaskQueue : ITaskQueue
{
    private readonly Channel<TaskItem> _queue;

    public TaskQueue()
    {
        _queue = Channel.CreateUnbounded<TaskItem>();
    }

    public async ValueTask EnqueueAsync(TaskItem taskItem)
    {
        await _queue.Writer.WriteAsync(taskItem);
    }

    public async ValueTask<TaskItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
