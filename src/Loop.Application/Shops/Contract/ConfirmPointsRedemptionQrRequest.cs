using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Shops.Contract;

public sealed class ConfirmPointsRedemptionQrRequest
{
    [Required]
    public required Guid QrId { get; init; }
}
