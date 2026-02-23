using System.Text.Json.Nodes;

namespace JsonSelector;

/// <summary>Base type for a filter expression node.</summary>
internal abstract record FilterNode;

/// <summary>Comparison expression (e.g. <c>==</c>, <c>!=</c>).</summary>
internal sealed record FilterComparison(FilterNode Left, string Op, FilterNode Right) : FilterNode;

/// <summary>Logical expression (e.g. <c>&amp;&amp;</c>, <c>||</c>).</summary>
internal sealed record FilterLogical(FilterNode Left, string Op, FilterNode Right) : FilterNode;

/// <summary>Path expression (e.g. <c>@.field</c>).</summary>
internal sealed record FilterPath(string[] Segments) : FilterNode;

/// <summary>String literal in single quotes.</summary>
internal sealed record FilterStringLiteral(string Value) : FilterNode;

/// <summary>Numeric literal for comparisons.</summary>
internal sealed record FilterNumericLiteral(string Value) : FilterNode;

/// <summary>isOneOf function: value equals any of the given strings.</summary>
internal sealed record FilterIsOneOf(FilterPath Path, IReadOnlyList<string> Values) : FilterNode;

/// <summary>Parses and evaluates filter expressions within JSONPath selectors.</summary>
internal static class FilterExpressionParser
{
    /// <summary>
    /// Parses a filter expression string into an expression tree.
    /// </summary>
    /// <param name="expression">The filter expression (e.g. <c>@.kind=='x' &amp;&amp; @.code=='10'</c>).</param>
    /// <param name="currentContext">Unused; reserved for future context-aware parsing.</param>
    /// <returns>The parsed expression, or <c>null</c> if invalid.</returns>
    public static FilterNode? Parse(string expression, JsonNode? currentContext)
    {
        if (string.IsNullOrWhiteSpace(expression)) return null;
        var span = expression.Trim().AsSpan();
        return ParseOr(ref span, currentContext);
    }

    private static FilterNode? ParseOr(ref ReadOnlySpan<char> s, JsonNode? ctx)
    {
        var left = ParseAnd(ref s, ctx);
        if (left is null) return null;
        s = s.TrimStart();
        while (s.StartsWith("||"))
        {
            s = s[2..].TrimStart();
            var right = ParseAnd(ref s, ctx);
            if (right is null) return null;
            left = new FilterLogical(left, "||", right);
            s = s.TrimStart();
        }
        return left;
    }

    private static FilterNode? ParseAnd(ref ReadOnlySpan<char> s, JsonNode? ctx)
    {
        var left = ParseComparison(ref s, ctx);
        if (left is null) return null;
        s = s.TrimStart();
        while (s.StartsWith("&&"))
        {
            s = s[2..].TrimStart();
            var right = ParseComparison(ref s, ctx);
            if (right is null) return null;
            left = new FilterLogical(left, "&&", right);
            s = s.TrimStart();
        }
        return left;
    }

    private static FilterNode? ParseComparison(ref ReadOnlySpan<char> s, JsonNode? ctx)
    {
        s = s.TrimStart();
        if (s.Length > 0 && s[0] == '(')
        {
            s = s[1..];
            var inner = ParseOr(ref s, ctx);
            if (inner is null) return null;
            s = s.TrimStart();
            if (s.Length == 0 || s[0] != ')') return null;
            s = s[1..];
            return inner;
        }

        if (TryParseIsOneOf(ref s, ctx, out var isOneOf))
            return isOneOf;

        var left = ParsePrimary(ref s, ctx);
        if (left is null) return null;
        s = s.TrimStart();

        if (s.StartsWith("=="))
        {
            s = s[2..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, "==", right);
        }
        if (s.StartsWith("!="))
        {
            s = s[2..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, "!=", right);
        }
        if (s.StartsWith(">="))
        {
            s = s[2..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, ">=", right);
        }
        if (s.StartsWith(">") && (s.Length == 1 || s[1] != '='))
        {
            s = s[1..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, ">", right);
        }
        if (s.StartsWith("<="))
        {
            s = s[2..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, "<=", right);
        }
        if (s.StartsWith("<") && (s.Length == 1 || s[1] != '='))
        {
            s = s[1..].TrimStart();
            var right = ParsePrimary(ref s, ctx);
            if (right is null) return null;
            return new FilterComparison(left, "<", right);
        }

        return left;
    }

