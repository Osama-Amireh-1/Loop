using Loop.Domain.Specifications;
using Loop.Domain.Stamps;

namespace Loop.Domain.Stamps.Specifications;

public class StampTransactionByQrIdSpecification : Specification<StampTransaction>
{
    public StampTransactionByQrIdSpecification(Guid qrId)
        : base(st => st.RedemptionRef == qrId)
    {
    }
}
