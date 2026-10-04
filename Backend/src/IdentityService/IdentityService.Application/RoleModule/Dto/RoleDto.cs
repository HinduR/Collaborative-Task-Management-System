namespace IdentityService.Application.RoleModule.Dto;

/// <summary>
/// Represents the RoleDto component.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the Id value.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Name value.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description value.
    /// </summary>
    public string? Description { get; set; }
}
