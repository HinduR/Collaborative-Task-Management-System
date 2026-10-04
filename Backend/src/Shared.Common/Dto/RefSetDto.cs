namespace Shared.Common.Dto;

/// <summary>
/// Represents the RefSetDto component.
/// </summary>
public class RefSetDto
{
    
    /// <summary>
    /// Gets or sets the RefSetId value.
    /// </summary>
    public Guid RefSetId { get; set; }

    /// <summary>
    /// Gets or sets the RefSetKey with this RefSet.
    /// </summary>
    public string? RefSetKey { get; set; }

    /// <summary>
    /// Gets or sets the list of RefTerms terms associated with this RefSet.
    /// </summary>
    public List<RefTermDto>? RefTerms { get; set; }

}
