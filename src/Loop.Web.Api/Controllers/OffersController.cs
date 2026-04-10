using Loop.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class OffersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("{MallId}/GetOffersByShopCategory")]
    public async Task<IActionResult> GetOfferByShopCategoryAsync(Guid MallId, CancellationToken cancellationToken)
    {
        var query = new Application.Offers.Query.GetOffersByShopCategory.Query(MallId);

        var userCardesResult = await dispatcher.Dispatch(query, cancellationToken);

        return userCardesResult.IsSuccess ? Ok(userCardesResult.Value) : BadRequest(userCardesResult.Error);
    }
}

