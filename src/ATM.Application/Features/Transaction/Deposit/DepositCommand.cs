using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Entities;
using ATM.Domain.Enums;
using ATM.Domain.Errors;
using FluentValidation;

namespace ATM.Application.Features.Transaction.Deposit;

public sealed record DepositCommand(int AccountId, decimal Amount) : ICommand;

public sealed class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Yatırılacak tutar sıfırdan büyük olmalıdır.");
    }
}

public sealed class DepositCommandHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DepositCommand>
{
    public async Task<Result> Handle(DepositCommand command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null)
            return Result.Failure(AccountErrors.NotFound);

        var deposit = account.Deposit(command.Amount);
        if (deposit.IsFailure)
            return deposit;

        await transactionRepository.AddAsync(
            new ATM.Domain.Entities.Transaction(
                TransactionType.Deposit, command.Amount, account.Balance, account.Id, "Para yatırma"),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
