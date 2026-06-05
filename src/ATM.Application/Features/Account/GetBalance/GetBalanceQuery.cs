using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Errors;

namespace ATM.Application.Features.Account.GetBalance;

public sealed record GetBalanceQuery(int AccountId) : IQuery<BalanceResponse>;

public sealed record BalanceResponse(decimal Balance, string AccountNumber, string AccountType);

public sealed class GetBalanceQueryHandler(IAccountRepository accountRepository)
    : IQueryHandler<GetBalanceQuery, BalanceResponse>
{
    public async Task<Result<BalanceResponse>> Handle(GetBalanceQuery query, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null)
            return Result.Failure<BalanceResponse>(AccountErrors.NotFound);

        return new BalanceResponse(account.Balance, account.AccountNumber, account.AccountType.ToString());
    }
}
