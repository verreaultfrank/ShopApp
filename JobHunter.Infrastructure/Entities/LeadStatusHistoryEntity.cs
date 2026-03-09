using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobHunter.Domain.Models;

namespace JobHunter.Infrastructure.Entities;

[Table("LeadStatusHistories")]
public class LeadStatusHistoryEntity
{
    [Key]
    public int Id { get; set; }
    
    [Column("JobLeadId")]
    public string JobLeadId { get; set; } = string.Empty;
    public JobOpportunityEntity Job { get; set; } = null!;

    [Column("JobStatusId")]
    public int JobStatusId { get; set; }
    public JobStatusEntity Status { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
}
