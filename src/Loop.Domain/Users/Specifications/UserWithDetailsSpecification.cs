using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public class UserWithDetailsSpecification:Specification<User>
{
    public UserWithDetailsSpecification(Guid userId):base(u=>u.UserId==userId)
    {
        AddInclude(e => e.PointsBalance);
    }
}
