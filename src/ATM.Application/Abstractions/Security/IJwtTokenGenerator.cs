using ATM.Domain.Entities;

namespace ATM.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(Card card);
}
