using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines real-time notification operations for board task events.</summary>
public interface IBoardRealtimeService
{
    /// <summary>Broadcasts a task-moved event to the users connected to the corresponding board.</summary>
    /// <param name="taskMovedEvent">The task movement information to broadcast.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task SendTaskMoved(
        TaskMovedEventDto taskMovedEvent,
        CancellationToken cancellationToken);
}
