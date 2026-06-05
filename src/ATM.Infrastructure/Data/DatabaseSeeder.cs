using ATM.Domain.Entities;
using ATM.Domain.Enums;

namespace ATM.Infrastructure.Data;

public static class DatabaseSeeder
{
    private static readonly DateTime ExpiryDate = new(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var user = new User("Ahmet", "Yılmaz", "ahmet@example.com");
        context.Users.Add(user);
        context.SaveChanges();

        var account = new Account("TR001234567890", AccountType.Checking, user.Id, 5000.00m);
        context.Accounts.Add(account);
        context.SaveChanges();

        context.Cards.Add(new Card(
            "1234567890123456",
            BCrypt.Net.BCrypt.HashPassword("1234"),
            ExpiryDate,
            user.Id,
            account.Id));

        // İkinci hesap (transfer testi için)
        var user2 = new User("Fatma", "Kaya", "fatma@example.com");
        context.Users.Add(user2);
        context.SaveChanges();

        var account2 = new Account("TR009876543210", AccountType.Savings, user2.Id, 1000.00m);
        context.Accounts.Add(account2);
        context.SaveChanges();

        context.Cards.Add(new Card(
            "6543210987654321",
            BCrypt.Net.BCrypt.HashPassword("5678"),
            ExpiryDate,
            user2.Id,
            account2.Id));

        context.SaveChanges();
    }
}
