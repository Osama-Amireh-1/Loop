namespace Loop.Application.Users.Contract;

public sealed record ForgotPasswordParams(string Email);

public sealed record ResetPasswordParams(string Email, string ResetToken, string NewPassword);

