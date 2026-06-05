using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Enums;
using ATM.Domain.Errors;
using FluentValidation;

namespace ATM.Application.Features.Transaction.Transfer;

public sealed record TransferCommand(int SourceAccountId, string TargetAccountNumber, decimal Amount) : ICommand;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.TargetAccountNumber)
            .NotEmpty().WithMessage("Hedef hesap numarası zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer tutarı sıfırdan büyük olmalıdır.");
    }
}

/// <summary>
/// Kaynak ve hedef hesabın güncellenmesi ile iki işlem kaydının eklenmesi tek bir
/// <see cref="IUnitOfWork.SaveChangesAsync"/> çağrısında, dolayısıyla tek bir veritabanı
/// transaction'ında atomik olarak yapılır — para asla "yolda kaybolmaz".
/// </summary>
public sealed class TransferCommandHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TransferCommand>
{
    public async Task<Result> Handle(TransferCommand command, CancellationToken cancellationToken)
    {
        var source = await accountRepository.GetByIdAsync(command.SourceAccountId, cancellationToken);
        if (source is null)
            return Result.Failure(AccountErrors.NotFound);

        var target = await accountRepository.GetByAccountNumberAsync(command.TargetAccountNumber, cancellationToken);
        if (target is null)
            return Result.Failure(AccountErrors.TargetNotFound);

        if (source.Id == target.Id)
            return Result.Failure(AccountErrors.SameAccountTransfer);

        var withdrawal = source.Withdraw(command.Amount);
        if (withdrawal.IsFailure)
            return withdrawal;

        target.Deposit(command.Amount);

        // Kaynak hesabın giden kaydı
        await transactionRepository.AddAsync(
            new ATM.Domain.Entities.Transaction(
                TransactionType.Transfer, command.Amount, source.Balance, source.Id,
                $"Transfer → {target.AccountNumber}", target.Id),
            cancellationToken);

        // Hedef hesabın gelen kaydı
        await transactionRepository.AddAsync(
            new ATM.Domain.Entities.Transaction(
                TransactionType.Transfer, command.Amount, target.Balance, target.Id,
                $"Transfer ← {source.AccountNumber}"),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
