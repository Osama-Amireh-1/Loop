using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Users.Contract;

public sealed class GeneratePointsRedemptionQrRequest
{
    [Required]
    public required int PointsToRedeem { get; init; }
}
