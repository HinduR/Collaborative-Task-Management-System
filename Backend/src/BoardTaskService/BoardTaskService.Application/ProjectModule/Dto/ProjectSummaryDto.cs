namespace BoardTaskService.Application.ProjectModule.Dto;

/// <summary>Represents summary information about a project and its assignment status.</summary>
public class ProjectSummaryDto
{
    /// <summary>Gets or sets the unique identifier of the project.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the name of the project.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the project is assigned to the current user.</summary>
    public bool IsAssigned { get; set; }
}
