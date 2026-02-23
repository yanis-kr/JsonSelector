namespace JsonSelector;

/// <summary>Base type for a JSONPath path segment.</summary>
internal abstract record PathSegment;

/// <summary>Child property selector (e.g. <c>.name</c>).</summary>
internal sealed record ChildSegment(string Name) : PathSegment;

/// <summary>Filter selector (e.g. <c>[?(@.kind=='x')]</c>).</summary>
internal sealed record FilterSegment(string Expression) : PathSegment;

/// <summary>Index selector (e.g. <c>[0]</c>, <c>[1]</c>, <c>[-1]</c> for last element).</summary>
internal sealed record IndexSegment(int Index) : PathSegment;

/// <summary>Value predicate on path result (e.g. <c>== 'match'</c>, <c>!= 'x'</c>, <c>>= 5</c>).</summary>
internal sealed record ValuePredicateSegment(string Op, string Value) : PathSegment;

/// <summary>Parses JSONPath selector strings into a sequence of path segments.</summary>
internal static class JsonPathParser
{
    /// <summary>
    /// Parses a JSONPath selector into path segments.
    /// </summary>
    /// <param name="selector">The selector string (e.g. <c>$.id</c>, <c>$.items[?(@.kind=='x')].id</c>).</param>
    /// <returns>The parsed segments, or <c>null</c> if the selector is invalid.</returns>
    public static IReadOnlyList<PathSegment>? Parse(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector)) return null;

        string normalized = selector.Trim();
        if (!normalized.StartsWith('$'))
            normalized = normalized.StartsWith('.') ? "$" + normalized : "$." + normalized;

        var segments = new List<PathSegment>();
        int i = 0;

        if (i < normalized.Length && normalized[i] == '$')
            i++;

        while (i < normalized.Length)
        {
            if (normalized[i] == '.')
            {
                i++;
                if (i >= normalized.Length) return null;
                string name = ReadIdentifier(normalized, ref i);
                if (string.IsNullOrEmpty(name)) return null;
                segments.Add(new ChildSegment(name));
            }
            else if (normalized[i] == '[')
            {
                i++;
                if (i >= normalized.Length) return null;
                if (normalized[i] == '?')
                {
                    i++;
                    if (i >= normalized.Length || normalized[i] != '(') return null;
                    i++;
                    int depth = 1;
                    int start = i;
                    while (i < normalized.Length && depth > 0)
                    {
                        char c = normalized[i];
                        if (c == '(') depth++;
                        else if (c == ')') depth--;
                        i++;
                    }
                    if (depth != 0) return null;
                    string expr = normalized[start..(i - 1)].Trim();
                    if (i >= normalized.Length || normalized[i - 1] != ')' || normalized[i] != ']') return null;
                    i++;
                    segments.Add(new FilterSegment(expr));
                }
                else if (TryReadIndex(normalized, ref i, out int index))
                {
                    if (i >= normalized.Length || normalized[i] != ']') return null;
                    i++;
                    segments.Add(new IndexSegment(index));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (segments.Count > 0)
                {
                    while (i < normalized.Length && char.IsWhiteSpace(normalized[i])) i++;
                    if (i < normalized.Length)
                    {
                        var predicate = TryParseValuePredicate(normalized, ref i);
                        if (predicate is not null && i >= normalized.Length)
                        {
                            segments.Add(predicate);
                            return segments;
                        }
                    }
                }
                return null;
            }
        }

        return segments;
    }

    private static ValuePredicateSegment? TryParseValuePredicate(string s, ref int i)
    {
        if (i >= s.Length) return null;
        string? op = null;
        if (s.Length > i + 2 && s[i] == '=' && s[i + 1] == '=') { op = "=="; i += 2; }
        else if (s.Length > i + 2 && s[i] == '!' && s[i + 1] == '=') { op = "!="; i += 2; }
        else if (s.Length > i + 2 && s[i] == '>' && s[i + 1] == '=') { op = ">="; i += 2; }
        else if (s.Length > i + 1 && s[i] == '>') { op = ">"; i += 1; }
        else if (s.Length > i + 2 && s[i] == '<' && s[i + 1] == '=') { op = "<="; i += 2; }
        else if (s.Length > i + 1 && s[i] == '<') { op = "<"; i += 1; }
        if (op is null) return null;

        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        if (i >= s.Length) return null;

        if (s[i] == '\'')
        {
            int end = i + 1;
            while (end < s.Length)
            {
                if (s[end] == '\\' && end + 1 < s.Length)
                    end += 2;
                else if (s[end] == '\'')
                {
                    string value = s[(i + 1)..end].Replace("\\'", "'").Replace("\\\\", "\\");
                    i = end + 1;
                    return new ValuePredicateSegment(op, value);
                }
                else
                    end++;
            }
            return null;
        }

        if (s[i] == '-' || char.IsDigit(s[i]))
        {
            int start = i;
            if (s[i] == '-') i++;
            while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
            string value = s[start..i];
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                return new ValuePredicateSegment(op, value);
        }
        return null;
    }

    private static string ReadIdentifier(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
            i++;
        return s[start..i];
    }

    private static bool TryReadIndex(string s, ref int i, out int index)
    {
        index = 0;
        if (i >= s.Length) return false;
        bool negative = s[i] == '-';
        if (negative) i++;
        if (i >= s.Length || !char.IsDigit(s[i])) return false;
        int start = i;
        while (i < s.Length && char.IsDigit(s[i])) i++;
        if (!int.TryParse(s[start..i], out int value)) return false;
        index = negative ? -value : value;
        return true;
    }
}
