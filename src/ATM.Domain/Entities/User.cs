namespace ATM.Domain.Entities;

public class User
{
    private User() { } // EF Core

    public User(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Card> Cards { get; private set; } = new List<Card>();
    public ICollection<Account> Accounts { get; private set; } = new List<Account>();

    public string FullName => $"{FirstName} {LastName}";
}
