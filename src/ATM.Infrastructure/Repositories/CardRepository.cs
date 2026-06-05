using ATM.Application.Abstractions.Persistence;
using ATM.Domain.Entities;
using ATM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure.Repositories;

public sealed class CardRepository(AppDbContext context) : ICardRepository
{
    public Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
        => context.Cards
            .Include(c => c.User)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber, cancellationToken);

    public Task<Card?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Cards
            .Include(c => c.User)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
