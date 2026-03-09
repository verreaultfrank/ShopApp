using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobHunter.Infrastructure.Entities;

[Table("PartDesigns")]
public class PartDesignEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CadFilePath { get; set; } = string.Empty;
    
    public List<JobPartDesignLinkEntity> JobLinks { get; set; } = new();
}
