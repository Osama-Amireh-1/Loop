using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Loop.Application.Users.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class UsersController(IDispatcher dispatcher, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserParams request, CancellationToken cancellationToken)
    {
        RegisterUser.RegisterUserCommand command = new(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Gender,
            request.Password);

        Result<Guid> result = await dispatcher.Dispatch(command, cancellationToken);
        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginUserParams request, CancellationToken cancellationToken)
    {
        LoginUser.LoginUserCommand command = new(request.Email, request.Password);

        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenParams request, CancellationToken cancellationToken)
    {
        RefreshToken.RefreshTokenCommand command = new(request.RefreshToken);

        Result<AuthTokensResponse> result = await dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        LogoutUser.LogoutUserCommand command = new();

        Result result = await dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordParams request, CancellationToken cancellationToken)
    {
        ForgotPassword.ForgotPasswordCommand command = new(request.Email);

        Result<string> result = await dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordParams request, CancellationToken cancellationToken)
    {
        ResetPassword.ResetPasswordCommand command = new(request.Email, request.ResetToken, request.NewPassword);

        Result result = await dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        GetUserById.GetUserByIdQuery query = new(id);

        Result<UserResponse> result = await dispatcher.Dispatch(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}


