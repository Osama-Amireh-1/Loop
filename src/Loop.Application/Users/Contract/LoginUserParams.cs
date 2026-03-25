using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using FluentValidation;

namespace Application.Users.Contract;

public class LoginUserParams
{
    [Required]
public string Email { get; set; } = null!;
    [Required]

    public string Password { get; set; } = null!;
}

public sealed class LoginUserParamsValidator : AbstractValidator<LoginUserParams>
{
    public LoginUserParamsValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
