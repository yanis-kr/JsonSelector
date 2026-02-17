# JsonSelector

A lightweight .NET library for querying JSON payloads using [JSONPath](https://www.rfc-editor.org/rfc/rfc9535.html) expressions (RFC 9535). Supports simple paths and filter expressions for eligibility checks and value extraction.

## Features

- **Any** — Check if a selector matches at least one node
- **FirstString** — Extract the first matching value as string
- **FirstInt** — Extract the first matching value as int

## Installation

Add a project reference to the JsonSelector project:

```xml
<ProjectReference Include="path\to\src\JsonSelector\JsonSelector.csproj" />
```

## Usage

```csharp
using JsonSelector;

// Register with DI
services.AddJsonSelector();

// Inject and use
var selector = serviceProvider.GetRequiredService<IJsonSelector>();

// Check if path exists
bool exists = selector.Any(json, "$.account");

// Extract values
string? accountId = selector.FirstString(json, "$.accountId");
int? tranCode = selector.FirstInt(json, "$.journalEntries[?(@.entryType=='credit')].tranCode");
```

## Requirements

- .NET 8.0
