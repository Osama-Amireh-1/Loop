using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Stamps.Specificarions;

public class StampRedemptionByUserSpecification:Specification<StampRedemption>
{
    public StampRedemptionByUserSpecification(Guid userId)
    : base(st => st.UserId == userId)
    {
    }
}
