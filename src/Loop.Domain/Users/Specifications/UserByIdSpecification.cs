using System;
using System.Collections.Generic;
using System.Text;
using Domain.Specifications;

namespace Domain.Users.Specifications;

public class UserByIdSpecification:Specification<User>
{
    public UserByIdSpecification(Guid id):base(x=>x.Id==id)
    {
    }
}
