using ATM.API.Contracts;
using ATM.Application.Features.Transaction.Deposit;
using ATM.Application.Features.Transaction.GetHistory;
using ATM.Application.Features.Transaction.Transfer;
using ATM.Application.Features.Transaction.Withdraw;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[Authorize]
public sealed class TransactionController(ISender sender) : ApiController(sender)
{
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] AmountRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DepositCommand(AccountId, request.Amount), cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = $"{request.Amount:N2} TL hesabınıza yatırıldı." })
            : HandleFailure(result.Error);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] AmountRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new WithdrawCommand(AccountId, request.Amount), cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = $"{request.Amount:N2} TL hesabınızdan çekildi." })
            : HandleFailure(result.Error);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new TransferCommand(AccountId, request.TargetAccountNumber, request.Amount), cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = $"{request.Amount:N2} TL {request.TargetAccountNumber} hesabına transfer edildi." })
            : HandleFailure(result.Error);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetHistoryQuery(AccountId, count), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }
}
