using System;
using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Stamps.Contract;

public sealed class ScanStampCollectQrRequest
{
    [Required]
    public required Guid QrId { get; init; }
}
