using ATM.Application.Abstractions.Persistence;
using ATM.Application.Abstractions.Security;
using ATM.Application.Features.Auth.Login;
using ATM.Domain.Entities;
using ATM.Domain.Errors;

namespace ATM.Application.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_cardRepository, _passwordHasher, _tokenGenerator, _unitOfWork);
    }

    private const string CardNumber = "1234567890123456";

    [Fact]
    public async Task Handle_WithCorrectPin_ReturnsTokenAndResetsAttempts()
    {
        var card = TestEntities.Card(number: CardNumber);
        _cardRepository.GetByCardNumberAsync(CardNumber, Arg.Any<CancellationToken>()).Returns(card);
        _passwordHasher.Verify("1234", "hash").Returns(true);
        _tokenGenerator.Generate(card).Returns(("token-abc", DateTime.UtcNow.AddMinutes(10)));

        var result = await _handler.Handle(new LoginCommand(CardNumber, "1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("token-abc");
        result.Value.CardholderName.Should().Be("Ahmet Yılmaz");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWrongPin_RegistersFailedAttemptAndFails()
    {
        var card = TestEntities.Card(number: CardNumber);
        _cardRepository.GetByCardNumberAsync(CardNumber, Arg.Any<CancellationToken>()).Returns(card);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _handler.Handle(new LoginCommand(CardNumber, "0000"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CardErrors.InvalidPin(2).Code);
        card.FailedAttempts.Should().Be(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithBlockedCard_FailsWithoutVerifyingPin()
    {
        var card = TestEntities.Card(number: CardNumber, isBlocked: true);
        _cardRepository.GetByCardNumberAsync(CardNumber, Arg.Any<CancellationToken>()).Returns(card);

        var result = await _handler.Handle(new LoginCommand(CardNumber, "1234"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CardErrors.Blocked);
        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownCard_FailsWithNotFound()
    {
        _cardRepository.GetByCardNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Card?)null);

        var result = await _handler.Handle(new LoginCommand("0000000000000000", "1234"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CardErrors.NotFound);
    }
}
