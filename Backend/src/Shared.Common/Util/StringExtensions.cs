namespace Shared.Common.Util;

/// <summary>
/// Provides extension methods for string manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a PascalCase or camelCase string to snake_case.
    /// </summary>
    public static string ConvertToSnakeCase(this string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        string result = System.Text.RegularExpressions.Regex
            .Replace(name, "([a-z0-9])([A-Z])", "$1_$2")
            .ToLower();

        return result;
    }
}