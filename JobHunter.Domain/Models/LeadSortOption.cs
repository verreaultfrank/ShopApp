using System.ComponentModel;

namespace JobHunter.Domain.Models;

public enum LeadSortOption
{
    [Description("Newest First")]
    NewestFirst,
    
    [Description("Closing Date (Soonest)")]
    ClosingDateSoonest,
    
    [Description("Price (Highest)")]
    PriceHighest,
    
    [Description("Price (Lowest)")]
    PriceLowest
}
