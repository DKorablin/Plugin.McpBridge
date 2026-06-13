# Plugin.DependencyAnalyzer
[![Auto build](https://github.com/DKorablin/Plugin.DependencyAnalyzer/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.DependencyAnalyzer/releases/latest)

Inspect, explore, and rationalize the real dependency graph of a Windows / .NET application (managed + native). Spot what is loaded, what is missing, what is unused, and why.

## Key Features
- Full assembly + native DLL inventory (direct + transitive)
- Version / path / architecture mismatch detection
- Unused public surface (API trimming hints)
- Interactive graph (expand / collapse / drill‑down)
- Categorization: Framework / GAC / Third‑party / Solution
- Call member provenance: who references what
- Search & filter (by name, category, version, path)
- Export (graph + tabular data) for audits / reviews
- Runs in inspection mode only (no code changes needed)

## Why
Common deployment & upgrade pain points usually reduce to: “What is really required, and where did it come from?” This plugin shortens that loop: quicker binding failure diagnosis, leaner packaging, safer refactors, confident framework or runtime upgrades.

## Installation
To install the Dependency Analyzer Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.DependencyAnalyzer/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)

## Quick Start
1. Select file to analyze.
2. Switch between List and Graph views.
3. Use search / filters to narrow.
4. (Optional) Export results for documentation or review.

## Typical Scenarios
| Scenario | Value |
|----------|-------|
| Shrink deployment | Identify unused public APIs & orphan assemblies |
| Resolve FileNotFound / DllNotFound | See missing & expected load location |
| Plan refactor | Visualize coupling clusters |
| Audit third‑party usage | Enumerate external vs internal binaries |
| Migrate to newer .NET | Catch binding & version drift early |

## Exports
- Dependency list (CSV / JSON)
- Graph data (JSON / DGML style if supported)
- Unused member report

## Configuration Knobs
| Setting | Purpose |
|---------|---------|
| Recursion depth | Limit traversal for faster, focused scans |
| Include native | Toggle Win32 DLL resolution |
| Show unused members | Enable deeper member analysis (slower) |
| Grouping mode | Categorize by origin (Framework / GAC / Third‑Party / Solution) |

## Performance Tips
- Start with shallow depth to isolate immediate problems.
- Disable unused member analysis for quick structural checks.
- Point directly at your build output to avoid scanning irrelevant folders.

## Roadmap (draft)
- Optional NuGet package reference resolution

## Limitations
- Dynamic Assembly.Load / reflection‑only patterns may not be fully inferred.
- Native dependency resolution depends on current machine PATH / probing rules.
- Unused member analysis is static; runtime reflection / serialization use not guaranteed to be detected.

## Contributing
Issues and pull requests welcome. Please include reproduction steps for bugs and keep changes focused.



## Attribution
Inspired by common deployment troubleshooting workflows; implemented as a focused inspection aid.

---
Less guesswork. Leaner binaries. Faster diagnostics. Clear insight.