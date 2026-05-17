using ATM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) => _accountService = accountService;

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        var result = await _accountService.GetBalanceAsync(accountId);
        return Ok(result);
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo()
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        var result = await _accountService.GetAccountInfoAsync(accountId);
        return Ok(result);
    }
}
