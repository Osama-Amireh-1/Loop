using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Application.Users.Contract;

public class RegisterUserParams
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    public string Phone { get; set; } = null!;

    [Required]
    public string Gender { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public sealed class RegisterUserParamsValidator : AbstractValidator<RegisterUserParams>
{
    public RegisterUserParamsValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Phone).NotEmpty();
        RuleFor(c => c.Gender).NotEmpty();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
