using Loop.SharedKernel;

namespace Loop.Domain.QRCode;

public static class QrCodeErrors
{
    public static Error NotFound(Guid qrId) => Error.NotFound(
        "QRCode.NotFound",
        $"The QR code with the Id = '{qrId}' was not found");

    public static readonly Error Expired = Error.Failure(
        "QRCode.Expired",
        "The QR code has expired");
}


