# Loaded Assemblies Plugin
[![Auto build](https://github.com/DKorablin/Plugin.LoadedAssemblies/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.LoadedAssemblies/releases/latest)

Plugin to inspect all assemblies currently loaded into the host process (in load order). Helps diagnose:
* Which assembly triggered (referenced) another
* Version / location conflicts (GAC vs local, shadow copy, etc.)

## Key Features
* Live list of loaded assemblies with ordering (earliest -> latest)
* Displays name, version, culture, public key token, processor architecture
* Physical path + flags (domain, dynamic, reflection-only if applicable to target)
* On demand refresh
* Context menu actions (copy rows / values)
* Multi-target build: .NET Framework 4.8 + .NET 8 (Windows)
* Graceful fallbacks for APIs not available in .NET 8 (AppDomains enumeration differences, dynamic resource assembly creation, etc.)

## Installation
1. Download archive from GitHub releases (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.LoadedAssemblies should appear in the plugin list (Tools -> Executables -> [Loaded Assemblies & CLR Events]).

## Usage
1. Open the plugin panel (Loaded Assemblies).
2. Sort or filter (if supported by host ListView wrapper) to locate assemblies of interest.
3. Right-click for context menu (copy selected rows / values for reporting).
4. Use refresh to update after dynamic loads (Reflection.Emit, Assembly.Load, etc.).

## Typical Scenarios
* Diagnose assembly binding/version mismatch
* Confirm whether satellite resource assemblies were loaded
* Detect unexpected transitive dependencies
* Inspect dynamically emitted assemblies

## Build
Requires .NET SDK 8 and legacy MSBuild (for net48). Steps:
```
dotnet restore
dotnet build -c Release
```
Artifacts will include both target frameworks.

## Limitations
* Full AppDomain enumeration not available on .NET 8 (single AppDomain model)
* Some legacy COM interop types are excluded for NET8 build.

## Related Tools / Ideas
* fuslogvw.exe for binding logs (complements this plugin)
* AssemblyLoadContext tracing on .NET Core / .NET 8

## Support
Use GitHub Issues for bugs / feature requests.