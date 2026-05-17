using ATM.Core.Entities;

namespace ATM.Core.Interfaces;

public interface ICardRepository
{
    Task<Card?> GetByCardNumberAsync(string cardNumber);
    Task<Card?> GetByIdAsync(int id);
    Task UpdateAsync(Card card);
}
