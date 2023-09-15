using System.Text.RegularExpressions;

namespace Build.Common.Integration;

internal static class StringExtensions
{
    private static readonly Regex PascalToKebabCaseRegex = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string ToKebabCaseLower(this string name)
    {
        return PascalToKebabCaseRegex.Replace(name, "-$1")
            .Trim()
            .ToLower();
    }
    
    public static Uri Combine(this Uri baseUrl, string relativeUrl)
    {
        var separator = "/";
        var left = baseUrl.OriginalString.TrimEnd('/');
        var right = relativeUrl.TrimStart('/');
        
        if (left.Contains('?') || right.StartsWith('?'))
        {
            separator = "";
        }

        return new Uri(left + separator + right);
    }
}