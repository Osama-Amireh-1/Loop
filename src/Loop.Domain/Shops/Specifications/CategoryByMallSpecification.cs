using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Shops;
using Loop.Domain.Specifications;

namespace Loop.Domain.Shops.Specifications;

public class CategoryByMallSpecification:Specification<Category>
{
    public CategoryByMallSpecification(Guid mallId)
        : base(category => category.MallId == mallId)
    {
    }
}

