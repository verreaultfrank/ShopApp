namespace JobHunter.Domain.Models;

public class Lead
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool IsAutomationPossible { get; set; }
    public decimal? AskedPrice { get; set; }
    
    // Extensible property bag for provider-specific data (e.g. geometric data, CAD links)
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Analytics Fields
    public DateTime DateFetched { get; set; } = DateTime.Now;
    public MachiningProcess[] MachiningProcessInferred { get; set; } = Array.Empty<MachiningProcess>();
    public decimal? EstimatedValue { get; set; }

    public string? ProviderJobId { get; set; }

    // Business relationship (replaces ProviderName)
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }

    // Preferred contact from the business's contacts
    public int? PreferredContactId { get; set; }
    public Contact? PreferredContact { get; set; }

    public List<LeadStatusHistory> StatusHistories { get; set; } = new();
}
