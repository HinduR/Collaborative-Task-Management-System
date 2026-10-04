using BoardTaskService.Application.BoardModule.Dto;

namespace BoardTaskService.Application.BoardModule.Contract.IService;

/// <summary>Defines operations for tracking active users and SignalR connections within boards.</summary>
public interface IBoardPresenceTracker
{
    /// <summary>Adds a SignalR connection for a user who joined a board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="userId">The unique identifier of the active user.</param>
    /// <param name="userName">The display name of the active user.</param>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    void AddConnection(
        Guid boardId,
        Guid userId,
        string userName,
        string connectionId);

    /// <summary>Removes a SignalR connection from presence tracking.</summary>
    /// <param name="connectionId">The SignalR connection identifier to remove.</param>
    /// <returns>The board identifier associated with the connection, or <see langword="null"/> when the connection was not registered.</returns>
    Guid? RemoveConnection(
        string connectionId);

    /// <summary>Gets the users who currently have an active connection to a board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <returns>A list containing the active board users.</returns>
    List<BoardActiveUserDto> GetActiveUsers(
        Guid boardId);
}
