using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Loop.Application.Users.Contract;

public sealed class RefreshTokenParams
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}

public sealed class RefreshTokenParamsValidator : AbstractValidator<RefreshTokenParams>
{
    public RefreshTokenParamsValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}

