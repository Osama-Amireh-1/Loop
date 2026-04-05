using SharedKernel;

namespace Domain.Stamps;

public static class StampTransactionErrors
{
    public static Error NotFound(Guid stampTransactionId) => Error.NotFound(
        "Stamps.TransactionNotFound",
        $"The stamp transaction with the Id = '{stampTransactionId}' was not found");
}
