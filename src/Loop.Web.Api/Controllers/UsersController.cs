using Loop.Application.Abstractions.Messaging;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Loop.Application.Users.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        GetUserById.GetUserByIdQuery query = new(id);

        Result<UserResponse> result = await dispatcher.Dispatch(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("GetUserPointsBalance")]
    [ProducesResponseType(typeof(PointsBalancResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPointsBalance([FromQuery] Guid mallId, CancellationToken cancellationToken)
    {
        GetUserPointsBalance.Query query = new(mallId);
        Result<PointsBalancResponse> result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
