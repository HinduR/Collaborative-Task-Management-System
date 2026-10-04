namespace BoardTaskService.Application.ProjectModule.Dto;

/// <summary>Represents a user mapped to a project.</summary>
public class ProjectUserMappingDto
{
    /// <summary>Gets or sets the unique identifier of the user-project mapping.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the display name of the user.</summary>
    public string UserName { get; set; } = string.Empty;
}
