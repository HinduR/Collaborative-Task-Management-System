using System.Collections.Concurrent;
using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.Realtime;

/// <summary>Tracks active users and their SignalR connections for each board.</summary>
public class BoardPresenceTracker : IBoardPresenceTracker
{
    private readonly ConcurrentDictionary<
        Guid,
        ConcurrentDictionary<Guid, ActiveUser>>
        _boardUsers = new();

    private readonly ConcurrentDictionary<string, ConnectionInfo>
        _connections = new();

    private readonly ILoggerManager<BoardPresenceTracker> _logger;

    /// <summary>Initializes a new instance of the <see cref="BoardPresenceTracker"/> class.</summary>
    /// <param name="logger">Logger used to record board presence activity.</param>
    public BoardPresenceTracker(
        ILoggerManager<BoardPresenceTracker> logger)
    {
        _logger = logger;
    }

    /// <summary>Adds a SignalR connection for a user who has joined a board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="userId">The unique identifier of the active user.</param>
    /// <param name="userName">The display name of the active user.</param>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    public void AddConnection(
        Guid boardId,
        Guid userId,
        string userName,
        string connectionId)
    {
        _logger.LogDebug(
            "Adding presence connection {ConnectionId} for user {UserId} on board {BoardId}.",
            connectionId,
            userId,
            boardId);

        ConcurrentDictionary<Guid, ActiveUser> users =
            _boardUsers.GetOrAdd(
                boardId,
                _ => new ConcurrentDictionary<Guid, ActiveUser>());

        ActiveUser activeUser = users.GetOrAdd(
            userId,
            _ => new ActiveUser
            {
                UserId = userId,
                UserName = userName
            });

        activeUser.ConnectionIds.TryAdd(connectionId, 0);

        _connections[connectionId] = new ConnectionInfo
        {
            BoardId = boardId,
            UserId = userId
        };

        _logger.LogDebug(
            "Presence connection {ConnectionId} added for user {UserId} on board {BoardId}. User connection count: {ConnectionCount}.",
            connectionId,
            userId,
            boardId,
            activeUser.ConnectionIds.Count);
    }

    /// <summary>Removes a SignalR connection and removes the user from the board when no connections remain.</summary>
    /// <param name="connectionId">The SignalR connection identifier to remove.</param>
    /// <returns>The board identifier associated with the connection, or <see langword="null"/> when the connection was not registered.</returns>
    public Guid? RemoveConnection(string connectionId)
    {
        _logger.LogDebug(
            "Removing presence connection {ConnectionId}.",
            connectionId);

        if (!_connections.TryRemove(
                connectionId,
                out ConnectionInfo? connection))
        {
            _logger.LogDebug(
                "Presence connection {ConnectionId} was not registered.",
                connectionId);

            return null;
        }

        if (!_boardUsers.TryGetValue(
                connection.BoardId,
                out ConcurrentDictionary<Guid, ActiveUser>? users))
        {
            _logger.LogDebug(
                "No active-user collection was found for board {BoardId} while removing connection {ConnectionId}.",
                connection.BoardId,
                connectionId);

            return connection.BoardId;
        }

        if (users.TryGetValue(
                connection.UserId,
                out ActiveUser? user))
        {
            user.ConnectionIds.TryRemove(connectionId, out _);

            _logger.LogDebug(
                "Connection {ConnectionId} removed for user {UserId} on board {BoardId}. Remaining connections: {ConnectionCount}.",
                connectionId,
                connection.UserId,
                connection.BoardId,
                user.ConnectionIds.Count);

            if (user.ConnectionIds.IsEmpty)
            {
                users.TryRemove(connection.UserId, out _);

                _logger.LogDebug(
                    "User {UserId} removed from board {BoardId} presence because no active connections remain.",
                    connection.UserId,
                    connection.BoardId);
            }
        }

        if (users.IsEmpty)
        {
            _boardUsers.TryRemove(connection.BoardId, out _);

            _logger.LogDebug(
                "Board {BoardId} removed from presence tracking because no active users remain.",
                connection.BoardId);
        }

        return connection.BoardId;
    }

    /// <summary>Gets the users who currently have at least one active connection to a board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <returns>A list containing the board's active users.</returns>
    public List<BoardActiveUserDto> GetActiveUsers(Guid boardId)
    {
        if (!_boardUsers.TryGetValue(
                boardId,
                out ConcurrentDictionary<Guid, ActiveUser>? users))
        {
            _logger.LogDebug(
                "No active users were found for board {BoardId}.",
                boardId);

            return [];
        }

        List<BoardActiveUserDto> activeUsers = users.Values
            .OrderBy(user => user.UserName)
            .Select(user => new BoardActiveUserDto
            {
                UserId = user.UserId,
                UserName = user.UserName
            })
            .ToList();

        _logger.LogDebug(
            "Retrieved {ActiveUserCount} active users for board {BoardId}.",
            activeUsers.Count,
            boardId);

        return activeUsers;
    }

    /// <summary>Represents an active user and the user's SignalR connections.</summary>
    private sealed class ActiveUser
    {
        /// <summary>Gets the unique identifier of the active user.</summary>
        public Guid UserId { get; init; }

        /// <summary>Gets the display name of the active user.</summary>
        public string UserName { get; init; } = string.Empty;

        /// <summary>Gets the active SignalR connection identifiers belonging to the user.</summary>
        public ConcurrentDictionary<string, byte> ConnectionIds
            { get; } = new();
    }

    /// <summary>Represents the board and user associated with a SignalR connection.</summary>
    private sealed class ConnectionInfo
    {
        /// <summary>Gets the unique identifier of the board associated with the connection.</summary>
        public Guid BoardId { get; init; }

        /// <summary>Gets the unique identifier of the user associated with the connection.</summary>
        public Guid UserId { get; init; }
    }
}