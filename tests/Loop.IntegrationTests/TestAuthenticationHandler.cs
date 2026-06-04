using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Loop.IntegrationTests;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IntegrationTest";
    private const string HeaderName = "X-Test-Auth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>();

        switch (values.ToString())
        {
            case "user":
                claims.Add(new Claim("user_id", Guid.NewGuid().ToString()));
                break;
            case "shop-admin":
                claims.Add(new Claim("shop_admin_id", Guid.NewGuid().ToString()));
                break;
            case "both":
                claims.Add(new Claim("user_id", Guid.NewGuid().ToString()));
                claims.Add(new Claim("shop_admin_id", Guid.NewGuid().ToString()));
                break;
            default:
                return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}