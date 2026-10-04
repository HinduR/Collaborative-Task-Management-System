using System.Runtime.Serialization;

namespace Shared.Common.Dto;

/// <summary>
/// Represents the RefTermRefSetDto component.
/// </summary>
public class RefTermRefSetDto
{
    /// <summary>
    /// Gets or sets the RefSetKey value.
    /// </summary>
    public required string RefSetKey { get; set; }

    /// <summary>
    /// Gets or sets the RefTermList value.
    /// </summary>
    public required List<RefTermDto> RefTermList { get; set; }
}
