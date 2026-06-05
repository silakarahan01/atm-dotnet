using ATM.Domain.Entities;

namespace ATM.Application.Abstractions.Persistence;

public interface ICardRepository
{
    Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<Card?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
