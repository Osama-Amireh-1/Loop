using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;
using Loop.Domain.Stamps;

namespace Loop.Domain.Stamps.Specifications;

public class ActiveUserStampCardsWithDetailsSpecification:Specification<UserStampCard>
{
    public ActiveUserStampCardsWithDetailsSpecification(Guid userId)
        : base(usc => usc.UserId == userId && usc.Stamp.IsActive && usc.Stamp.StartDate <= DateTime.Now && usc.Stamp.EndDate >= DateTime.Now)
    {
        AddInclude(usc => usc.Stamp);
        AddInclude(usc => usc.Stamp.Shop);
    }
}

