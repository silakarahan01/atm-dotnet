using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Application.Abstractions.Security;
using ATM.Domain.Common;
using ATM.Domain.Errors;
using FluentValidation;

namespace ATM.Application.Features.Auth.Login;

public sealed record LoginCommand(string CardNumber, string Pin) : ICommand<LoginResponse>;

public sealed record LoginResponse(string Token, DateTime ExpiresAt, string CardholderName);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Kart numarası zorunludur.")
            .Matches("^[0-9]{16}$").WithMessage("Kart numarası 16 haneli bir sayı olmalıdır.");

        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("PIN zorunludur.")
            .Matches("^[0-9]{4}$").WithMessage("PIN 4 haneli bir sayı olmalıdır.");
    }
}

public sealed class LoginCommandHandler(
    ICardRepository cardRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var card = await cardRepository.GetByCardNumberAsync(command.CardNumber, cancellationToken);

        if (card is null)
            return Result.Failure<LoginResponse>(CardErrors.NotFound);

        if (card.IsBlocked)
            return Result.Failure<LoginResponse>(CardErrors.Blocked);

        if (!passwordHasher.Verify(command.Pin, card.PinHash))
        {
            var error = card.RegisterFailedAttempt();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginResponse>(error);
        }

        card.ResetFailedAttempts();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = tokenGenerator.Generate(card);
        return new LoginResponse(token, expiresAt, card.User.FullName);
    }
}
