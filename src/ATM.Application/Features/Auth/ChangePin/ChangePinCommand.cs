using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Application.Abstractions.Security;
using ATM.Domain.Common;
using ATM.Domain.Errors;
using FluentValidation;

namespace ATM.Application.Features.Auth.ChangePin;

public sealed record ChangePinCommand(int CardId, string CurrentPin, string NewPin) : ICommand;

public sealed class ChangePinCommandValidator : AbstractValidator<ChangePinCommand>
{
    public ChangePinCommandValidator()
    {
        RuleFor(x => x.CurrentPin)
            .NotEmpty().WithMessage("Mevcut PIN zorunludur.");

        RuleFor(x => x.NewPin)
            .Matches("^[0-9]{4}$").WithMessage("Yeni PIN 4 haneli bir sayı olmalıdır.");
    }
}

public sealed class ChangePinCommandHandler(
    ICardRepository cardRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangePinCommand>
{
    public async Task<Result> Handle(ChangePinCommand command, CancellationToken cancellationToken)
    {
        var card = await cardRepository.GetByIdAsync(command.CardId, cancellationToken);

        if (card is null)
            return Result.Failure(CardErrors.NotFound);

        if (!passwordHasher.Verify(command.CurrentPin, card.PinHash))
            return Result.Failure(CardErrors.WrongCurrentPin);

        card.ChangePin(passwordHasher.Hash(command.NewPin));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
