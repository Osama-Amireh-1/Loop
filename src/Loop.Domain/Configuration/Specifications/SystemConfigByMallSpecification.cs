using System;
using System.Collections.Generic;
using System.Text;
using Loop.Domain.Specifications;

namespace Loop.Domain.Configuration.Specifications;

public class SystemConfigByMallSpecification:Specification<SystemConfig>
{
    public SystemConfigByMallSpecification(Guid mallId):base(x=>x.MallId==mallId)
    {
    }
}
