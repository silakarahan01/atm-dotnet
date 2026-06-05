using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;

namespace ATM.Application.Features.Transaction.GetHistory;

public sealed record GetHistoryQuery(int AccountId, int Count = 10) : IQuery<IReadOnlyList<TransactionResponse>>;

public sealed record TransactionResponse(
    int Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);

public sealed class GetHistoryQueryHandler(ITransactionRepository transactionRepository)
    : IQueryHandler<GetHistoryQuery, IReadOnlyList<TransactionResponse>>
{
    public async Task<Result<IReadOnlyList<TransactionResponse>>> Handle(GetHistoryQuery query, CancellationToken cancellationToken)
    {
        var transactions = await transactionRepository.GetByAccountIdAsync(query.AccountId, query.Count, cancellationToken);

        IReadOnlyList<TransactionResponse> response = transactions
            .Select(t => new TransactionResponse(
                t.Id, t.Type.ToString(), t.Amount, t.BalanceAfter, t.Description, t.CreatedAt))
            .ToList();

        return Result.Success(response);
    }
}
