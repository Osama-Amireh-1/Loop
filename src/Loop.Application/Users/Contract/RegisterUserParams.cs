using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using FluentValidation;

namespace Application.Users.Contract;

public class RegisterUserParams
{
    [Required]
    public string Email { get; set; }=null!;
    [Required]

    public string FirstName { get; set; } =null!;
    [Required]
    public string LastName { get; set; } =null!;
    [Required]
    public string Password { get; set; }=null!;
}

public sealed class RegisterUserParamsValidator : AbstractValidator<RegisterUserParams>
{
    public RegisterUserParamsValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
