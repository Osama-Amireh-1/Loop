using Loop.Domain.Specifications;

namespace Loop.Domain.Receipts.Specifications;

public sealed class ReceiptByImageHashSpecification : Specification<Receipt>
{
    public ReceiptByImageHashSpecification(string imageHash)
        : base(r => r.ImageHash == imageHash)
    {
    }
}
