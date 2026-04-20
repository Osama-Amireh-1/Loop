using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Stamps.Specificarions;

public class StampTransactionByUserSpecification:Specification<StampTransaction>
{

    public StampTransactionByUserSpecification(Guid userId)
        : base(st => st.UserId == userId)
    {
    }
}
