using Loop.Application.Abstractions.Messaging;
using Loop.Application.Offers.Command;
using Loop.Application.Offers.Query;
using Loop.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/offers")]
[ApiController]
public class OffersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Loop.Application.Offers.Contract.GetOffersByShopCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "UserOnly")]
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
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetByShop(
         Guid mallId,
         Guid shopId,
        CancellationToken cancellationToken)
    {
        var query = new GetOffersByShop.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{offerId:guid}")]
    [ProducesResponseType(typeof(Loop.Application.Offers.Contract.GetOfferByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GetById(Guid offerId, CancellationToken cancellationToken)
    {
        var query = new GetOfferById.Query(offerId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("{offerId:guid}/redeem/qr")]
    [ProducesResponseType(typeof(GenerateOfferRedemptionQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> GenerateRedemptionQr(
        Guid offerId,
        CancellationToken cancellationToken)
    {
        var command = new GenerateOfferRedemptionQr.Command(offerId);
        var result = await dispatcher.Dispatch(command, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("redeem/confirm")]
    [Authorize(Policy = "ShopAdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmRedemption(
        [FromBody] ConfirmOfferRedemptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmOfferRedemption.Command(request.QrId);
        var result = await dispatcher.Dispatch(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { success = true });
    }
}
