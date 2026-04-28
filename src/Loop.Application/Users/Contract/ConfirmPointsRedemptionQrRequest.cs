using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Users.Contract;

public sealed class ConfirmPointsRedemptionQrRequest
{
    [Required]
    public required string QrCodeData { get; init; }
}
