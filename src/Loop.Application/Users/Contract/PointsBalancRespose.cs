using System;
using System.Collections.Generic;
using System.Text;

namespace Loop.Application.Users.Contract;

public record PointsBalancResponse (int TotalPoints, decimal EvaluatedValue);
