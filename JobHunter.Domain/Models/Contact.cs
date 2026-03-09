namespace JobHunter.Domain.Models;

public class Contact
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Title { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public override bool Equals(object? obj)
    {
        if (obj is Contact other)
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
