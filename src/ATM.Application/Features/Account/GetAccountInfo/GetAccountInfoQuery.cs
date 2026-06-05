using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Errors;

namespace ATM.Application.Features.Account.GetAccountInfo;

public sealed record GetAccountInfoQuery(int AccountId) : IQuery<AccountInfoResponse>;

public sealed record AccountInfoResponse(
    string AccountNumber,
    string AccountType,
    decimal Balance,
    string OwnerName,
    DateTime CreatedAt);

public sealed class GetAccountInfoQueryHandler(IAccountRepository accountRepository)
    : IQueryHandler<GetAccountInfoQuery, AccountInfoResponse>
{
    public async Task<Result<AccountInfoResponse>> Handle(GetAccountInfoQuery query, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null)
            return Result.Failure<AccountInfoResponse>(AccountErrors.NotFound);

        return new AccountInfoResponse(
            account.AccountNumber,
            account.AccountType.ToString(),
            account.Balance,
            account.User.FullName,
            account.CreatedAt);
    }
}
