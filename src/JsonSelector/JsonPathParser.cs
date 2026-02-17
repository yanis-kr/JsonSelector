namespace JsonSelector;

internal abstract record PathSegment;

internal sealed record ChildSegment(string Name) : PathSegment;

internal sealed record FilterSegment(string Expression) : PathSegment;

internal static class JsonPathParser
{
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
                if (normalized[i] != '?') return null;
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
}
