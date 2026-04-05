using SharedKernel;

namespace Domain.Audit;

public static class AuditLogErrors
{
    public static Error NotFound(Guid logId) => Error.NotFound(
        "Audit.NotFound",
        $"The audit log with the Id = '{logId}' was not found");
}
