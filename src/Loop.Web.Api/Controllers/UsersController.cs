using Application.Abstractions.Messaging;
using Application.Interfaces;
using Application.Users.Command;
using Application.Users.Contract;
using Application.Users.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]  // ← all endpoints require auth by default
public class UsersController(IDispatcher dispatcher, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]  // ← public, no auth needed
    public async Task<IActionResult> Register([FromBody] RegisterUserParams request, CancellationToken cancellationToken)
    {
        RegisterUser.RegisterUserCommand command = new(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password);

        Result<Guid> result = await dispatcher.Dispatch(command, cancellationToken);
        if(result.IsSuccess)
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

        Result<string> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        GetUserById.GetUserByIdQuery query = new(id);

        Result<UserResponse> result = await dispatcher.Dispatch(query, cancellationToken);
        

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
