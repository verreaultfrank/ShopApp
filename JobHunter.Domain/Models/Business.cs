namespace JobHunter.Domain.Models;

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }

    public List<Contact> Contacts { get; set; } = new();

    public override bool Equals(object? obj)
    {
        if (obj is Business other)
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
