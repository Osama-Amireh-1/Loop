namespace Loop.Application.Users.Contract;

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken);

