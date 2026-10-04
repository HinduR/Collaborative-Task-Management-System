namespace BoardTaskService.Application.ProjectModule.Dto;

/// <summary>Represents a list of projects to assign to a user.</summary>
public class ProjectListDto
{
    /// <summary>Gets or sets the unique identifiers of the projects to assign.</summary>
    public required List<Guid> ProjectIdList { get; set; }
}
