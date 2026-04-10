using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) => Error.NotFound(
        "Shops.CategoryNotFound",
        $"The category with the Id = '{categoryId}' was not found");
}


