using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class UserStampsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserStampCards(Guid userId, CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetUserStampCards.Query(userId);

        var userCardesResult = await dispatcher.Dispatch(query, cancellationToken);

        return userCardesResult.IsSuccess ? Ok(userCardesResult.Value) : BadRequest(userCardesResult.Error);
    }
}


