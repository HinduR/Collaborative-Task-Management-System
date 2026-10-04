namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents summary information about a board and its accessible workflow columns.</summary>
public class BoardSummaryDto
{
    /// <summary>Gets or sets the unique identifier of the board.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the board.</summary>
    public string BoardName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the Board Owner.</summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>Gets or sets a value indicating whether the current user owns the board.</summary>
    public bool IsBoardOwner { get; set; }

    /// <summary>Gets or sets the workflow columns accessible to the current user.</summary>
    public List<WorkflowColumnDto> Columns { get; set; } = [];
}
