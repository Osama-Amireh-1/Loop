using Loop.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StampsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStamps([FromQuery] Guid mallId, [FromQuery] Guid shopId, CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetStamps.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
