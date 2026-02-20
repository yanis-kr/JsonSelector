namespace JsonSelector;

/// <summary>Base type for a JSONPath path segment.</summary>
internal abstract record PathSegment;

/// <summary>Child property selector (e.g. <c>.name</c>).</summary>
internal sealed record ChildSegment(string Name) : PathSegment;

/// <summary>Filter selector (e.g. <c>[?(@.kind=='x')]</c>).</summary>
internal sealed record FilterSegment(string Expression) : PathSegment;

/// <summary>Index selector (e.g. <c>[0]</c>, <c>[1]</c>, <c>[-1]</c> for last element).</summary>
internal sealed record IndexSegment(int Index) : PathSegment;

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
                return null;
            }
        }

        return segments;
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
