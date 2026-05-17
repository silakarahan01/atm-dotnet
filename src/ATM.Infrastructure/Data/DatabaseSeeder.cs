using ATM.Core.Entities;
using ATM.Core.Enums;

namespace ATM.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var user = new User
        {
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Email = "ahmet@example.com"
        };
        context.Users.Add(user);
        context.SaveChanges();

        var account = new Account
        {
            AccountNumber = "TR001234567890",
            Balance = 5000.00m,
            AccountType = AccountType.Checking,
            UserId = user.Id
        };
        context.Accounts.Add(account);
        context.SaveChanges();

        var card = new Card
        {
            CardNumber = "1234567890123456",
            PinHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            ExpiryDate = new DateTime(2028, 12, 31),
            UserId = user.Id,
            AccountId = account.Id
        };
        context.Cards.Add(card);

        // İkinci hesap (transfer testi için)
        var user2 = new User
        {
            FirstName = "Fatma",
            LastName = "Kaya",
            Email = "fatma@example.com"
        };
        context.Users.Add(user2);
        context.SaveChanges();

        var account2 = new Account
        {
            AccountNumber = "TR009876543210",
            Balance = 1000.00m,
            AccountType = AccountType.Savings,
            UserId = user2.Id
        };
        context.Accounts.Add(account2);
        context.SaveChanges();

        var card2 = new Card
        {
            CardNumber = "6543210987654321",
            PinHash = BCrypt.Net.BCrypt.HashPassword("5678"),
            ExpiryDate = new DateTime(2028, 12, 31),
            UserId = user2.Id,
            AccountId = account2.Id
        };
        context.Cards.Add(card2);
        context.SaveChanges();
    }
}
