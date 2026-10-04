namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents a user who is currently active on a board.</summary>
public class BoardActiveUserDto
{
    /// <summary>Gets or sets the unique identifier of the active user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the display name of the active user.</summary>
    public string UserName { get; set; } = string.Empty;
}
