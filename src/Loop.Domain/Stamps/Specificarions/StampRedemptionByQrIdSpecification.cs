using Loop.Domain.Specifications;

namespace Loop.Domain.Stamps.Specificarions;

public class StampRedemptionByQrIdSpecification : Specification<StampRedemption>
{
    public StampRedemptionByQrIdSpecification(Guid qrId)
        : base(sr => sr.QrId == qrId)
    {
    }
}
