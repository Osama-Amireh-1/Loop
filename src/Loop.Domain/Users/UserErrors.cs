using Loop.SharedKernel;

namespace Loop.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error PhoneNotUnique = Error.Conflict(
        "Users.PhoneNotUnique",
        "The provided phone number is not unique");

    public static readonly Error InvalidGender = Error.Failure(
        "Users.InvalidGender",
        "The provided gender value is invalid");

    public static readonly Error InsufficientPoints = Error.Failure(
        "Users.InsufficientPoints",
        "The user does not have enough points for this operation");

    public static readonly Error InvalidRefreshToken = Error.Failure(
        "Users.InvalidRefreshToken",
        "The provided refresh token is invalid.");

    public static readonly Error RefreshTokenExpired = Error.Failure(
        "Users.RefreshTokenExpired",
        "The provided refresh token is expired.");

    public static readonly Error InvalidPasswordResetToken = Error.Failure(
        "Users.InvalidPasswordResetToken",
        "The provided password reset token is invalid or expired.");

    public static readonly Error PasswordResetTokenExpired = Error.Failure(
        "Users.PasswordResetTokenExpired",
        "The password reset token has expired. Please request a new one.");
}


