using System.Text.Json.Nodes;

namespace JsonSelector;

internal static class JsonPathEvaluator
{
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
}
