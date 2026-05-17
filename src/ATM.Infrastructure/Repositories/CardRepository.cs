using ATM.Core.Entities;
using ATM.Core.Interfaces;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _context;

    public CardRepository(AppDbContext context) => _context = context;

    public async Task<Card?> GetByCardNumberAsync(string cardNumber)
        => await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

    public async Task<Card?> GetByIdAsync(int id)
        => await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
    }
}
