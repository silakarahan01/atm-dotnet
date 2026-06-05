using ATM.Application.Features.Transaction.Withdraw;
using FluentValidation.TestHelper;

namespace ATM.Application.UnitTests.Features.Transaction;

public class WithdrawCommandValidatorTests
{
    private readonly WithdrawCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_WithNonPositiveAmount_HasError(decimal amount)
    {
        var result = _validator.TestValidate(new WithdrawCommand(1, amount));

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithPositiveAmount_HasNoError()
    {
        var result = _validator.TestValidate(new WithdrawCommand(1, 250m));

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
