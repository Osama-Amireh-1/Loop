using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Loop.Application.Users.Contract;

public sealed class UpdateUserParams
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? ProfileImageUrl { get; set; }
}

public sealed class UpdateUserParamsValidator : AbstractValidator<UpdateUserParams>
{
    public UpdateUserParamsValidator()
    {
        RuleFor(c => c)
            .Must(c =>
                !string.IsNullOrWhiteSpace(c.FirstName) ||
                !string.IsNullOrWhiteSpace(c.LastName) ||
                !string.IsNullOrWhiteSpace(c.Phone) ||
                !string.IsNullOrWhiteSpace(c.ProfileImageUrl))
            .WithMessage("At least one field must be provided.");

        RuleFor(c => c.FirstName)
            .MaximumLength(100)
            .When(c => c.FirstName is not null);

        RuleFor(c => c.LastName)
            .MaximumLength(100)
            .When(c => c.LastName is not null);

        RuleFor(c => c.Phone)
            .MaximumLength(20)
            .When(c => c.Phone is not null);

        RuleFor(c => c.ProfileImageUrl)
            .MaximumLength(512)
            .When(c => c.ProfileImageUrl is not null);
    }
}
