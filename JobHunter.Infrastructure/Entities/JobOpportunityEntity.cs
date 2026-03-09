using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobHunter.Domain.Models;

namespace JobHunter.Infrastructure.Entities;

[Table("JobLeads")]
public class JobOpportunityEntity
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool IsAutomationPossible { get; set; }
    public decimal? AskedPrice { get; set; }
    
    // Extensible property bag for provider-specific data (e.g. geometric data, CAD links)
    // Now modeled as a string for JSON persistence in the entity
    [Column("Metadata")]
    public string? MetadataJson { get; set; }

    // Analytics Fields
    public DateTime DateFetched { get; set; } = DateTime.Now;
    
    // Stored as CSV in DB
    [Column("MachiningProcessInferred")]
    public string? MachiningProcessInferredCsv { get; set; } 
    public decimal? EstimatedValue { get; set; }

    public string? ProviderJobId { get; set; }

    // Business relationship (replaces ProviderName)
    public int? BusinessId { get; set; }

    [ForeignKey(nameof(BusinessId))]
    public BusinessEntity? Business { get; set; }

    // Preferred contact from the business's contacts
    public int? PreferredContactId { get; set; }

    [ForeignKey(nameof(PreferredContactId))]
    public ContactEntity? PreferredContact { get; set; }

    // Navigation - many-to-many materials
    public List<JobMaterialLinkEntity> MaterialLinks { get; set; } = new();
    
    // Navigation - related entities
    public List<JobPartDesignLinkEntity> PartDesignLinks { get; set; } = new();
    public List<LeadStatusHistoryEntity> StatusHistories { get; set; } = new();
}

