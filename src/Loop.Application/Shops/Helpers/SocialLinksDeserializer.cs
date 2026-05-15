#pragma warning disable CA1304

using System.Globalization;
using System.Text.Json;
using Loop.Application.Shops.Contract;

namespace Loop.Application.Shops.Helpers;

public static class SocialLinksDeserializer
{
    /// <summary>
    /// Deserializes JSON social links string to a list of SocialLinkDto.
    /// Expected format: {"facebook": "www.facebook.com/...", "instagram": "@..."}
    /// </summary>
    public static List<SocialLinkDto> Deserialize(string? socialLinksJson)
    {
        var result = new List<SocialLinkDto>();

        if (string.IsNullOrWhiteSpace(socialLinksJson))
        {
            return result;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(socialLinksJson);
            var root = jsonDoc.RootElement;

            foreach (var property in root.EnumerateObject())
            {
                var name = CamelCaseToTitle(property.Name);
                var link = property.Value.GetString();

                if (!string.IsNullOrWhiteSpace(link))
                {
                    result.Add(new SocialLinkDto
                    {
                        Name = name,
                        Link = link
                    });
                }
            }
        }
        catch (JsonException)
        {
            // If parsing fails, return empty list
            return result;
        }

        return result;
    }

    /// <summary>
    /// Converts camelCase to Title Case (e.g., "instagram" -> "Instagram")
    /// </summary>
    private static string CamelCaseToTitle(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
        {
            return camelCase;
        }

        return char.ToUpper(camelCase[0], CultureInfo.InvariantCulture) + camelCase[1..];
    }
}

#pragma warning restore CA1304
