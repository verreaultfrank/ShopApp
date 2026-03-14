using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobHunter.Domain.Models;

namespace JobHunter.Infrastructure.Entities;

[Table("LeadStatusHistories")]
public class LeadStatusHistoryEntity
{
    [Key]
    public int Id { get; set; }
    
    [Column("LeadId")]
    public string LeadId { get; set; } = string.Empty;
    public LeadEntity Lead { get; set; } = null!;

    [Column("LeadStatusId")]
    public int LeadStatusId { get; set; }
    public LeadStatusEntity Status { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
}
