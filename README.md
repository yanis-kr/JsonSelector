# JsonSelector

A lightweight .NET library for querying JSON payloads using [JSONPath](https://www.rfc-editor.org/rfc/rfc9535.html) expressions. Implements a subset of [RFC 9535 (JSONPath: Query Expressions for JSON)](https://www.rfc-editor.org/rfc/rfc9535.html). No external JSONPath dependencies—built on `System.Text.Json` only.

## Features

- **Any** — Check if a selector matches at least one node
- **FirstString** — Extract the first matching value as string (numbers use invariant culture)
- **FirstInt** — Extract the first matching value as int (supports numeric and string values)

## Supported JSONPath Features

- Root selector `$`
- Child selector `.name`
- Index selector `[0]`, `[1]`, `[-1]` (negative index = from end)
- Bracket notation for filters `[?()]`
- Filter expressions: `@.field`, `==`, `!=`, `&&`, `||`, single-quoted strings
- `isOneOf(@.field, 'a','b','c')` for multiple-value matching

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

## Installation

**NuGet (GitHub Packages):**

```bash
dotnet add package JsonSelector
```

Add the GitHub Packages source to `nuget.config` if needed:

```xml
<packageSources>
  <add key="github" value="https://nuget.pkg.github.com/YOUR_ORG/index.json" />
</packageSources>
```

**Project reference:**

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
bool exists = selector.Any(json, "$.id");

// Extract values
string? name = selector.FirstString(json, "$.name");
int? code = selector.FirstInt(json, "$.items[?(@.kind=='x')].code");

// Multiple-value filter (OR)
bool hasMatch = selector.Any(json, "$.items[?(@.kind=='x' && (@.code=='10' || @.code=='30'))]");

// Multiple-value filter (isOneOf)
string? id = selector.FirstString(json, "$.items[?(@.kind=='x' && isOneOf(@.code, '10','20'))].id");

// Array index (first, last, by position)
string? first = selector.FirstString(json, "$.data.myArray[0].myItem");
string? last = selector.FirstString(json, "$.data.myArray[-1].myItem");
```

## Requirements

- .NET 8.0
