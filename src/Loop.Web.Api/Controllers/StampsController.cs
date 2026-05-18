using Loop.Application.Abstractions.Messaging;
using Loop.Application.Stamps.Command;
using Loop.Application.Stamps.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/stamps")]
[ApiController]
[Authorize]
public class StampsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(List<GetStampsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStamps([FromQuery] Guid mallId, [FromQuery] Guid shopId, CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetStamps.Query(mallId, shopId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("completed")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(List<GetComletedStampsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComletedStamps(CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetComletedStamps.Query();
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("{stampId:guid}/redemption-qr")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(GenerateStampRedemptionQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateRedemptionQr(Guid stampId, CancellationToken cancellationToken)
    {
        var command = new GenerateStampRedemptionQr.Command(stampId);
        var result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("redemption-qr/confirm")]
    [Authorize(Policy = "ShopAdminOnly")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmRedemptionQr([FromBody] ConfirmStampRedemptionQrRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmStampRedemptionQr.Command(request.QrId);
        var result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("active")]
    [Authorize(Policy = "ShopAdminOnly")]
    [ProducesResponseType(typeof(List<GetActiveShopStampsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveShopStamps(CancellationToken cancellationToken)
    {
        var query = new Application.Stamps.Query.GetActiveShopStamps.Query();
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("{stampId:guid}/collection-qr")]
    [Authorize(Policy = "ShopAdminOnly")]
    [ProducesResponseType(typeof(GenerateStampCollectQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateCollectQr(Guid stampId, [FromQuery] int stampsCount, CancellationToken cancellationToken)
    {
        var command = new GenerateStampCollectQr.Command(stampId, stampsCount <= 0 ? 1 : stampsCount);
        var result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("collection-qr/scan")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(ScanStampCollectQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScanCollectQr([FromBody] ScanStampCollectQrRequest request, CancellationToken cancellationToken)
    {
        var command = new ScanStampCollectQr.Command(request.QrId);
        var result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
