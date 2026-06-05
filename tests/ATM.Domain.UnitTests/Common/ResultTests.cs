using ATM.Domain.Common;

namespace ATM.Domain.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResultWithNoError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailedResultCarryingTheError()
    {
        var error = Error.Validation("Test.Code", "Bir hata.");

        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ExposesTheValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void GenericFailure_ThrowsWhenValueIsAccessed()
    {
        var result = Result.Failure<int>(Error.Failure("X", "Y"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_ProducesSuccess()
    {
        Result<string> result = "merhaba";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("merhaba");
    }
}
