using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public class UserByPKSpecification:Specification<User>
{
    public UserByPKSpecification(Guid id):base(x=>x.UserId==id)
    {
    }
}

