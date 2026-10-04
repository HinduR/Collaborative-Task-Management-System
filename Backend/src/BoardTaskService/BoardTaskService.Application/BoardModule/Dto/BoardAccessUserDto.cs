namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents a user-project mapping that has access to a board.</summary>
public class BoardAccessUserDto
{
    /// <summary>Gets or sets the unique identifier of the user-project mapping.</summary>
    public Guid UserProjectMappingId { get; set; }

    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid UserId { get; set; }
}
