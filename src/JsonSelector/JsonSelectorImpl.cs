using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonSelector;

/// <summary>Default implementation of <see cref="IJsonSelector"/> using a custom JSONPath engine.</summary>
internal sealed class JsonSelectorImpl : IJsonSelector
{
    /// <inheritdoc />
    public bool Any(string json, string selector)
    {
        var path = JsonPathParser.Parse(selector);
        if (path is null) return false;
        JsonNode? node = ParseJson(json);
        if (node is null) return false;
        var matches = JsonPathEvaluator.Evaluate(path, node);
        return matches.Any(m => m is not null);
    }

    /// <inheritdoc />
    public string? FirstString(string json, string selector)
    {
        JsonNode? value = FirstValue(json, selector);
        if (value is not JsonValue jv) return null;
        return jv.GetValueKind() switch
        {
            JsonValueKind.String => jv.GetValue<string>(),
            JsonValueKind.Number => jv.GetValue<decimal>().ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => null
        };
    }

    /// <inheritdoc />
    public int? FirstInt(string json, string selector)
    {
        JsonNode? value = FirstValue(json, selector);
        if (value is not JsonValue jv) return null;
        return jv.GetValueKind() switch
        {
            JsonValueKind.Number => (int)jv.GetValue<decimal>(),
            JsonValueKind.String => int.TryParse(jv.GetValue<string>(), out int parsed) ? parsed : null,
            _ => null
        };
    }

    private static JsonNode? ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonNode.Parse(json); }
        catch (JsonException) { return null; }
    }

    private static JsonNode? FirstValue(string json, string selector)
    {
        var path = JsonPathParser.Parse(selector);
        if (path is null) return null;
        JsonNode? node = ParseJson(json);
        if (node is null) return null;
        return JsonPathEvaluator.Evaluate(path, node).FirstOrDefault();
    }
}
