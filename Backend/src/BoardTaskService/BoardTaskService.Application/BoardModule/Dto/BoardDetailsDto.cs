namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents the details of a board, including its workflow columns and tasks.</summary>
public class BoardDetailsDto
{
    /// <summary>Gets or sets the name of the board.</summary>
    public string BoardName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the current user owns the board.</summary>
    public bool IsBoardOwner { get; set; }

    /// <summary>Gets or sets the workflow columns belonging to the board.</summary>
    public List<BoardColumnDetailsDto> Columns { get; set; } = [];
}

/// <summary>Represents a workflow column and its tasks within a board.</summary>
public class BoardColumnDetailsDto
{
    /// <summary>Gets or sets the unique identifier of the workflow column.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the display name of the workflow column.</summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>Gets or sets the display order of the workflow column.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the tasks belonging to the workflow column.</summary>
    public List<BoardTaskSummaryDto> Tasks { get; set; } = [];
}

/// <summary>Represents summary information about a task displayed within a board column.</summary>
public class BoardTaskSummaryDto
{
    /// <summary>Gets or sets the unique identifier of the task.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the title of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the task priority.</summary>
    public string PriorityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the task type.</summary>
    public string TaskTypeName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the assigned user, or <see langword="null"/> when the task is unassigned.</summary>
    public Guid? AssigneeUserId { get; set; }

    /// <summary>Gets or sets the display name of the assigned user.</summary>
    public string? AssigneeName { get; set; }

    /// <summary>Gets or sets a value indicating whether the current user owns the task.</summary>
    public bool IsTaskOwner { get; set; }
}
