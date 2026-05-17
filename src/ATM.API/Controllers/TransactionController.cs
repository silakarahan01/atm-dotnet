using ATM.API.Services;
using ATM.Core.DTOs.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionController(TransactionService transactionService)
        => _transactionService = transactionService;

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto request)
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        await _transactionService.DepositAsync(accountId, request.Amount);
        return Ok(new { message = $"{request.Amount:N2} TL hesabınıza yatırıldı." });
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto request)
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        await _transactionService.WithdrawAsync(accountId, request.Amount);
        return Ok(new { message = $"{request.Amount:N2} TL hesabınızdan çekildi." });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        await _transactionService.TransferAsync(accountId, request.TargetAccountNumber, request.Amount);
        return Ok(new { message = $"{request.Amount:N2} TL {request.TargetAccountNumber} hesabına transfer edildi." });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int count = 10)
    {
        var accountId = int.Parse(User.FindFirst("accountId")!.Value);
        var result = await _transactionService.GetHistoryAsync(accountId, count);
        return Ok(result);
    }
}
