using System.Security.Claims;
using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.Realtime;

/// <summary>Provides real-time board presence operations through SignalR.</summary>
[Authorize]
public class BoardHub : Hub
{
    private readonly IBoardPresenceTracker _presenceTracker;
    private readonly ILoggerManager<BoardHub> _log;

    /// <summary>Initializes a new instance of the <see cref="BoardHub"/> class.</summary>
    /// <param name="presenceTracker">Tracks active user connections for each board.</param>
    /// <param name="log">Logger used to record SignalR presence activity.</param>
    public BoardHub(
        IBoardPresenceTracker presenceTracker,
        ILoggerManager<BoardHub> log)
    {
        _presenceTracker = presenceTracker;
        _log = log;
    }

    /// <summary>Adds the current SignalR connection to the specified board and broadcasts the updated presence list.</summary>
    /// <param name="boardId">The unique identifier of the board to join.</param>
    public async Task JoinBoard(Guid boardId)
    {
        Guid userId = GetUserId();

        string userName =
            Context.User?.FindFirstValue(ClaimTypes.Name)
            ?? Context.User?.FindFirstValue("name")
            ?? "Unknown user";

        _log.LogDebug(
            "Presence join started. BoardId: {BoardId}, UserId: {UserId}, UserName: {UserName}, ConnectionId: {ConnectionId}.",
            boardId,
            userId,
            userName,
            Context.ConnectionId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetBoardGroupName(boardId));

        _log.LogDebug(
            "Connection {ConnectionId} added to SignalR group {GroupName}.",
            Context.ConnectionId,
            GetBoardGroupName(boardId));

        _presenceTracker.AddConnection(
            boardId,
            userId,
            userName,
            Context.ConnectionId);

        _log.LogDebug(
            "Presence connection registered. BoardId: {BoardId}, UserId: {UserId}, ConnectionId: {ConnectionId}.",
            boardId,
            userId,
            Context.ConnectionId);

        List<BoardActiveUserDto> activeUsers =
            _presenceTracker.GetActiveUsers(boardId);

        Console.WriteLine(
            string.Concat(
                "Broadcasting presence: Board=",
                boardId,
                ", Users=",
                activeUsers.Count));

        _log.LogDebug(
            "Broadcasting presence update for board {BoardId} with {ActiveUserCount} active users.",
            boardId,
            activeUsers.Count);

        await Clients
            .Group(GetBoardGroupName(boardId))
            .SendAsync(
                "BoardPresenceChanged",
                activeUsers);

        _log.LogDebug(
            "Presence join completed. BoardId: {BoardId}, UserId: {UserId}, ConnectionId: {ConnectionId}.",
            boardId,
            userId,
            Context.ConnectionId);
    }

    /// <summary>Removes the current SignalR connection from the specified board and broadcasts the updated presence list.</summary>
    /// <param name="boardId">The unique identifier of the board to leave.</param>
    public async Task LeaveBoard(Guid boardId)
    {
        _log.LogDebug(
            "Presence leave started. BoardId: {BoardId}, ConnectionId: {ConnectionId}.",
            boardId,
            Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetBoardGroupName(boardId));

        _log.LogDebug(
            "Connection {ConnectionId} removed from SignalR group {GroupName}.",
            Context.ConnectionId,
            GetBoardGroupName(boardId));

        _presenceTracker.RemoveConnection(
            Context.ConnectionId);

        _log.LogDebug(
            "Presence connection removed. BoardId: {BoardId}, ConnectionId: {ConnectionId}.",
            boardId,
            Context.ConnectionId);

        await BroadcastPresence(boardId);

        _log.LogDebug(
            "Presence leave completed. BoardId: {BoardId}, ConnectionId: {ConnectionId}.",
            boardId,
            Context.ConnectionId);
    }

    /// <summary>Removes the disconnected SignalR connection from presence tracking and broadcasts the updated presence list.</summary>
    /// <param name="exception">The exception that caused the disconnection, if one occurred.</param>
    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        if (exception is null)
        {
            _log.LogDebug(
                "SignalR connection disconnected normally. ConnectionId: {ConnectionId}.",
                Context.ConnectionId);
        }
        else
        {
            _log.LogError(
                "SignalR connection disconnected due to an error. ConnectionId: {ConnectionId}.",
                exception,
                Context.ConnectionId);
        }

        Guid? boardId =
            _presenceTracker.RemoveConnection(
                Context.ConnectionId);

        if (boardId.HasValue)
        {
            _log.LogDebug(
                "Disconnected connection {ConnectionId} removed from board {BoardId}.",
                Context.ConnectionId,
                boardId.Value);

            await BroadcastPresence(boardId.Value);
        }
        else
        {
            _log.LogDebug(
                "No board presence registration was found for disconnected connection {ConnectionId}.",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);

        _log.LogDebug(
            "SignalR disconnection processing completed. ConnectionId: {ConnectionId}.",
            Context.ConnectionId);
    }

    /// <summary>Creates the SignalR group name for the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <returns>The SignalR group name associated with the board.</returns>
    public static string GetBoardGroupName(Guid boardId)
    {
        return string.Concat(
            "board-",
            boardId);
    }

    /// <summary>Broadcasts the current active-user list to all connections in the specified board group.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    private Task BroadcastPresence(Guid boardId)
    {
        List<BoardActiveUserDto> users =
            _presenceTracker.GetActiveUsers(boardId);

        _log.LogDebug(
            "Broadcasting presence update for board {BoardId} with {ActiveUserCount} active users.",
            boardId,
            users.Count);

        return Clients
            .Group(GetBoardGroupName(boardId))
            .SendAsync(
                "BoardPresenceChanged",
                users);
    }

    /// <summary>Retrieves the authenticated user identifier from the SignalR connection claims.</summary>
    /// <returns>The authenticated user's unique identifier.</returns>
    /// <exception cref="HubException">Thrown when the authenticated user identifier is missing or invalid.</exception>
    private Guid GetUserId()
    {
        string? value =
            Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("userId")
            ?? Context.User?.FindFirstValue("sub");

        if (!Guid.TryParse(value, out Guid userId))
        {
            _log.LogError(
                "Authenticated user ID is unavailable for SignalR connection {ConnectionId}.",
                null,
                Context.ConnectionId);

            throw new HubException(
                "Authenticated user ID is unavailable.");
        }

        _log.LogDebug(
            "Authenticated user {UserId} resolved for SignalR connection {ConnectionId}.",
            userId,
            Context.ConnectionId);

        return userId;
    }
}
