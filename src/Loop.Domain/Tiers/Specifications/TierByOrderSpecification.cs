using System;
using System.Collections.Generic;
using System.Text;
using Domain.Specifications;
using Domain.Tiers;

namespace Loop.Domain.Tiers.Specifications;

public class TierByOrderSpecification :Specification<Tier>
{
    public TierByOrderSpecification(int order) : base(t => t.TierOrder == order) { }
}
