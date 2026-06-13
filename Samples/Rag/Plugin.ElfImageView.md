# Plugin.ElfImageView
[![Auto build](https://github.com/DKorablin/Plugin.ElfImageView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.ElfImageView/releases/latest)

A [SAL (Software Application Layer)](https://github.com/DKorablin/SAL.Windows) plugin for viewing **ELF (Executable and Linkable Format)** binary files (`.so`). Built as part of a broader set of plugins for reading file formats commonly found inside APK archives — including Dalvik (`.dex`) and JavaClass (`.class`).

## Features

- Parse and display ELF binary structure in a tree-based table of contents panel
- Navigate ELF sections through a dedicated viewer with the following section types:
  - **Identification** — ELF identification header bytes
  - **Header** — ELF file header fields
  - **Sections** — full section header table
  - **String Tables** — `.strtab` / `.dynstr` string section contents
  - **Symbols** — symbol table entries
  - **Relocations** — `.rel` relocation entries
  - **Relocations (Addend)** — `.rela` relocation entries with explicit addends
  - **Notes** — `.note` section contents
  - **Debug Strings** — DWARF debug string section contents
- Load ELF files from disk or from memory (e.g. extracted from an APK)
- Optional file system monitoring for live file change detection
- Display integer values in decimal or hexadecimal format
- Configurable maximum array item display count
- Persistent session: previously loaded files are restored on startup

## Requirements

- Windows (WinForms-based UI)
- [SAL.Windows](https://github.com/DKorablin) host application
- [AlphaOmega.ElfReader](https://www.nuget.org/packages/AlphaOmega.ElfReader) NuGet package

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.ElfImageView should appear in the plugin list (View -> Executables -> ELF View).

## Target Frameworks

| Framework | Support |
|---|---|
| .NET Framework 4.8 | ✔ |
| .NET 8 (Windows) | ✔ |

## Settings

The plugin exposes the following configurable settings through the SAL host:

| Setting | Category | Default | Description |
|---|---|---|---|
| `ShowAsHexValue` | Appearance | `false` | Display integer values in hexadecimal format |
| `MaxArrayDisplay` | Appearance | `10` | Maximum number of array items to display |
| `MonitorFileChange` | Data | `false` | Monitor loaded files for changes on the file system |

## License

[MIT](https://opensource.org/licenses/MIT) © Danila Korablin