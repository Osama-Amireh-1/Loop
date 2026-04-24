using Loop.Application.Abstractions.Messaging;
using Loop.Application.Shops.Command;
using Loop.Application.Shops.Contract;
using Loop.Application.Users.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShopAdminsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginShopAdminParams request, CancellationToken cancellationToken)
    {
        var command = new LoginShopAdmin.LoginShopAdminCommand(request.Email, request.Password);
        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshShopAdminTokenParams request, CancellationToken cancellationToken)
    {
        var command = new RefreshShopAdminToken.RefreshShopAdminTokenCommand(request.RefreshToken);
        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
