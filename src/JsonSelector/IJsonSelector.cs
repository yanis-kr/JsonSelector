namespace JsonSelector;

/// <summary>
/// Service for querying JSON payloads using JSONPath expressions (RFC 9535 subset).
/// Supports simple paths, filter expressions, and multiple-value matching for eligibility checks and value extraction.
/// </summary>
public interface IJsonSelector
{
    /// <summary>
    /// Checks if the selector matches at least one node in the JSON payload.
    /// </summary>
    /// <param name="json">The JSON string to query.</param>
    /// <param name="selector">The JSONPath selector (e.g. <c>$.id</c>, <c>$.items[?(@.kind=='x')]</c>).</param>
    /// <returns><c>true</c> if at least one node matches; otherwise <c>false</c>. Returns <c>false</c> for null, empty, or invalid JSON or selector.</returns>
    bool Any(string json, string selector);

    /// <summary>
    /// Extracts the first matching value as a string.
    /// Numbers are converted using invariant culture.
    /// </summary>
    /// <param name="json">The JSON string to query.</param>
    /// <param name="selector">The JSONPath selector.</param>
    /// <returns>The first matching value as string, or <c>null</c> if no match, invalid input, or non-scalar value.</returns>
    string? FirstString(string json, string selector);

    /// <summary>
    /// Extracts the first matching value as an integer.
    /// Supports both numeric and string values (parsed via <c>int.TryParse</c>).
    /// </summary>
    /// <param name="json">The JSON string to query.</param>
    /// <param name="selector">The JSONPath selector.</param>
    /// <returns>The first matching value as int, or <c>null</c> if no match, invalid input, or value cannot be parsed as int.</returns>
    int? FirstInt(string json, string selector);
}
