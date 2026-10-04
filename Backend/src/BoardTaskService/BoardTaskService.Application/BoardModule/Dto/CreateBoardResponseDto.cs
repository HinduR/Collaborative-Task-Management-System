namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents the response returned after a board is created.</summary>
public class CreateBoardResponseDto
{
    /// <summary>Gets or sets the unique identifier of the created board.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the created board.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the Board Owner.</summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>Gets or sets the workflow columns belonging to the created board.</summary>
    public List<WorkflowColumnDto> WorkflowColumns { get; set; } = [];
}
