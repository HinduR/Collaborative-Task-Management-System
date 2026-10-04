namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents a request to replace the users who have access to a board.</summary>
public class UpdateBoardAccessRequest
{
    /// <summary>Gets or sets the user-project mapping identifiers that should have access to the board.</summary>
    public List<Guid> UserProjectMappingIdList { get; set; } = [];
}
