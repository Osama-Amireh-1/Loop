using Loop.Application.Abstractions.Messaging;
using Loop.Application.Shops.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ShopsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GetShopsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShops([FromQuery] GetShopsParams param, CancellationToken cancellationToken)
    {
        var query = new Application.Shops.Query.GetShops.Query(
            param.MallId,
            param.CategoryId,
            param.SearchTerm);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{mallId:guid}/{shopId:guid}")]
    [ProducesResponseType(typeof(GetShopByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShopById(Guid mallId, Guid shopId, CancellationToken cancellationToken)
    {
        var query = new Application.Shops.Query.GetShopById.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
