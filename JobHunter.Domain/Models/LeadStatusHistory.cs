namespace JobHunter.Domain.Models;

public class LeadStatusHistory
{
    public int Id { get; set; }
    public int JobStatusId { get; set; }
    public JobStatus Status { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Reason { get; set; }
}
