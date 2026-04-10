using Loop.SharedKernel;

namespace Loop.Domain.Malls;

public static class MallErrors
{
    public static Error NotFound(Guid mallId) => Error.NotFound(
        "Malls.NotFound",
        $"The mall with the Id = '{mallId}' was not found");

    public static readonly Error NameNotUnique = Error.Conflict(
        "Malls.NameNotUnique",
        "A mall with the specified name already exists");

    public static Error AdminNotFound(Guid mallAdminId) => Error.NotFound(
        "Malls.AdminNotFound",
        $"The mall admin with the Id = '{mallAdminId}' was not found");

    public static readonly Error AdminEmailNotUnique = Error.Conflict(
        "Malls.AdminEmailNotUnique",
        "A mall admin with the specified email already exists");

    public static readonly Error AdminPhoneNotUnique = Error.Conflict(
        "Malls.AdminPhoneNotUnique",
        "A mall admin with the specified phone already exists");
}


