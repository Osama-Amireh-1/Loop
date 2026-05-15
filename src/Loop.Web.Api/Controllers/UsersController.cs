using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Loop.Application.Users.Query;
using Loop.Web.Api.Controllers.Requests;
using Loop.Web.Api.Controllers.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loop.SharedKernel;

namespace Loop.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IDispatcher dispatcher, IUserContext userContext) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserParams request, CancellationToken cancellationToken)
    {
        RegisterUser.RegisterUserCommand command = new(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Gender,
            request.Password);

        Result<Guid> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("UploadProfileImage")]
    [Authorize(Policy = "UserOnly")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfileImage(
        [FromForm] UploadUserProfileImageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ImageFile == null || request.ImageFile.Length == 0)
        {
            return BadRequest(new { error = "Image file is required and cannot be empty" });
        }

      
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(request.ImageFile.ContentType))
        {
            return BadRequest(new { error = "Invalid image format. Allowed formats: jpg, jpeg, png, gif, webp" });
        }

        const long maxFileSize = 5 * 1024 * 1024;
        if (request.ImageFile.Length > maxFileSize)
        {
            return BadRequest(new { error = "File size exceeds maximum allowed size of 5MB" });
        }

        byte[] imageContent;
        using (var memoryStream = new MemoryStream())
        {
            await request.ImageFile.CopyToAsync(memoryStream, cancellationToken);
            imageContent = memoryStream.ToArray();
        }

        var command = new UploadUserProfileImage.UploadUserProfileImageCommand(
            request.ImageFile.FileName,
            imageContent);

        Result<bool> result = await dispatcher.Dispatch(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error.Description });
        }


        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        GetUserById.GetUserByIdQuery query = new(id);

        Result<UserResponse> result = await dispatcher.Dispatch(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("GetLoginUserDetials")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoginUserDetails(CancellationToken cancellationToken)
    {
        GetUserById.GetUserByIdQuery query = new(userContext.UserId);

        Result<UserResponse> result = await dispatcher.Dispatch(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("GetUserPointsBalance")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(PointsBalancResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPointsBalance([FromQuery] Guid mallId, CancellationToken cancellationToken)
    {
        GetUserPointsBalance.Query query = new(mallId);
        Result<PointsBalancResponse> result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("redeem/points/confirm")]
    [Authorize(Policy = "ShopAdminOnly")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPointsRedemptionQr([FromBody] ConfirmPointsRedemptionQrRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmPointsRedemptionQr.Command(request.QrId);
        Result<bool> result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("AddPoints")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPointsToCurrentUser(
        [FromBody] AddPointsToUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = userContext.UserId;

        var command = new AddPointsToUser.Command(userId, request.Amount);
        Result<bool> result = await dispatcher.Dispatch(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type == ErrorType.NotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPut("UpdateMyProfile ")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserParams request, CancellationToken cancellationToken)
    {
        var command = new UpdateUser.UpdateUserCommand(
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Gender,
            request.ProfileImageUrl);

        Result<UserResponse> result = await dispatcher.Dispatch(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("redeem/points/qr")]
    [Authorize(Policy = "UserOnly")]
    [ProducesResponseType(typeof(GeneratePointsRedemptionQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePointsRedemptionQr([FromBody] GeneratePointsRedemptionQrRequest request, CancellationToken cancellationToken)
    {
        var command = new GeneratePointsRedemptionQr.Command(request.PointsToRedeem);
        Result<GeneratePointsRedemptionQrResponse> result = await dispatcher.Dispatch(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
