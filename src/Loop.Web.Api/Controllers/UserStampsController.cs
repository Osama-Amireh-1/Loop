using Loop.Application.Abstractions.Messaging;
using Loop.Application.Stamps.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/users/{userId:guid}/stamp-cards")]
[ApiController]
[Authorize]
public class UserStampCardsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GetUserStampCardsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserStampCards(CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetUserStampCards.Query();
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
