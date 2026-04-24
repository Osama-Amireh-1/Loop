using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Loop.Application.Shops.Contract;

public sealed class RefreshShopAdminTokenParams
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}

public sealed class RefreshShopAdminTokenParamsValidator : AbstractValidator<RefreshShopAdminTokenParams>
{
    public RefreshShopAdminTokenParamsValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}
