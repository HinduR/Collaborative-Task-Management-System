namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents a request to create a workflow column.</summary>
public class CreateWorkflowColumnRequest
{
    /// <summary>Gets or sets the name of the workflow column.</summary>
    public string WorkflowColumnName { get; set; } = string.Empty;
}
