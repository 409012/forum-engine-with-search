using FEwS.Forums.API.Authentication;
using FEwS.Forums.API.Models;
using FEwS.Forums.Domain.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Domain.UseCases.SignOn;

namespace FEwS.Forums.API.Controllers;

[ApiController]
[Route("account")]
public class AccountController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SignOnAsync(
        [FromBody] SignOnRequest request,
        CancellationToken cancellationToken)
    {
        IIdentity identity = await mediator.Send(new SignOnCommand(request.UserName, request.Password), cancellationToken);
        return Ok(identity);
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignInAsync(
        [FromBody] SignInRequest request,
        [FromServices] IAuthTokenStorage tokenStorage,
        CancellationToken cancellationToken)
    {
        (IIdentity identity, string token) = await mediator.Send(
            new SignInCommand(request.UserName, request.Password), cancellationToken);
        tokenStorage.Store(HttpContext, token);
        return Ok(identity);
    }
}