using ATM.Domain.Entities;
using ATM.Domain.Enums;
using ATM.Domain.Errors;

namespace ATM.Domain.UnitTests.Entities;

public class AccountTests
{
    private static Account CreateAccount(decimal balance = 1000m)
        => new("TR001234567890", AccountType.Checking, userId: 1, initialBalance: balance);

    [Fact]
    public void Withdraw_WithSufficientBalance_DecreasesBalanceAndSucceeds()
    {
        var account = CreateAccount(1000m);

        var result = account.Withdraw(400m);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(600m);
    }

    [Fact]
    public void Withdraw_WithInsufficientBalance_FailsAndLeavesBalanceUnchanged()
    {
        var account = CreateAccount(100m);

        var result = account.Withdraw(500m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InsufficientFunds);
        account.Balance.Should().Be(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Withdraw_WithNonPositiveAmount_FailsWithInvalidAmount(decimal amount)
    {
        var account = CreateAccount(1000m);

        var result = account.Withdraw(amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidAmount);
        account.Balance.Should().Be(1000m);
    }

    [Fact]
    public void Deposit_WithPositiveAmount_IncreasesBalance()
    {
        var account = CreateAccount(1000m);

        var result = account.Deposit(250m);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(1250m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deposit_WithNonPositiveAmount_FailsWithInvalidAmount(decimal amount)
    {
        var account = CreateAccount(1000m);

        var result = account.Deposit(amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidAmount);
        account.Balance.Should().Be(1000m);
    }
}
