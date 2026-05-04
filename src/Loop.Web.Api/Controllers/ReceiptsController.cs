using Loop.Application.Abstractions.Messaging;
using Loop.Application.Receipts.Command;
using Loop.Application.Receipts.Contract;
using Loop.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "UserOnly")]
public class ReceiptsController : ControllerBase
{
    private const string MallIdHeaderName = "X-Mall-Id";

    private readonly IDispatcher _dispatcher;

    public ReceiptsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("ocr")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReceiptOcrResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ocr([FromHeader(Name = MallIdHeaderName)] Guid mallId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("file is required");

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        var command = new ProcessReceiptOcr.Command(
            mallId,
            file.FileName,
            file.ContentType ?? "image/jpeg",
            buffer.ToArray());

        var result = await _dispatcher.Dispatch(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error.Type == ErrorType.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }
}
