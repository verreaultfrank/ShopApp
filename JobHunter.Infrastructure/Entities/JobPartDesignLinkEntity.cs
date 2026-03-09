using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobHunter.Infrastructure.Entities;

[Table("JobPartDesignLinks")]
public class JobPartDesignLinkEntity
{
    [Key]
    public int Id { get; set; }

    [Column("JobLeadId")]
    public string JobLeadId { get; set; } = string.Empty;
    public JobOpportunityEntity Job { get; set; } = null!;

    [Column("PartDesignId")]
    public int PartDesignId { get; set; }
    public PartDesignEntity PartDesign { get; set; } = null!;
}
