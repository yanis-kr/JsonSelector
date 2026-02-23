using System.Text.Json.Nodes;

namespace JsonSelector;

/// <summary>Evaluates parsed JSONPath segments against a JSON document.</summary>
internal static class JsonPathEvaluator
{
    /// <summary>
    /// Evaluates a path against a JSON root node and returns all matching nodes.
    /// </summary>
    /// <param name="path">The parsed path segments.</param>
    /// <param name="root">The root JSON node (typically the document).</param>
    /// <returns>All nodes that match the path, in document order.</returns>
    public static IEnumerable<JsonNode?> Evaluate(IReadOnlyList<PathSegment> path, JsonNode? root)
    {
        if (path.Count == 0) return [root];
        return EvaluateFrom(path, 0, root);
    }

    private static IEnumerable<JsonNode?> EvaluateFrom(IReadOnlyList<PathSegment> path, int index, JsonNode? current)
    {
        if (current is null || index >= path.Count)
            return [current];

        PathSegment seg = path[index];
        if (seg is ChildSegment child)
        {
            var nextNodes = ResolveChild(current, child.Name);
            if (index + 1 >= path.Count)
                return nextNodes;
            return nextNodes.SelectMany(n => EvaluateFrom(path, index + 1, n));
        }

        if (seg is FilterSegment filter)
        {
            var candidates = ResolveFilter(current, filter.Expression);
            if (index + 1 >= path.Count)
                return candidates;
            return candidates.SelectMany(n => EvaluateFrom(path, index + 1, n));
        }

        if (seg is IndexSegment indexSeg)
        {
            var nextNodes = ResolveIndex(current, indexSeg.Index);
            if (index + 1 >= path.Count)
                return nextNodes;
            return nextNodes.SelectMany(n => EvaluateFrom(path, index + 1, n));
        }

        if (seg is ValuePredicateSegment pred)
        {
            if (!ValuePredicateMatches(current, pred))
                return [];
            if (index + 1 >= path.Count)
                return [current];
            return EvaluateFrom(path, index + 1, current);
        }

        return [];
    }

    private static IEnumerable<JsonNode?> ResolveChild(JsonNode? node, string name)
    {
        if (node is JsonObject obj && obj.TryGetPropertyValue(name, out var child))
            return [child];
        return [];
    }

    private static IEnumerable<JsonNode?> ResolveFilter(JsonNode? node, string expression)
    {
        if (node is not JsonArray arr)
            return [];

        var filterNode = FilterExpressionParser.Parse(expression, null);
        if (filterNode is null)
            return [];

        var results = new List<JsonNode?>();
        foreach (var item in arr)
        {
            if (item is null) continue;
            if (FilterExpressionParser.Evaluate(filterNode, item))
                results.Add(item);
        }
        return results;
    }

    private static IEnumerable<JsonNode?> ResolveIndex(JsonNode? node, int index)
    {
        if (node is not JsonArray arr)
            return [];

        int effectiveIndex = index >= 0 ? index : arr.Count + index;
        if (effectiveIndex < 0 || effectiveIndex >= arr.Count)
            return [];

        var item = arr[effectiveIndex];
        return [item];
    }

    private static bool ValuePredicateMatches(JsonNode? node, ValuePredicateSegment pred)
    {
        string? nodeVal = NodeToString(node);
        string rightVal = pred.Value;

        if (pred.Op is "==" or "!=")
        {
            bool eq = nodeVal == rightVal;
            return pred.Op == "==" ? eq : !eq;
        }

        if (pred.Op is ">=" or ">" or "<=" or "<")
        {
            if (!decimal.TryParse(nodeVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal leftNum) ||
                !decimal.TryParse(rightVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rightNum))
                return false;
            return pred.Op switch
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

    private static string? NodeToString(JsonNode? node)
    {
        if (node is null) return null;
        if (node is JsonValue jv)
        {
            return jv.GetValueKind() switch
            {
                System.Text.Json.JsonValueKind.String => jv.GetValue<string>(),
                System.Text.Json.JsonValueKind.Number => jv.GetValue<decimal>().ToString(System.Globalization.CultureInfo.InvariantCulture),
                _ => node.ToString()
            };
        }
        return node.ToString();
    }
}
