using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Loop.Application.Interfaces;
using Loop.Application.Receipts.Contract;
using Loop.Application.Receipts.Services;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specificarions;
using Microsoft.EntityFrameworkCore;

namespace Loop.Infrastructure.Receipts.MerchantMatcher;

public sealed class LocalMerchantMatcher(IReadOnlyRepository<Shop> shopReadRepository) : IMerchantMatcher
{
    private const double MinimumMatchScore = 0.35;

    public async Task<ReceiptOcrResult> MatchAsync(Guid mallId, ReceiptOcrResult ocrResult, CancellationToken cancellationToken = default)
    {
        var receipt = Normalize(ocrResult);
        var receiptName = Normalize(receipt.StoreName ?? receipt.MerchantName);

        if (string.IsNullOrWhiteSpace(receiptName))
            return receipt;

        var shops = await shopReadRepository
            .Find(new ActiveShopsByMallSpecification(mallId))
            .Select(shop => new { shop.ShopId, shop.Name })
            .ToListAsync(cancellationToken);

        var bestMatch = shops
            .Select(shop => new
            {
                shop.ShopId,
                shop.Name,
                Score = GetSimilarityScore(receiptName, Normalize(shop.Name))
            })
            .OrderByDescending(match => match.Score)
            .FirstOrDefault();

        if (bestMatch is null)
            return receipt;

        var pendingReview = bestMatch.Score < MinimumMatchScore;

        return new ReceiptOcrResult
        {
            StoreName = receipt.StoreName,
            MerchantName = receipt.MerchantName,
            Items = receipt.Items,
            Subtotal = receipt.Subtotal,
            Currency = receipt.Currency,
            MatchedShopId = bestMatch.ShopId,
            MatchedShopName = bestMatch.Name,
            MatchScore = Math.Round(bestMatch.Score, 3),
            IsPendingReview = pendingReview
        };
    }

    private static ReceiptOcrResult Normalize(ReceiptOcrResult result) => new()
    {
        StoreName = result.StoreName?.Trim(),
        MerchantName = string.IsNullOrWhiteSpace(result.MerchantName) ? result.StoreName?.Trim() : result.MerchantName.Trim(),
        Items = result.Items ?? [],
        Subtotal = result.Subtotal,
        Currency = result.Currency?.Trim(),
        MatchedShopId = result.MatchedShopId,
        MatchedShopName = result.MatchedShopName,
        MatchScore = result.MatchScore,
        IsPendingReview = result.IsPendingReview
    };

    private static double GetSimilarityScore(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0;

        if (source == target)
            return 1;

        if (source.Contains(target, StringComparison.Ordinal) || target.Contains(source, StringComparison.Ordinal))
            return 0.95;

        var sourceTokens = Tokenize(source);
        var targetTokens = Tokenize(target);

        if (sourceTokens.Count == 0 || targetTokens.Count == 0)
            return 0;

        var intersection = sourceTokens.Intersect(targetTokens).Count();
        var overlapScore = intersection / (double)Math.Max(sourceTokens.Count, targetTokens.Count);
        var lengthScore = 1 - Math.Min(1d, Math.Abs(source.Length - target.Length) / (double)Math.Max(source.Length, target.Length));

        return overlapScore * 0.7 + lengthScore * 0.3;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var normalized = Regex.Replace(text, @"[^\p{L}\p{N}]+", " ");

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(char.ToUpperInvariant(c));
        }

        return Regex.Replace(builder.ToString(), @"[^\p{L}\p{N}]+", " ").Trim();
    }
}
