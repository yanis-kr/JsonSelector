namespace JsonSelector;

/// <summary>
/// Service for querying JSON payloads using JSONPath expressions (RFC 9535).
/// Supports both simple paths and filter expressions for eligibility checks and value extraction.
/// </summary>
public interface IJsonSelector
{
    /// <summary>
    /// Checks if the selector matches at least one node in the JSON payload.
    /// Use for MatchContent eligibility: event is eligible when Any returns true.
    /// </summary>
    bool Any(string json, string selector);

    /// <summary>
    /// Extracts the first matching value as string.
    /// Use for AccountIdPath, CustomId2Path when value is string-like.
    /// </summary>
    string? FirstString(string json, string selector);

    /// <summary>
    /// Extracts the first matching value as int.
    /// Use for CustomId2Path when value is numeric (e.g. tranCode).
    /// </summary>
    int? FirstInt(string json, string selector);
}