    private static bool TryParseIsOneOf(ref ReadOnlySpan<char> s, JsonNode? ctx, out FilterNode? result)
    {
        result = null;
        if (!s.StartsWith("isOneOf", StringComparison.OrdinalIgnoreCase)) return false;
        var rest = s[7..];
        if (rest.Length == 0 || (rest[0] != '(' && !char.IsWhiteSpace(rest[0]))) return false;
        rest = rest.TrimStart();
        if (rest.Length == 0 || rest[0] != '(') return false;
        rest = rest[1..].TrimStart();

        var path = ParsePrimary(ref rest, ctx) as FilterPath;
        if (path is null) return false;
        rest = rest.TrimStart();
        if (rest.Length == 0 || rest[0] != ',') return false;
        rest = rest[1..].TrimStart();

        var values = new List<string>();
        while (rest.Length > 0)
        {
            var lit = ParsePrimary(ref rest, ctx) as FilterStringLiteral;
            if (lit is null) return false;
            values.Add(lit.Value);
            rest = rest.TrimStart();
            if (rest.Length == 0) break;
            if (rest[0] == ')') break;
            if (rest[0] != ',') return false;
            rest = rest[1..].TrimStart();
        }
        if (rest.Length == 0 || rest[0] != ')' || values.Count < 2) return false;
        rest = rest[1..];

        s = rest;
        result = new FilterIsOneOf(path, values);
        return true;
    }

    private static FilterNode? ParsePrimary(ref ReadOnlySpan<char> s, JsonNode? ctx)
    {
        s = s.TrimStart();
        if (s.Length == 0) return null;

        if (s[0] == '\'')
        {
            int end = 1;
            while (end < s.Length)
            {
                if (s[end] == '\\' && end + 1 < s.Length)
                    end += 2;
                else if (s[end] == '\'')
                {
                    string value = UnescapeString(s[1..end].ToString());
                    s = s[(end + 1)..];
                    return new FilterStringLiteral(value);
                }
                else
                    end++;
            }
            return null;
        }

        if (s.Length > 0 && (s[0] == '-' || char.IsDigit(s[0])))
        {
            int start = 0;
            int i = s[0] == '-' ? 1 : 0;
            if (i >= s.Length || !char.IsDigit(s[i])) return null;
            while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
            string value = s[start..i].ToString();
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                s = s[i..];
                return new FilterNumericLiteral(value);
            }
            return null;
        }

        if (s.StartsWith("@."))
        {
            var segments = new List<string>();
            var rest = s[2..];
            while (rest.Length > 0)
            {
                rest = rest.TrimStart();
                if (rest.Length == 0) break;
                int i = 0;
                while (i < rest.Length && (char.IsLetterOrDigit(rest[i]) || rest[i] == '_')) i++;
                if (i == 0) break;
                segments.Add(rest[..i].ToString());
                rest = rest[i..];
                if (rest.Length > 0 && rest[0] == '.')
                    rest = rest[1..];
                else
                    break;
            }
            if (segments.Count == 0) return null;
            s = rest;
            return new FilterPath(segments.ToArray());
        }

        return null;
    }

    private static string UnescapeString(string s)
    {
        return s.Replace("\\'", "'").Replace("\\\\", "\\");
    }

    /// <summary>
    /// Evaluates a filter expression against a JSON node.
    /// </summary>
    /// <param name="node">The parsed filter expression.</param>
    /// <param name="context">The current JSON node (e.g. array element) to evaluate against.</param>
    /// <returns><c>true</c> if the filter matches; otherwise <c>false</c>.</returns>
    public static bool Evaluate(FilterNode node, JsonNode? context)
    {
        return node switch
        {
            FilterComparison c => EvalComparison(c, context),
            FilterLogical l => EvalLogical(l, context),
            FilterIsOneOf io => EvalIsOneOf(io, context),
            _ => false
        };
    }

    private static bool EvalComparison(FilterComparison c, JsonNode? context)
    {
        string? leftVal = GetValue(c.Left, context);
        string? rightVal = GetValue(c.Right, context);
        return JsonValueComparer.ApplyComparison(leftVal, rightVal, c.Op);
    }

    private static bool EvalLogical(FilterLogical l, JsonNode? context)
    {
        bool left = EvalToBool(l.Left, context);
        return l.Op switch
        {
            "&&" => left && EvalToBool(l.Right, context),
            "||" => left || EvalToBool(l.Right, context),
            _ => false
        };
    }

    private static bool EvalIsOneOf(FilterIsOneOf io, JsonNode? context)
    {
        string? val = GetValue(io.Path, context);
        if (val is null) return false;
        return io.Values.Contains(val);
    }

    private static bool EvalToBool(FilterNode node, JsonNode? context)
    {
        if (node is FilterComparison c)
            return EvalComparison(c, context);
        if (node is FilterLogical l)
            return EvalLogical(l, context);
        if (node is FilterIsOneOf io)
            return EvalIsOneOf(io, context);
        return false;
    }

    private static string? GetValue(FilterNode node, JsonNode? context)
    {
        if (node is FilterPath p)
            return GetPathValue(p, context);
        if (node is FilterStringLiteral s)
            return s.Value;
        if (node is FilterNumericLiteral n)
            return n.Value;
        return null;
    }

    private static string? GetPathValue(FilterPath path, JsonNode? node)
    {
        if (node is null) return null;
        foreach (string seg in path.Segments)
        {
            if (node is JsonObject obj && obj.TryGetPropertyValue(seg, out var next))
                node = next;
            else
                return null;
        }
        return JsonValueComparer.NodeToString(node);
    }
}
