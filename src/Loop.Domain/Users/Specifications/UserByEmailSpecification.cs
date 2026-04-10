using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Common;
using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(Email email)
           : base(user => user.Email == email)
    {
    }
}

