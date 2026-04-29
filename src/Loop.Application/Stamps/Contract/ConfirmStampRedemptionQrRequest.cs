using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Stamps.Contract;

public sealed class ConfirmStampRedemptionQrRequest
{
    [Required]
    public required Guid QrId { get; init; }
}
