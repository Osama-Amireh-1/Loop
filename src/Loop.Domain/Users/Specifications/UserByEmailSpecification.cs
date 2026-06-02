using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Common;
using Loop.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Loop.Domain.Users.Specifications;

public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(Email email)
           : base(user => EF.Functions.ILike(user.Email.Value, email.Value))
    {
    }
}

