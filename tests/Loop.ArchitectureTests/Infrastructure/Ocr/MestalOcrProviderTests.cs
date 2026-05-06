using System.Net;
using System.Net.Http;
using System.Text;
using Loop.Infrastructure.Ocr;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Loop.ArchitectureTests.Infrastructure.Ocr;

public class MestalOcrProviderTests
{
    [Fact]
    public async Task ProcessAsync_ShouldReturnFailure_WhenHttpRequestFails()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var httpClient = new HttpClient(handler);
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mestal:OcrEndpoint"] = "https://example.com/ocr",
                ["Mestal:ApiKey"] = "test-key"
            })
            .Build();

        var provider = new MestalOcrProvider(configuration, httpClient);

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.HttpFailure");
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
