using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;
using Loop.Domain.Tiers;

namespace Loop.Domain.Tiers.Specifications;

public class TierByOrderSpecification :Specification<Tier>
{
    public TierByOrderSpecification(int order) : base(t => t.TierOrder == order) { }
}

