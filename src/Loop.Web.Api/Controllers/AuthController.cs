using Loop.Application.Abstractions.Messaging;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginUserParams request, CancellationToken cancellationToken)
    {
        LoginUser.LoginUserCommand command = new(request.Email, request.Password);

        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenParams request, CancellationToken cancellationToken)
    {
        RefreshToken.RefreshTokenCommand command = new(request.RefreshToken);

        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        LogoutUser.LogoutUserCommand command = new();

        Result result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordParams request, CancellationToken cancellationToken)
    {
        ForgotPassword.ForgotPasswordCommand command = new(request.Email);

        Result<string> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? Accepted(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordParams request, CancellationToken cancellationToken)
    {
        ResetPassword.ResetPasswordCommand command = new(request.Email, request.ResetToken, request.NewPassword);

        Result result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
