# AGENTS.md

## Cursor Cloud specific instructions

### Overview

JsonSelector is a .NET 10 library for querying JSON via JSONPath (RFC 9535 subset). It has one library project and one xUnit test project. No databases, Docker, or external services are required.

### .NET SDK

.NET 10 SDK is installed at `$HOME/.dotnet`. The `PATH` and `DOTNET_ROOT` are configured in `~/.bashrc`.

### Key commands

| Action | Command |
|--------|---------|
| Restore | `dotnet restore` |
| Build | `dotnet build` |
| Test | `dotnet test` |
| Pack | `dotnet pack src/JsonSelector/JsonSelector.csproj -c Release` |

The repo uses `.slnx` solution format (supported by .NET 9+). All standard `dotnet` commands work at the repo root.

### API usage note

`JsonSelectorImpl` is `internal`. External consumers must use DI via `services.AddJsonSelector()` and resolve `IJsonSelector`. The test project has `InternalsVisibleTo` access.
