using System.Security.Claims;
using ATM.API.Extensions;
using ATM.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    /// <summary>JWT içindeki hesap kimliği (kimliği doğrulanmış istekler için).</summary>
    protected int AccountId => int.Parse(User.FindFirstValue("accountId")!);

    /// <summary>JWT içindeki kart kimliği.</summary>
    protected int CardId => int.Parse(User.FindFirstValue("cardId")!);

    protected IActionResult HandleFailure(Error error) => this.ToProblem(error);
}
