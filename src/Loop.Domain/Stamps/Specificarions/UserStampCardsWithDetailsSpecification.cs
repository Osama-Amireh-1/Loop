using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;
using Loop.Domain.Stamps;

namespace Loop.Domain.Stamps.Specifications;

public class UserStampCardsWithDetailsSpecification:Specification<UserStampCard>
{
    public UserStampCardsWithDetailsSpecification(Guid userId)
        : base(usc => usc.UserId == userId)
    {
        AddInclude(usc => usc.Stamp);
        AddInclude(usc => usc.Stamp.Shop);
    }
}

