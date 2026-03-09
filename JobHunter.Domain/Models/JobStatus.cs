namespace JobHunter.Domain.Models;

public class JobStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Helper static constants to aid with logic previously referencing enum
    public const string New = "New";
    public const string Quoted = "Quoted";
    public const string Won = "Won";
    public const string Lost = "Lost";
    public const string Rejected = "Rejected";

    public override bool Equals(object? obj)
    {
        if (obj is JobStatus other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
