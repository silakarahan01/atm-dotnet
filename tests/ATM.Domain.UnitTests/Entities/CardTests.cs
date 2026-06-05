using ATM.Domain.Entities;
using ATM.Domain.Errors;

namespace ATM.Domain.UnitTests.Entities;

public class CardTests
{
    private static Card CreateCard()
        => new("1234567890123456", "hash", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), userId: 1, accountId: 1);

    [Fact]
    public void RegisterFailedAttempt_BelowLimit_ReturnsRemainingAttemptsAndDoesNotBlock()
    {
        var card = CreateCard();

        var error = card.RegisterFailedAttempt();

        card.IsBlocked.Should().BeFalse();
        card.FailedAttempts.Should().Be(1);
        error.Code.Should().Be(CardErrors.InvalidPin(2).Code);
    }

    [Fact]
    public void RegisterFailedAttempt_OnThirdAttempt_BlocksCard()
    {
        var card = CreateCard();

        card.RegisterFailedAttempt();
        card.RegisterFailedAttempt();
        var thirdError = card.RegisterFailedAttempt();

        card.IsBlocked.Should().BeTrue();
        card.FailedAttempts.Should().Be(Card.MaxFailedAttempts);
        thirdError.Should().Be(CardErrors.JustBlocked);
    }

    [Fact]
    public void ResetFailedAttempts_SetsCounterBackToZero()
    {
        var card = CreateCard();
        card.RegisterFailedAttempt();

        card.ResetFailedAttempts();

        card.FailedAttempts.Should().Be(0);
        card.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void ChangePin_UpdatesThePinHash()
    {
        var card = CreateCard();

        card.ChangePin("new-hash");

        card.PinHash.Should().Be("new-hash");
    }
}
