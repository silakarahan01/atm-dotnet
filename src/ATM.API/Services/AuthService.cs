using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ATM.API.Configuration;
using ATM.Core.DTOs.Auth;
using ATM.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ATM.API.Services;

public class AuthService
{
    private readonly ICardRepository _cardRepo;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ICardRepository cardRepo, IOptions<JwtSettings> jwtOptions)
    {
        _cardRepo = cardRepo;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var card = await _cardRepo.GetByCardNumberAsync(request.CardNumber)
            ?? throw new KeyNotFoundException("Kart bulunamadı.");

        if (card.IsBlocked)
            throw new UnauthorizedAccessException("Kart bloke edilmiştir. Lütfen bankanızla iletişime geçin.");

        if (!BCrypt.Net.BCrypt.Verify(request.Pin, card.PinHash))
        {
            card.FailedAttempts++;
            if (card.FailedAttempts >= 3)
                card.IsBlocked = true;

            await _cardRepo.UpdateAsync(card);

            var remaining = 3 - card.FailedAttempts;
            if (card.IsBlocked)
                throw new UnauthorizedAccessException("3 hatalı girişten sonra kart bloke edildi.");

            throw new UnauthorizedAccessException($"Hatalı PIN. {remaining} deneme hakkınız kaldı.");
        }

        card.FailedAttempts = 0;
        await _cardRepo.UpdateAsync(card);

        var token = GenerateToken(card);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            CardholderName = $"{card.User.FirstName} {card.User.LastName}"
        };
    }

    public async Task ChangePinAsync(int cardId, ChangePinRequestDto request)
    {
        var card = await _cardRepo.GetByIdAsync(cardId)
            ?? throw new KeyNotFoundException("Kart bulunamadı.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPin, card.PinHash))
            throw new UnauthorizedAccessException("Mevcut PIN hatalı.");

        if (request.NewPin.Length != 4 || !request.NewPin.All(char.IsDigit))
            throw new InvalidOperationException("Yeni PIN 4 haneli bir sayı olmalıdır.");

        card.PinHash = BCrypt.Net.BCrypt.HashPassword(request.NewPin);
        await _cardRepo.UpdateAsync(card);
    }

    private string GenerateToken(ATM.Core.Entities.Card card)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", card.UserId.ToString()),
            new Claim("cardId", card.Id.ToString()),
            new Claim("accountId", card.AccountId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
