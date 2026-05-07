using System.Net;
using System.Text;
using Shouldly;

namespace Loop.IntegrationTests;

public sealed class ApiIntegrationTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    [Fact]
    public async Task SwaggerDocument_ShouldBeServed()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.ShouldContain("\"openapi\"");
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/shops");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        using var client = factory.CreateClient();
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
