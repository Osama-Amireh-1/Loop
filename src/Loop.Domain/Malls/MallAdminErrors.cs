using Loop.SharedKernel;

namespace Loop.Domain.Malls;

public static class MallAdminErrors
{
    public static Error NotFound(Guid mallAdminId) => Error.NotFound(
        "Malls.AdminNotFound",
        $"The mall admin with the Id = '{mallAdminId}' was not found");
}


