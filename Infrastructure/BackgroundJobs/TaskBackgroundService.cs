using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.BackgroundJobs;

public class TaskBackgroundService : BackgroundService
{
    private readonly ITaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskBackgroundService> _logger;

    public TaskBackgroundService(ITaskQueue taskQueue, IServiceProvider serviceProvider, ILogger<TaskBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TaskBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var taskItem = await _taskQueue.DequeueAsync(stoppingToken);

                // Simulate processing delay
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

                // Resolve scoped repository to update database
                using var scope = _serviceProvider.CreateScope();
                var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

                // Reload task to ensure we have the latest version from DB
                var dbTask = await taskRepository.GetByIdAsync(taskItem.Id);
                if (dbTask != null)
                {
                    dbTask.Status = TaskManagement.Domain.Enums.TaskStatus.InProgress;
                    dbTask.UpdatedAt = DateTime.UtcNow;

                    await taskRepository.UpdateAsync(dbTask);
                    _logger.LogInformation("BackgroundService: Task {TaskId} processed and marked as InProgress.", dbTask.Id);
                }
            }
            catch (OperationCanceledException)
            {
                // Task was canceled during stopping, perfectly normal.
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BackgroundService: Error occurred executing task work item.");
            }
        }

        _logger.LogInformation("TaskBackgroundService is stopping.");
    }
}
