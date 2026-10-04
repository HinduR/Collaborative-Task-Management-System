using System.Runtime.Serialization;

namespace Shared.Common.Dto;

/// <summary>
/// Data Transfer Object (DTO) representing a RefTerm.
/// </summary>
[DataContract]
/// <summary>
/// Represents the RefTermDto component.
/// </summary>
public class RefTermDto
{
    /// <summary>
    /// Gets or sets the RefTermId associated with this RefTerm.
    /// </summary>
    [DataMember(Name = "ref_term_id")]
    /// <summary>
    /// Gets or sets the RefTermId value.
    /// </summary>
    public Guid RefTermId { get; set; }

    /// <summary>
    /// Gets or sets the RefTermKey associated with this RefTerm.
    /// </summary>
    [DataMember(Name = "ref_term_key")]
    /// <summary>
    /// Gets or sets the RefTermKey value.
    /// </summary>
    public string RefTermKey { get; set; }

    /// <summary>
    /// Gets or sets the RefTermKey associated with this RefTerm.
    /// </summary>
    [DataMember(Name = "description")]
    /// <summary>
    /// Gets or sets the Description value.
    /// </summary>
    public string Description { get; set; }
}
