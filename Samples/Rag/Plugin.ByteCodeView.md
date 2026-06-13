# Plugin.ByteCodeView
[![Auto build](https://github.com/DKorablin/Plugin.ByteCodeView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.ByteCodeView/releases/latest)

A **[SAL (Software Abstraction Layer)](https://github.com/DKorablin/SAL.Windows)** plugin for viewing Java bytecode and related binary formats. Originally developed as part of a project to inspect the contents of APK files, this plugin provides structured browsing of commonly used file formats found inside APK archives.

## Supported Formats

| Format | Extension | Description |
|---|---|---|
| JavaClass | `.class` | Java bytecode files compiled for the JVM |
| Dalvik | `.dex` | Dalvik Executable — Android's bytecode format |
| ELF | `.so` | Shared native libraries included in APK packages |

## Features

- **Tree-based Table of Contents** — a dockable panel (`JVM Class View`) provides a hierarchical view of all open class files and their sections
- **Structured section viewers** — browse individual sections of a `.class` file:
  - Class File header
  - Constant Pool
  - Fields & Methods
  - Interfaces
  - Attributes & Attribute Pool
  - Raw Tables
- **Reflection-based search** — search through class file structure using SAL's built-in search infrastructure
- **File system monitoring** — optionally watch open files for changes and reload automatically
- **Configurable display** — control how data is rendered via plugin settings

## Plugin Settings

| Setting | Default | Description |
|---|---|---|
| `ShowAsHexValue` | `false` | Display integer values in hexadecimal format |
| `ShowBaseMetaTables` | `false` | Show low-level base structure data |
| `MaxArrayDisplay` | `10` | Maximum number of array items to display inline |
| `MonitorFileChange` | `false` | Watch open files for changes on the file system and reload |

## Requirements

- Windows (WinForms host)
- [SAL.Windows](https://github.com/DKorablin/SAL.Windows) host application
- .NET Framework 4.8 **or** .NET 8 (Windows)

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.McpBridge should appear in the plugin list (View -> Executables -> ByteCode View).

## Usage

Once the plugin is loaded by the SAL host, navigate to **View → Executables → ByteCode View** to open the `JVM Class View` panel. From there you can open any `.class`, `.dex`, or `.so` file and browse its internal structure.

## License

[MIT](https://opensource.org/licenses/MIT)