using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobHunter.Infrastructure.Entities;

[Table("StockItems")]
public class StockItemEntity
{
    [Key]
    public int Id { get; set; }

    /// <summary>Stored as Foreign Key to StockTypes.</summary>
    public int StockTypeId { get; set; }

    [ForeignKey(nameof(StockTypeId))]
    public StockTypeEntity? StockType { get; set; }

    public int MaterialId { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public MaterialEntity? Material { get; set; }

    // ── Dimensions (inches, nullable) ──
    [Column(TypeName = "decimal(10,4)")]
    public decimal? OutsideDiameter { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? InsideDiameter { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? WallThickness { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Width { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Height { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Thickness { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Length { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? AcrossFlats { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? LegLength { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? LegWidth { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? FlangeWidth { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? WebDepth { get; set; }

    // ── Inventory ──
    public int Quantity { get; set; } = 1;

    [Column(TypeName = "nvarchar(100)")]
    public string? Location { get; set; }
}
