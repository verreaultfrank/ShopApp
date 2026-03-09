using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobHunter.Infrastructure.Entities;

[Table("JobMaterialLinks")]
public class JobMaterialLinkEntity
{
    [Key]
    public int Id { get; set; }
    
    public string JobId { get; set; } = string.Empty;
    public JobOpportunityEntity Job { get; set; } = null!;

    public int MaterialId { get; set; }
    public MaterialEntity Material { get; set; } = null!;
}
