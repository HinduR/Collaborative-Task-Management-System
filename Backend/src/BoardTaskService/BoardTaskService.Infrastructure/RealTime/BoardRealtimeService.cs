using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using Microsoft.AspNetCore.SignalR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.Realtime;

/// <summary>Provides real-time SignalR notifications for board task events.</summary>
public class BoardRealtimeService : IBoardRealtimeService
{
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ILoggerManager<BoardRealtimeService> _logger;

    /// <summary>Initializes a new instance of the <see cref="BoardRealtimeService"/> class.</summary>
    /// <param name="hubContext">SignalR hub context used to send board notifications.</param>
    /// <param name="logger">Logger used to record real-time board activity.</param>
    public BoardRealtimeService(
        IHubContext<BoardHub> hubContext,
        ILoggerManager<BoardRealtimeService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>Sends a task-moved event to every SignalR connection in the corresponding board group.</summary>
    /// <param name="taskMovedEvent">The task movement information to broadcast.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public Task SendTaskMoved(
        TaskMovedEventDto taskMovedEvent,
        CancellationToken cancellationToken)
    {
        string groupName = BoardHub.GetBoardGroupName(
            taskMovedEvent.BoardId);

        _logger.LogDebug(
            "Broadcasting TaskMoved event for task {TaskId} on board {BoardId} to SignalR group {GroupName}.",
            taskMovedEvent.TaskId,
            taskMovedEvent.BoardId,
            groupName);

        return _hubContext.Clients
            .Group(groupName)
            .SendAsync(
                "TaskMoved",
                taskMovedEvent,
                cancellationToken);
    }
}