# NuGet Package Configuration

## CodeSoupCafe.Maui Package

The `CodeSoupCafe.Maui` package contains reusable MAUI components extracted from LunaDraw, including the ItemGalleryView carousel component.

### Package Information

- **Package Name**: CodeSoupCafe.Maui
- **Current Version**: 1.0.0
- **NuGet URL**: https://www.nuget.org/packages/CodeSoupCafe.Maui/

### Package Sources Configuration

The project uses a custom `nuget.config` file that configures two package sources:

1. **nuget.org** (Primary): The official NuGet package registry
   - Used in all environments (local development, CI/CD, production builds)
   - All packages are available from this source

2. **LocalNuGetPackages** (Optional): Local development source
   - Path: `../LocalNuGetPackages`
   - Used for local testing before publishing to NuGet.org
   - Automatically ignored if the path doesn't exist (e.g., in GitHub Actions)

### Package Source Mapping

The `nuget.config` includes package source mapping to control which packages come from which sources:

- All packages (`*`) can be restored from nuget.org
- CodeSoupCafe packages (`CodeSoupCafe.*`) can also be restored from LocalNuGetPackages

This configuration ensures:
- External builds (GitHub Actions, Azure DevOps, etc.) will always pull from nuget.org
- Local developers can test unreleased versions from the LocalNuGetPackages folder
- No conflicts or errors when the local folder doesn't exist

### Publishing Updates to NuGet

To publish a new version of CodeSoupCafe.Maui:

1. **Update the package version** in the library's .csproj file
2. **Build the package**: `dotnet pack CodeSoupCafe.Maui.csproj -c Release`
3. **Test locally** by copying the .nupkg to `../LocalNuGetPackages`
4. **Upload to NuGet** using the provided scripts:
   ```bash
   # Windows Batch
   upload-to-nuget.bat YOUR_API_KEY

   # PowerShell (cross-platform)
   .\upload-to-nuget.ps1 -ApiKey YOUR_API_KEY
   ```
5. **Update the version** in LunaDraw.csproj's PackageReference
6. **Test the restore** from NuGet.org:
   ```bash
   dotnet restore LunaDraw.csproj --configfile nuget.config --no-cache
   ```

### GitHub Actions Integration

The GitHub Actions workflow (`.github/workflows/dotnet-desktop.yml`) is configured to:

1. **Cache NuGet packages** for faster builds
2. **Explicitly use nuget.config** during restore: `dotnet restore --configfile nuget.config`
3. **Pull packages from nuget.org** automatically (LocalNuGetPackages is ignored)

No additional configuration or secrets are needed in GitHub Actions for package restore.

### Troubleshooting

#### Package not found in external builds

If the package isn't found during CI/CD builds:

1. Verify the package is published to nuget.org: https://www.nuget.org/packages/CodeSoupCafe.Maui/
2. Check the version in LunaDraw.csproj matches the published version
3. Ensure nuget.config is committed to the repository
4. Try clearing the NuGet cache: `dotnet nuget locals all --clear`

#### Using a pre-release version locally

To test a pre-release version before publishing:

1. Build the package: `dotnet pack -c Release`
2. Copy the .nupkg to `C:\Projects\LocalNuGetPackages\`
3. Update the version in LunaDraw.csproj to match
4. Restore: `dotnet restore --no-cache`

The local source will take precedence for CodeSoupCafe packages.

#### Verifying package source

To see which source a package was restored from:

```bash
dotnet restore LunaDraw.csproj --configfile nuget.config --verbosity detailed
```

Look for lines like:
```
  GET https://api.nuget.org/v3/...
  Installed CodeSoupCafe.Maui 1.0.0 from https://api.nuget.org/v3/index.json
```

### Best Practices

1. **Always test locally** before publishing to nuget.org
2. **Use semantic versioning** for package versions (MAJOR.MINOR.PATCH)
3. **Update LunaDraw immediately** after publishing a new package version
4. **Don't commit .nupkg files** to the repository (they're in .gitignore)
5. **Keep nuget.config** in source control for consistent builds across environments
