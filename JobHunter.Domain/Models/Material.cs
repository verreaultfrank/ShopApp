namespace JobHunter.Domain.Models;

/// <summary>
/// A material with cross-referenced standard designations (AMS, UNS, ISO).
/// </summary>
public class Material
{
    public int Id { get; set; }

    /// <summary>Common name, e.g. "Aluminum 6061-T6"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Material family, e.g. "Aluminum", "Titanium", "Nickel"</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Aerospace Material Specification, e.g. "AMS 4027"</summary>
    public string? AmsDesignation { get; set; }

    /// <summary>Unified Numbering System, e.g. "A96061"</summary>
    public string? UnsDesignation { get; set; }

    /// <summary>ISO designation, e.g. "AlMg1SiCu"</summary>
    public string? IsoDesignation { get; set; }

    /// <summary>Form: Sheet, Bar, Plate, Forging, Wire, Tube, etc.</summary>
    public string? Form { get; set; }

    /// <summary>Temper or heat treatment condition, e.g. "T6", "Annealed", "H900"</summary>
    public string? TemperCondition { get; set; }
}
