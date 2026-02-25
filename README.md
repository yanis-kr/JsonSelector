# JsonSelector

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue?logo=github)](https://github.com/yanis-kr/JsonSelector)

A lightweight .NET library for querying JSON payloads using [JSONPath](https://www.rfc-editor.org/rfc/rfc9535.html) expressions. Implements a subset of [RFC 9535 (JSONPath: Query Expressions for JSON)](https://www.rfc-editor.org/rfc/rfc9535.html). No external JSONPath dependencies—built on `System.Text.Json` only.

---

## Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Supported JSONPath Features](#supported-jsonpath-features)
- [Path and Selector Examples](#path-and-selector-examples)
- [Installation](#installation)
- [Usage](#usage)
- [Requirements](#requirements)
- [License](#license)

---

## Features

| Method | Description |
|--------|-------------|
| **Any** | Check if a selector matches at least one node |
| **FirstString** | Extract the first matching value as string (numbers use invariant culture) |
| **FirstInt** | Extract the first matching value as int (supports numeric and string values) |

---

## Quick Start

```csharp
using JsonSelector;

var selector = new JsonSelectorImpl();  // or inject IJsonSelector via DI

string json = """{ "id": 1001, "name": "alpha", "items": [{ "kind": "x", "code": "10" }] }""";

bool exists = selector.Any(json, "$.items[?(@.kind=='x')]");  // true
string? name = selector.FirstString(json, "$.name");           // "alpha"
int? code = selector.FirstInt(json, "$.items[0].code");        // 10
```

---

## Supported JSONPath Features

- **Root selector** — `$`
- **Child selector** — `.name`
- **Index selector** — `[0]`, `[1]`, `[-1]` (negative index = from end)
- **Bracket notation for filters** — `[?()]`
- **Filter expressions** — `@.field`, `==`, `!=`, `>=`, `>`, `<=`, `<`, `&&`, `||`, single-quoted strings, numeric literals
- **isOneOf** — `isOneOf(@.field, 'a','b','c')` for multiple-value matching

---

## Path and Selector Examples

| Selector | Description |
|----------|-------------|
| `$.id` | Root property |
| `$.data.name` | Nested property |
| `$.data.myArray[0].myItem` | Array element by index |
| `$.items[-1].id` | Last array element (negative index) |
| `$.items[?(@.kind=='x')]` | Array elements matching filter |
| `$.items[?(@.a=='1' && @.b=='2')]` | Filter with AND |
| `$.items[?(@.kind=='x' && (@.code=='10' \|\| @.code=='30'))]` | Filter with OR |
| `$.items[?(@.kind=='x' && isOneOf(@.code, '10','30'))]` | Filter with isOneOf |
| `$.node1.node2 == 'match'` | Path value equals (non-array) |
| `$.node1.node2 != 'match'` | Path value not equals |
| `$.count >= 5` | Path value comparison (>=, >, <=, <) |

---

## Installation

### NuGet (GitHub Packages)

```bash
dotnet add package JsonSelector
```

Add the GitHub Packages source to `nuget.config` if needed:

```xml
<packageSources>
  <add key="github" value="https://nuget.pkg.github.com/YOUR_ORG/index.json" />
</packageSources>
```

### Project Reference

```xml
<ProjectReference Include="path\to\src\JsonSelector\JsonSelector.csproj" />
```

---

## Usage

### Dependency Injection

```csharp
using JsonSelector;

// Register with DI
services.AddJsonSelector();

// Inject and use
var selector = serviceProvider.GetRequiredService<IJsonSelector>();
```

### Example with Sample JSON

```csharp
string json = """
{
  "id": 1001,
  "name": "alpha",
  "items": [
    { "id": "A1", "kind": "x", "code": "10" },
    { "id": "B2", "kind": "y", "code": "20" },
    { "id": "C3", "kind": "x", "code": "30" }
  ]
}
""";

// Check if path exists
bool exists = selector.Any(json, "$.id");

// Check if path value matches (non-array paths)
bool nameMatches = selector.Any(json, "$.name == 'alpha'");
bool idInRange = selector.Any(json, "$.id >= 1000");

// Extract values
string? name = selector.FirstString(json, "$.name");
int? code = selector.FirstInt(json, "$.items[?(@.kind=='x')].code");

// Multiple-value filter (OR)
bool hasMatch = selector.Any(json, "$.items[?(@.kind=='x' && (@.code=='10' || @.code=='30'))]");

// Multiple-value filter (isOneOf)
string? id = selector.FirstString(json, "$.items[?(@.kind=='x' && isOneOf(@.code, '10','20'))].id");

// Array index (first, last, by position)
string? first = selector.FirstString(json, "$.items[0].id");
string? last = selector.FirstString(json, "$.items[-1].id");
```

---

## Requirements

- .NET 10.0

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## Repository

Source code: [https://github.com/yanis-kr/JsonSelector](https://github.com/yanis-kr/JsonSelector)
