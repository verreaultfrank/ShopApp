namespace JobHunter.Domain.Models;

public class Quote
{
    public string JobId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public TimeSpan LeadTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> AttachmentPaths { get; set; } = new();
}
