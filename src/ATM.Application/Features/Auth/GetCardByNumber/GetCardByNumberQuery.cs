using ATM.Application.Abstractions.Messaging;
using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Common;
using ATM.Domain.Errors;

namespace ATM.Application.Features.Auth.GetCardByNumber;

/// <summary>
/// Kart takıldığında PIN istenmeden önce kartı doğrular ve sahibinin bilgilerini getirir
/// (Blazor ATM akışındaki "kart takma" adımı için).
/// </summary>
public sealed record GetCardByNumberQuery(string CardNumber) : IQuery<CardLookupResponse>;

public sealed record CardLookupResponse(
    int CardId,
    int AccountId,
    string AccountNumber,
    string CardholderName,
    decimal Balance);

public sealed class GetCardByNumberQueryHandler(ICardRepository cardRepository)
    : IQueryHandler<GetCardByNumberQuery, CardLookupResponse>
{
    public async Task<Result<CardLookupResponse>> Handle(GetCardByNumberQuery query, CancellationToken cancellationToken)
    {
        var card = await cardRepository.GetByCardNumberAsync(query.CardNumber, cancellationToken);

        if (card is null)
            return Result.Failure<CardLookupResponse>(CardErrors.NotFound);

        if (card.IsBlocked)
            return Result.Failure<CardLookupResponse>(CardErrors.Blocked);

        return new CardLookupResponse(
            card.Id,
            card.AccountId,
            card.Account.AccountNumber,
            card.User.FullName,
            card.Account.Balance);
    }
}
