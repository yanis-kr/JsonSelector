# Publishing NuGet Packages

The GitHub Action builds and publishes the JsonSelector NuGet package.

## Triggers

- **Push a version tag** (e.g. `v1.0.0`) — Builds, tests, packs, and publishes to GitHub Packages. Optionally publishes to NuGet.org if `NUGET_API_KEY` is configured.
- **Manual dispatch** — Run the workflow from the Actions tab. You can specify a version and choose whether to publish to NuGet.org.

## Feeds

### GitHub Packages (automatic)

Publishes to `https://nuget.pkg.github.com/<owner>/index.json` using `GITHUB_TOKEN`. No extra setup.

**Consuming in another project:**

1. Add a `nuget.config` in your solution/project directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/YOUR_ORG/index.json" />
  </packageSources>
</configuration>
```

2. Authenticate (required for private repos; public packages may work without):

```bash
dotnet nuget add source https://nuget.pkg.github.com/YOUR_ORG/index.json --name github --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_PAT --store-password-in-clear-text
```

Use a [Personal Access Token](https://github.com/settings/tokens) with `read:packages` scope.

3. Add the package reference:

```xml
<PackageReference Include="JsonSelector" Version="1.0.0" />
```

### NuGet.org (optional)

To publish to NuGet.org:

1. Create an API key at [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
2. Add it as a repository secret: **Settings → Secrets and variables → Actions → New repository secret** → Name: `NUGET_API_KEY`, Value: your API key
3. On tag push, the package will be published to NuGet.org. Or run the workflow manually and check "Publish to NuGet.org"

**Consuming:** Add the package reference; NuGet.org is the default source.
