using Loop.Domain.Specifications;

namespace Loop.Domain.Offers.Specifications;

public class OfferRedemptionByQrIdSpecification : Specification<OfferRedemption>
{
    public OfferRedemptionByQrIdSpecification(Guid qrId)
        : base(r => r.QrId == qrId)
    {
    }
}
