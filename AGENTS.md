# AGENTS.md

## Cursor Cloud specific instructions

### Overview

JsonSelector is a .NET 8 library for querying JSON via JSONPath (RFC 9535 subset). It has one library project and one xUnit test project. No databases, Docker, or external services are required.

### .NET SDK

.NET 8 SDK is installed at `$HOME/.dotnet`. The `PATH` and `DOTNET_ROOT` are configured in `~/.bashrc`.

### Solution file caveat

The repo uses `.slnx` (XML-based solution format) which requires .NET 9+. Since the project targets `net8.0`, build and test the `.csproj` files directly:

```
dotnet build src/JsonSelector/JsonSelector.csproj
dotnet build tests/JsonSelector.Tests/JsonSelector.Tests.csproj
dotnet test tests/JsonSelector.Tests/JsonSelector.Tests.csproj
```

### Key commands

| Action | Command |
|--------|---------|
| Restore | `dotnet restore src/JsonSelector/JsonSelector.csproj && dotnet restore tests/JsonSelector.Tests/JsonSelector.Tests.csproj` |
| Build | `dotnet build tests/JsonSelector.Tests/JsonSelector.Tests.csproj` (builds library transitively) |
| Test | `dotnet test tests/JsonSelector.Tests/JsonSelector.Tests.csproj` |
| Pack | `dotnet pack src/JsonSelector/JsonSelector.csproj -c Release` |

### API usage note

`JsonSelectorImpl` is `internal`. External consumers must use DI via `services.AddJsonSelector()` and resolve `IJsonSelector`. The test project has `InternalsVisibleTo` access.
