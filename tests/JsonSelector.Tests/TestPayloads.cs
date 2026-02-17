namespace JsonSelector.Tests;

internal static class TestPayloads
{
    public const string SimplePayload = """
        {
          "id": 1001,
          "name": "alpha",
          "tags": []
        }
        """;

    public const string AlternatePayload = """
        {
          "ref": 2002,
          "label": "beta",
          "tags": []
        }
        """;

    public const string ArrayPayload = """
        {
          "items": [
            { "id": "A1", "kind": "x", "code": "10" },
            { "id": "B2", "kind": "y", "code": "20" },
            { "id": "C3", "kind": "x", "code": "30" }
          ]
        }
        """;

    public const string NestedPayload = """
        {
          "data": {
            "id": 1001,
            "items": [
              { "id": "A1", "kind": "x", "code": "10" }
            ]
          }
        }
        """;
}
