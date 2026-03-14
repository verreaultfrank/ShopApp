using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobHunter.Infrastructure.Entities;

[Table("Materials")]
public class MaterialEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? AmsDesignation { get; set; }
    public string? UnsDesignation { get; set; }
    public string? IsoDesignation { get; set; }
    public string? Form { get; set; }
    public string? TemperCondition { get; set; }
}
