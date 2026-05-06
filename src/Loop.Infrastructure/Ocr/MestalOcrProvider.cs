using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Loop.Application.Abstractions.Ocr;
using Loop.Application.Receipts.Contract;
using Loop.SharedKernel;
using Microsoft.Extensions.Configuration;

namespace Loop.Infrastructure.Ocr;

internal sealed class MestalOcrProvider : IReceiptOcrProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly Error HttpFailureError = new(
        "ReceiptOcr.HttpFailure",
        "The OCR service request failed.",
        ErrorType.Problem);

    private static readonly Error InvalidResponseError = new(
        "ReceiptOcr.InvalidResponse",
        "The OCR service returned an invalid response.",
        ErrorType.Problem);

    private readonly string _url;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public MestalOcrProvider(IConfiguration configuration, HttpClient httpClient)
    {
        _url = configuration["Mestal:OcrEndpoint"] ?? throw new InvalidOperationException("Missing Mestal OCR endpoint configuration");
        _apiKey = configuration["Mestal:ApiKey"] ?? throw new InvalidOperationException("Missing Mestal API key");
        _httpClient = httpClient;
    }

    public async Task<Result<ReceiptOcrResult>> ProcessAsync(Stream imageStream, string contentType = "image/jpeg", CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer, cancellationToken);
        var base64 = Convert.ToBase64String(buffer.ToArray());
        var mimeType = NormalizeMimeType(contentType);

        var requestBody = new
        {
            model = "mistral-ocr-latest",
            document = new
            {
                type = "image_url",
                image_url = $"data:{mimeType};base64,{base64}"
            },
            document_annotation_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "Receipt",
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            storeName = new { type = "string" },
                            items = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        name = new { type = "string" },
                                        quantity = new { type = "number" },
                                        unitPrice = new { type = "number" },
                                        totalPrice = new { type = "number" }
                                    }
                                }
                            },
                            subtotal = new { type = "number" },
                            currency = new { type = "string" }
                        }
                    }
                }
            },
            document_annotation_prompt = ReceiptPrompts.SystemPrompt
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<ReceiptOcrResult>(HttpFailureError);
            }

            using var doc = JsonDocument.Parse(responseString);
            var rawText = ExtractMarkdown(doc.RootElement);
            var structuredJson = ExtractJsonPayload(doc.RootElement);

            ReceiptOcrResult? parsed = null;
            if (!string.IsNullOrWhiteSpace(structuredJson))
            {
                parsed = TryDeserialize(structuredJson);
            }

            if (parsed is null && !string.IsNullOrWhiteSpace(rawText))
            {
                parsed = new ReceiptOcrResult
                {
                    StoreName = ExtractFirstLine(rawText),
                };
            }

            if (parsed is null)
            {
                return Result.Failure<ReceiptOcrResult>(InvalidResponseError);
            }

            return Result.Success(new ReceiptOcrResult
            {
                StoreName = parsed.StoreName,
                MerchantName = parsed.MerchantName ?? parsed.StoreName,
                Items = parsed.Items ?? [],
                Subtotal = parsed.Subtotal,
                Currency = parsed.Currency,
                IsPendingReview = parsed.IsPendingReview,
                RawText = parsed.RawText
            });
        }
        catch (HttpRequestException)
        {
            return Result.Failure<ReceiptOcrResult>(HttpFailureError);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<ReceiptOcrResult>(HttpFailureError);
        }
        catch (JsonException)
        {
            return Result.Failure<ReceiptOcrResult>(InvalidResponseError);
        }
    }

    private static ReceiptOcrResult? TryDeserialize(string json)
    {
        try
        {
            var cleanJson = TrimCodeFences(json);
            return JsonSerializer.Deserialize<ReceiptOcrResult>(cleanJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractJsonPayload(JsonElement root)
    {
        if (TryGetPropertyValue(root, "document_annotation", out var annotation))
            return annotation;

        if (TryGetPropertyValue(root, "annotation", out var fallbackAnnotation))
            return fallbackAnnotation;

        return null;
    }

    private static string? ExtractMarkdown(JsonElement root)
    {
        if (root.TryGetProperty("pages", out var pages) && pages.ValueKind == JsonValueKind.Array && pages.GetArrayLength() > 0)
        {
            var firstPage = pages[0];
            if (firstPage.TryGetProperty("markdown", out var markdown) && markdown.ValueKind == JsonValueKind.String)
                return markdown.GetString();
        }

        if (TryGetPropertyValue(root, "text", out var text))
            return text;

        return null;
    }

    private static bool TryGetPropertyValue(JsonElement root, string propertyName, out string? value)
    {
        value = null;

        if (!root.TryGetProperty(propertyName, out var element))
            return false;

        value = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object or JsonValueKind.Array => element.GetRawText(),
            _ => null
        };

        return !string.IsNullOrWhiteSpace(value);
    }

    private static string? ExtractFirstLine(string text)
    {
        foreach (var line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }

        return null;
    }

    private static string TrimCodeFences(string text)
    {
        var cleanJson = text.Trim();

        if (!cleanJson.StartsWith("```", StringComparison.Ordinal))
            return cleanJson;

        var firstNewLine = cleanJson.IndexOf('\n');
        if (firstNewLine >= 0)
            cleanJson = cleanJson[(firstNewLine + 1)..];

        var lastFence = cleanJson.LastIndexOf("```", StringComparison.Ordinal);
        if (lastFence >= 0)
            cleanJson = cleanJson[..lastFence];

        return cleanJson.Trim();
    }

    private static string NormalizeMimeType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return "image/jpeg";

        return contentType.Split(';', 2)[0].Trim();
    }
}
