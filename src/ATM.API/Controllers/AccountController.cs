using ATM.Application.Features.Account.GetAccountInfo;
using ATM.Application.Features.Account.GetBalance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[Authorize]
public sealed class AccountController(ISender sender) : ApiController(sender)
{
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBalanceQuery(AccountId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAccountInfoQuery(AccountId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }
}
