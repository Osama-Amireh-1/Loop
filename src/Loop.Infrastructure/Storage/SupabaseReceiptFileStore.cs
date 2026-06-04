using Loop.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;

namespace Loop.Infrastructure.Storage;

internal sealed class SupabaseReceiptFileStore : IReceiptFileStore
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _bucketName;

    public SupabaseReceiptFileStore(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url is not configured");
        _supabaseKey = configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase:Key is not configured");
        _bucketName =  "receipts";
    }

    public async Task<string> SaveAsync(Guid mallId, Guid receiptId, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var objectPath = $"malls/{mallId:N}/receipts/{receiptId:N}{extension}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{objectPath}")
            {
                Content = new ByteArrayContent(content),
            };

            request.Headers.Add("authorization", $"Bearer {_supabaseKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to upload receipt to Supabase: {response.StatusCode} - {errorContent}");
            }

            var receiptUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{objectPath}";
            return receiptUrl;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error uploading receipt to Supabase: {ex.Message}", ex);
        }
    }
}
