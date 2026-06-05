using ATM.Domain.Entities;
using ATM.Domain.Enums;

namespace ATM.Application.UnitTests;

/// <summary>
/// Domain entity'lerinin kapsüllemesini (private setter) bozmadan, testler için
/// kimlik ve navigation alanlarını reflection ile dolduran yardımcı.
/// </summary>
internal static class TestEntities
{
    private static readonly DateTime Expiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static User User(int id = 1, string firstName = "Ahmet", string lastName = "Yılmaz")
    {
        var user = new User(firstName, lastName, "ahmet@example.com");
        SetId(user, id);
        return user;
    }

    public static Account Account(int id = 1, decimal balance = 1000m, string number = "TR001234567890", User? user = null)
    {
        var owner = user ?? User(id);
        var account = new Account(number, AccountType.Checking, owner.Id, balance);
        SetId(account, id);
        SetMember(account, nameof(Domain.Entities.Account.User), owner);
        return account;
    }

    public static Card Card(
        int id = 1,
        string number = "1234567890123456",
        string pinHash = "hash",
        bool isBlocked = false,
        Account? account = null,
        User? user = null)
    {
        var owner = user ?? User();
        var acc = account ?? Account(user: owner);
        var card = new Card(number, pinHash, Expiry, owner.Id, acc.Id);
        SetId(card, id);
        SetMember(card, nameof(Domain.Entities.Card.User), owner);
        SetMember(card, nameof(Domain.Entities.Card.Account), acc);
        if (isBlocked)
            SetMember(card, nameof(Domain.Entities.Card.IsBlocked), true);
        return card;
    }

    private static void SetId(object entity, int id)
        => entity.GetType().GetProperty("Id")!.SetValue(entity, id);

    private static void SetMember(object entity, string propertyName, object value)
        => entity.GetType().GetProperty(propertyName)!.SetValue(entity, value);
}
