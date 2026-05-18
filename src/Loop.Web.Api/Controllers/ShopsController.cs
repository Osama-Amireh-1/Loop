using Loop.Application.Abstractions.Messaging;
using Loop.Application.Shops.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/shops")]
[ApiController]
[Authorize(Policy = "UserOnly")]
public class ShopsController(IDispatcher dispatcher) : ControllerBase
{
    private const string MallIdHeaderName = "X-Mall-Id";

    [HttpGet]
    [ProducesResponseType(typeof(List<GetShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShops([FromHeader(Name = MallIdHeaderName)] Guid mallId, [FromQuery] GetShopsParams param, CancellationToken cancellationToken)
    {
        var query = new Application.Shops.Query.GetShops.Query(
            mallId,
            param.CategoryId,
            param.SearchTerm);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{shopId:guid}")]
    [ProducesResponseType(typeof(GetShopByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShopById([FromHeader(Name = MallIdHeaderName)] Guid mallId, Guid shopId, CancellationToken cancellationToken)
    {
        var query = new Application.Shops.Query.GetShopById.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
