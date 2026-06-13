# Plugin.WebHelper

[![Auto build](https://github.com/DKorablin/Plugin.WebHelper/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.WebHelper/releases/latest)

A comprehensive suite of web development utilities based on SAL (Software Abstraction Layer) platform. This plugin provides various tools designed to help web administrators and programmers with common web-related tasks.

## Features

### 🔐 Hash Generator
Generate cryptographic hashes for passwords using various algorithms compatible with ASP.NET configuration:
- **MD5** hashing
- **SHA1** hashing
- **SHA256** hashing
- Batch password processing
- Compatible with `FormsAuthentication.HashPasswordForStoringInConfigFile`

### 📄 ViewState Decoder
Decode and analyze ASP.NET ViewState data with advanced features:
- Decode ViewState from Base64 strings
- Extract ViewState directly from URLs
- Visualize ViewState structure in tree format
- Export ViewState to XML
- Parse both ViewState and ControlState
- Handle ViewState from external assemblies gracefully
- Original decoder author: **Fritz Onion**

### 🎫 ASP.NET Ticket Manager
Encrypt and decrypt ASP.NET Forms Authentication tickets:
- Decode authentication tickets
- Create custom authentication tickets
- View ticket properties (expiration, issue date, user data, etc.)
- Compatible with `FormsAuthentication.Encrypt/Decrypt`

### 🖼️ Image to Base64 Converter
Convert images to Base64 encoded strings for embedding in web pages:
- Drag & drop support for multiple files
- Paste images directly from clipboard
- Batch conversion
- Image resizing options
- Custom output format templates
- Support for various image formats (JPEG, PNG, GIF, BMP, etc.)
- Direct copy to clipboard
- Large icon and detail view modes

### 🌐 IP Calculator
Network calculation utilities for IP address management:
- Calculate network masks
- Determine network and broadcast addresses
- CIDR notation converter
- Subnet mask to bit length conversion
- IPv4 address calculations

### 🔧 Web Request Tool
Make and analyze HTTP requests:
- Custom HTTP requests
- View response details
- Header manipulation

## Installation

To install the WebHelper Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.WebHelper/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)

## Usage

After installing the plugin, access the tools through the SAL main menu:

**Tools → Network → [Tool Name]**

Available menu items:
- **Hash** - Password hash generator
- **ViewState Decoder** - ASP.NET ViewState analyzer
- **Image to Base64** - Image to Base64 converter
- **IP Calculator** - Network calculator

### Quick Start Examples

#### Decoding ViewState
1. Open **Tools → Network → ViewState Decoder**
2. Paste a ViewState string or enter a URL
3. Click "Extract" to download ViewState from URL
4. Click "Decode" to visualize the structure

#### Generating Password Hash
1. Open **Tools → Network → Hash**
2. Enter a password
3. Select hash format (MD5, SHA1, SHA256)
4. Click "Encrypt"
5. Copy the generated hash for use in web.config

#### Converting Images to Base64
1. Open **Tools → Network → Image to Base64**
2. Drag and drop images or click "Browse"
3. Configure size and format options
4. Select items and copy to clipboard or save to file

## Technical Details

### Target Frameworks
- **.NET Framework 3.5** - For legacy SAL platforms
- **.NET 8.0** - Modern cross-platform support

### Architecture
- **Plugin Type**: Windows Forms UserControl-based plugin
- **Host**: SAL.Windows (Swiss Army Launcher)
- **UI Framework**: Windows Forms
- **Docking**: Supports various dock states (Document, DockRight, DockBottom, Float)

### Compatibility
- Uses System.Web components on .NET Framework 3.5
- Includes compatibility layer for .NET 8.0 to support ASP.NET-specific functionality
- Multi-targeted build ensures compatibility across different .NET versions

### Project Structure
```
Plugin.WebHelper/
├── Bll/                    # Business Logic Layer
├── Compat/                 # .NET 8 compatibility layer
│   ├── FormsAuthenticationCompat.cs
│   ├── LosFormatterCompat.cs
│   └── SystemWebUICompat.cs
├── Properties/             # Assembly and resource files
├── Reflection/            # Reflection utilities
├── UI/                    # Custom UI components
├── DocumentViewState.cs   # ViewState decoder
├── DocumentAspTicket.cs   # ASP.NET ticket manager
├── PanelHash.cs           # Hash generator
├── PanelImage2Base64.cs   # Image converter
├── PanelIpCalculator.cs   # IP calculator
└── PluginWindows.cs       # Main plugin entry point
```

### Dependencies
- **SAL.Windows** - Core SAL framework for plugin infrastructure
- **System.Web** (.NET Framework 3.5 only) - ASP.NET utilities

### Settings Persistence
The plugin automatically saves user preferences:
- Last used ViewState URL
- Password hash format preference
- Image conversion settings (size, format, clipboard option)
- View mode preferences

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET Framework 3.5 Developer Pack
- .NET 8.0 SDK

### Build Instructions
```powershell
# Restore NuGet packages
msbuild Plugin.WebHelper.sln /t:Restore

# Build for all target frameworks
msbuild Plugin.WebHelper.sln /p:Configuration=Release

# Or use dotnet CLI
dotnet build Plugin.WebHelper.sln -c Release
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Credits

- **ViewState Decoder**: Original implementation by [Fritz Onion](https://docs.microsoft.com/en-us/archive/msdn-magazine/2003/march/asp-net-internals-httpmodules-and-httphandlers)
- **Plugin Development**: Danila Korablin

## Version History

See [Releases](https://github.com/DKorablin/Plugin.WebHelper/releases) for detailed version history and changelogs.

---

**Note**: This plugin is part of the SAL (Software Abstraction Layer) ecosystem and requires the SAL.Windows host application to run.