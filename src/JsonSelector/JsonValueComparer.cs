using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonSelector;

/// <summary>Shared comparison and string conversion for JSON values.</summary>
internal static class JsonValueComparer
{
    /// <summary>
    /// Converts a JSON node to a string for comparison, using invariant culture for numbers.
    /// </summary>
    public static string? NodeToString(JsonNode? node)
    {
        if (node is null) return null;
        if (node is JsonValue jv)
        {
            return jv.GetValueKind() switch
            {
                JsonValueKind.String => jv.GetValue<string>(),
                JsonValueKind.Number => jv.GetValue<decimal>().ToString(CultureInfo.InvariantCulture),
                _ => node.ToString()
            };
        }
        return node.ToString();
    }

    /// <summary>
    /// Applies a comparison operator to two string values. Handles ==, !=, &gt;=, &gt;, &lt;=, &lt;.
    /// </summary>
    /// <param name="left">Left operand (e.g. from NodeToString or literal).</param>
    /// <param name="right">Right operand.</param>
    /// <param name="op">Operator: ==, !=, &gt;=, &gt;, &lt;=, &lt;.</param>
    /// <returns>True if the comparison holds; otherwise false.</returns>
    public static bool ApplyComparison(string? left, string? right, string op)
    {
        if (left is null && right is null)
            return op == "==";
        if (left is null || right is null)
            return op == "!=";
        if (op is "==" or "!=")
            return op switch { "==" => left == right, "!=" => left != right, _ => false };
        if (op is ">=" or ">" or "<=" or "<")
        {
            if (!decimal.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal leftNum) ||
                !decimal.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rightNum))
                return false;
            return op switch
            {
                ">=" => leftNum >= rightNum,
                ">" => leftNum > rightNum,
                "<=" => leftNum <= rightNum,
                "<" => leftNum < rightNum,
                _ => false
            };
        }
        return false;
    }
}
