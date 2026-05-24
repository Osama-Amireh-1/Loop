using Loop.Application.Abstractions.Messaging;
using Loop.Application.Shops.Contract;
using Loop.SharedKernel;
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
    [ProducesResponseType(typeof(PaginatedShopsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShops(
        [FromHeader(Name = MallIdHeaderName)] Guid mallId,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new Application.Shops.Query.GetShops.Query(
            mallId,
            categoryId,
            searchTerm,
            pageNumber,
            pageSize);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error),
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
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
