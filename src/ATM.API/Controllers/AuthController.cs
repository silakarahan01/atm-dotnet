using ATM.API.Services;
using ATM.Core.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("change-pin")]
    public async Task<IActionResult> ChangePin([FromBody] ChangePinRequestDto request)
    {
        var cardId = int.Parse(User.FindFirst("cardId")!.Value);
        await _authService.ChangePinAsync(cardId, request);
        return Ok(new { message = "PIN başarıyla güncellendi." });
    }
}
