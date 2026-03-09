namespace JobHunter.Domain.Models;

/// <summary>
/// The profile shape / cross-section type of raw material stock.
/// </summary>
public class StockType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
