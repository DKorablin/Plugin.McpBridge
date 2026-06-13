# Plugin.PEImageView
[![Auto build](https://github.com/DKorablin/Plugin.PEImageView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.PEImageView/releases/latest)

Portable Executable (PE) + CLI metadata inspection plugin. Originally created to answer a simple question across many internal projects: "Is a specific method actually used anywhere, or can it be removed?" Reflection-based approaches quickly hit limits (loading conflicts, shadow copying, missing unsupported metadata). The project therefore moved down to raw PE / CLI structures and selective resource parsing (e.g. TypeLib) to provide reliable static insight without full runtime loading.

## Goals
- Inspect low-level PE headers and sections.
- Read CLR (CLI) metadata structures without executing code.
- Enumerate and analyze embedded resources (e.g. COM TypeLib where available).
- Provide an extensible plugin surface for different loading / analysis providers.
- Assist in safe code cleanup decisions (unused members, assemblies relationships).

## Installation
To install the Portable Executable Viewer Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.PEImageView/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)

## Supported Targets
- .NET Framework 4.8 (full feature set, including runtime TypeLib import via TypeLibConverter).
- .NET 8 (Windows) (TypeLib import disabled; falls back to `PlatformNotSupported` where not applicable).

## Features (High-Level)
- PE image structure viewing (DOS header, NT headers, sections) (internal helpers).
- CLI metadata table access (types, methods, references) for static cross-assembly relation checks.
- Resource inspection; conditional COM TypeLib analysis (.NET Framework only) via ComTypeAnalyzer.
- Separation of providers to avoid unsafe reflection loading side effects.

## Why Not Only Reflection?
Reflection-based scanning can: 
- Trigger load failures due to missing dependencies.
- Execute unwanted static constructors.
- Miss certain metadata (forwarded types, unreferenced IL, removed/inaccessible members).
Working directly with PE + metadata avoids these issues and enables lightweight offline analysis.

## Limitations
- COM TypeLib import is not available on .NET 8 builds; calls throw PlatformNotSupportedException.
- Some advanced metadata (e.g. edit-and-continue deltas, profiler-specific streams) may not be parsed.
- This is a static analysis aid; dynamic behaviors (reflection emit, runtime code generation) are out of scope.