using Loop.Domain.Specifications;

namespace Loop.Domain.QRCode.Specifications;

public class QrCodeByDataAndUserSpecification : Specification<QrCode>
{
    public QrCodeByDataAndUserSpecification(string qrCodeData, Guid userId)
        : base(q => q.QrCodeData == qrCodeData && q.UserId == userId)
    {
    }
}
