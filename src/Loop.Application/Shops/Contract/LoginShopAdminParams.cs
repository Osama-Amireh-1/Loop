using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Loop.Application.Shops.Contract;

public sealed class LoginShopAdminParams
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public sealed class LoginShopAdminParamsValidator : AbstractValidator<LoginShopAdminParams>
{
    public LoginShopAdminParamsValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
