using Loop.Application.Abstractions.Messaging;
using Loop.Application.Offers.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OffersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Loop.Application.Offers.Contract.GetOffersByShopCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByShopCategory(
        [FromQuery] Guid mallId,
        CancellationToken cancellationToken)
    {
        var query = new GetOffersByShopCategory.Query(mallId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{mallId:guid}/{shopId:guid}")]
    [ProducesResponseType(typeof(List<Loop.Application.Offers.Contract.GetOffersByShopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByShop(
         Guid mallId,
         Guid shopId,
        CancellationToken cancellationToken)
    {
        var query = new GetOffersByShop.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
