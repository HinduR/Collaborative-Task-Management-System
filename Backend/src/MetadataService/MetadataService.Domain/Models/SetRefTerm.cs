using System.ComponentModel.DataAnnotations;
using Shared.Common.Model;

namespace MetadataService.Domain.Models;

public class SetRefTerm : BaseModel
{
    [Key]
    public Guid Id { get; set; }
    public Guid RefSetId { get; set; }
    public RefSet? RefSet { get; set; }
    public Guid RefTermId { get; set; }
    public RefTerm? RefTerm { get; set; }
    public int SortOrder { get; set; }
}