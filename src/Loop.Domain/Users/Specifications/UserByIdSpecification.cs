using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public class UserByIdSpecification:Specification<User>
{
    public UserByIdSpecification(Guid id):base(x=>x.UserId==id)
    {
    }
}

