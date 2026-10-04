namespace BoardTaskService.Application.ProjectModule.Dto;

/// <summary>Represents the projects assigned to a user.</summary>
public class UserProjectMappingsDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the unique identifiers of the projects assigned to the user.</summary>
    public List<Guid> ProjectIdList { get; set; } = [];
}
