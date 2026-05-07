using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Loop.Infrastructure.Ocr;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Loop.ArchitectureTests.Infrastructure.Ocr;

public class MestalOcrProviderTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenEndpointConfigurationIsMissing()
    {
        IConfiguration configuration = BuildConfiguration(apiKey: "test-key");
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler);

        var exception = Should.Throw<InvalidOperationException>(() => new MestalOcrProvider(configuration, httpClient));

        exception.Message.ShouldBe("Missing Mestal OCR endpoint configuration");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenApiKeyConfigurationIsMissing()
    {
        IConfiguration configuration = BuildConfiguration(endpoint: "https://example.com/ocr");
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler);

        var exception = Should.Throw<InvalidOperationException>(() => new MestalOcrProvider(configuration, httpClient));

        exception.Message.ShouldBe("Missing Mestal API key");
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnFailure_WhenHttpRequestFails()
    {
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.HttpFailure");
    }

    [Fact]
    public async Task ProcessAsync_ShouldParseStructuredAnnotation_WhenAnnotationContainsValidJson()
    {
        const string annotationJson = """
        {"storeName":"Carrefour","items":[{"name":"Milk","quantity":1,"unitPrice":2.5,"totalPrice":2.5}],"subtotal":2.5,"currency":"JOD"}
        """;

        var responseBody = JsonSerializer.Serialize(new
        {
            document_annotation = annotationJson
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StoreName.ShouldBe("Carrefour");
        result.Value.MerchantName.ShouldBe("Carrefour");
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Subtotal.ShouldBe(2.5m);
        result.Value.Currency.ShouldBe("JOD");
    }

    [Fact]
    public async Task ProcessAsync_ShouldParseStructuredAnnotation_WhenFallbackAnnotationPropertyExists()
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            annotation = """
                         {"storeName":"Mega Store","items":[],"subtotal":100.0,"currency":"USD"}
                         """
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StoreName.ShouldBe("Mega Store");
        result.Value.MerchantName.ShouldBe("Mega Store");
        result.Value.Subtotal.ShouldBe(100m);
        result.Value.Currency.ShouldBe("USD");
    }

    [Fact]
    public async Task ProcessAsync_ShouldParseStructuredAnnotation_WhenAnnotationIsWrappedInCodeFences()
    {
        const string annotationJson = """
        ```json
        {"storeName":"Code Fence Shop","items":[],"subtotal":12.0,"currency":"JOD"}
        ```
        """;

        var responseBody = JsonSerializer.Serialize(new
        {
            document_annotation = annotationJson
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StoreName.ShouldBe("Code Fence Shop");
        result.Value.MerchantName.ShouldBe("Code Fence Shop");
    }

    [Fact]
    public async Task ProcessAsync_ShouldFallbackToMarkdownFirstLine_WhenStructuredAnnotationIsMissing()
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            pages = new[]
            {
                new { markdown = "Mega Mart\nLine item details" }
            }
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StoreName.ShouldBe("Mega Mart");
        result.Value.MerchantName.ShouldBe("Mega Mart");
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_ShouldFallbackToTextProperty_WhenPagesAreMissing()
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            text = "Plain Text Store\nSecond line"
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StoreName.ShouldBe("Plain Text Store");
        result.Value.MerchantName.ShouldBe("Plain Text Store");
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnInvalidResponse_WhenAnnotationCannotBeDeserialized()
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            document_annotation = "{invalid-json}"
        });

        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.InvalidResponse");
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnInvalidResponse_WhenResponseBodyIsNotJson()
    {
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{not-json", Encoding.UTF8, "application/json")
            }));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.InvalidResponse");
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnFailure_WhenHttpClientThrowsHttpRequestException()
    {
        using var handler = new StubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("boom"));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.HttpFailure");
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnFailure_WhenRequestIsCanceled()
    {
        using var handler = new StubHttpMessageHandler((_, _) =>
            throw new TaskCanceledException("canceled"));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var result = await provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", cancellationTokenSource.Token);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ReceiptOcr.HttpFailure");
    }

    [Fact]
    public async Task ProcessAsync_ShouldRethrowTaskCanceledException_WhenTokenWasNotCanceled()
    {
        using var handler = new StubHttpMessageHandler((_, _) =>
            throw new TaskCanceledException("canceled"));
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        await Should.ThrowAsync<TaskCanceledException>(() =>
            provider.ProcessAsync(new MemoryStream(Encoding.UTF8.GetBytes("fake-image")), "image/jpeg", CancellationToken.None));
    }

    [Fact]
    public async Task ProcessAsync_ShouldNormalizeMimeType_WhenContentTypeContainsParameters()
    {
        string? capturedBody = null;
        var responseBody = JsonSerializer.Serialize(new
        {
            document_annotation = """
                                 {"storeName":"Mime Shop","items":[],"subtotal":10.0,"currency":"USD"}
                                 """
        });

        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        });
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(
            new MemoryStream(Encoding.UTF8.GetBytes("fake-image")),
            "image/png; charset=utf-8",
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        capturedBody.ShouldNotBeNull();
        capturedBody.ShouldContain("data:image/png;base64,");
    }

    [Fact]
    public async Task ProcessAsync_ShouldDefaultMimeTypeToJpeg_WhenContentTypeIsBlank()
    {
        string? capturedBody = null;
        var responseBody = JsonSerializer.Serialize(new
        {
            document_annotation = """
                                 {"storeName":"Default Mime Shop","items":[],"subtotal":9.0,"currency":"USD"}
                                 """
        });

        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            capturedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        });
        using var context = new TestProviderContext(handler);
        var provider = context.Provider;

        var result = await provider.ProcessAsync(
            new MemoryStream(Encoding.UTF8.GetBytes("fake-image")),
            " ",
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        capturedBody.ShouldNotBeNull();
        capturedBody.ShouldContain("data:image/jpeg;base64,");
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => sendAsync(request, cancellationToken);
    }

    private sealed class TestProviderContext : IDisposable
    {
        private readonly HttpClient _httpClient;

        public TestProviderContext(HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler);
            IConfiguration configuration = BuildConfiguration("https://example.com/ocr", "test-key");
            Provider = new MestalOcrProvider(configuration, _httpClient);
        }

        public MestalOcrProvider Provider { get; }

        public void Dispose() => _httpClient.Dispose();
    }

    private static IConfiguration BuildConfiguration(string? endpoint = null, string? apiKey = null)
    {
        var values = new Dictionary<string, string?>();

        if (endpoint is not null)
        {
            values["Mestal:OcrEndpoint"] = endpoint;
        }

        if (apiKey is not null)
        {
            values["Mestal:ApiKey"] = apiKey;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
