using Loop.Domain.QRCode;
using Loop.Domain.Specifications;

namespace Loop.Domain.QRCode.Specifications;

public class QrCodeByPKSpecification : Specification<QrCode>
{
    public QrCodeByPKSpecification(Guid qrId)
        : base(q => q.QrId == qrId)
    {
    }
}
