using ATM.API.Contracts;
using ATM.Application.Features.Auth.ChangePin;
using ATM.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

public sealed class AuthController(ISender sender) : ApiController(sender)
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [Authorize]
    [HttpPut("change-pin")]
    public async Task<IActionResult> ChangePin([FromBody] ChangePinRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ChangePinCommand(CardId, request.CurrentPin, request.NewPin), cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "PIN başarıyla güncellendi." })
            : HandleFailure(result.Error);
    }
}
