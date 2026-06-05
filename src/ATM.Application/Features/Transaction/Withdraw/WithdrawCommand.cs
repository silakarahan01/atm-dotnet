using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Enums;
using ATM.Domain.Errors;
using FluentValidation;

namespace ATM.Application.Features.Transaction.Withdraw;

public sealed record WithdrawCommand(int AccountId, decimal Amount) : ICommand;

public sealed class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Çekilecek tutar sıfırdan büyük olmalıdır.");
    }
}

public sealed class WithdrawCommandHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<WithdrawCommand>
{
    public async Task<Result> Handle(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null)
            return Result.Failure(AccountErrors.NotFound);

        var withdrawal = account.Withdraw(command.Amount);
        if (withdrawal.IsFailure)
            return withdrawal;

        await transactionRepository.AddAsync(
            new ATM.Domain.Entities.Transaction(
                TransactionType.Withdrawal, command.Amount, account.Balance, account.Id, "Para çekme"),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
