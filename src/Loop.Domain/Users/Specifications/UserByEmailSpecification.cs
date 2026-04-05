using System;
using System.Collections.Generic;
using System.Text;
using Domain.Specifications;

namespace Domain.Users.Specifications;

public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(string email)
        : base(user => string.Equals(user.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase))
    {
    }
}
