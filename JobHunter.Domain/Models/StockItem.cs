namespace JobHunter.Domain.Models;

/// <summary>
/// A piece of raw material stock in the shop inventory.
/// Dimensions are in inches and only the fields relevant to the
/// <see cref="StockType"/> are populated; the rest remain null.
/// </summary>
public class StockItem
{
    public int Id { get; set; }

    /// <summary>Profile shape / cross-section type ID.</summary>
    public int StockTypeId { get; set; }

    /// <summary>Navigation — resolved profile shape.</summary>
    public StockType? StockType { get; set; }

    /// <summary>FK to the material this stock is made from.</summary>
    public int MaterialId { get; set; }

    /// <summary>Navigation — resolved material.</summary>
    public Material? Material { get; set; }

    // ── Dimensions (inches) ──

    /// <summary>Outside Diameter — round bar, round tubing.</summary>
    public decimal? OutsideDiameter { get; set; }

    /// <summary>Inside Diameter — round tubing.</summary>
    public decimal? InsideDiameter { get; set; }

    /// <summary>Wall Thickness — tubing, angle, U-channel.</summary>
    public decimal? WallThickness { get; set; }

    /// <summary>Width — flat bar, square bar, rectangular tubing, square tubing, key stock.</summary>
    public decimal? Width { get; set; }

    /// <summary>Height — rectangular tubing, key stock.</summary>
    public decimal? Height { get; set; }

    /// <summary>Thickness — flat bar, angle, U-channel.</summary>
    public decimal? Thickness { get; set; }

    /// <summary>Overall cut length of the stock piece.</summary>
    public decimal? Length { get; set; }

    /// <summary>Distance across opposite flats — hex bar.</summary>
    public decimal? AcrossFlats { get; set; }

    /// <summary>Leg length (long leg) — angle profile.</summary>
    public decimal? LegLength { get; set; }

    /// <summary>Leg width (short leg) — angle profile.</summary>
    public decimal? LegWidth { get; set; }

    /// <summary>Flange width — U-channel.</summary>
    public decimal? FlangeWidth { get; set; }

    /// <summary>Web depth — U-channel.</summary>
    public decimal? WebDepth { get; set; }

    // ── Inventory fields ──

    /// <summary>Number of pieces on hand.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Storage location identifier (rack, bin, etc.).</summary>
    public string? Location { get; set; }

    // ── Computed helpers ──

    /// <summary>Human-readable summary of the relevant dimensions for display.</summary>
    public string DimensionSummary => StockType?.Name switch
    {
        "RoundBar"          => $"OD {OutsideDiameter}\" × {Length}\" L",
        "FlatBar"           => $"{Thickness}\" T × {Width}\" W × {Length}\" L",
        "SquareBar"         => $"{Width}\" sq × {Length}\" L",
        "HexBar"            => $"{AcrossFlats}\" AF × {Length}\" L",
        "RoundTubing"       => $"OD {OutsideDiameter}\" × ID {InsideDiameter}\" × {WallThickness}\" wall × {Length}\" L",
        "SquareTubing"      => $"{Width}\" sq × {WallThickness}\" wall × {Length}\" L",
        "RectangularTubing" => $"{Width}\" W × {Height}\" H × {WallThickness}\" wall × {Length}\" L",
        "Angle"             => $"{LegLength}\" × {LegWidth}\" × {Thickness}\" T × {Length}\" L",
        "UChannel"          => $"{WebDepth}\" web × {FlangeWidth}\" flange × {Thickness}\" T × {Length}\" L",
        "KeyStock"          => $"{Width}\" W × {Height}\" H × {Length}\" L",
        _                           => "N/A"
    };
}
