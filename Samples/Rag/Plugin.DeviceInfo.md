# Plugin.DeviceInfo

[![Auto build](https://github.com/DKorablin/Plugin.DeviceInfo/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.DeviceInfo/releases/latest)

A Windows plugin that provides comprehensive device information through a user-friendly interface, including S.M.A.R.T. disk data and SMBIOS system information using the Win32 DeviceIoControl API.

## Overview

This project is a SAL (Software Abstraction Layer) plugin that wraps the Windows DeviceIoControl function to provide detailed hardware information.
It displays disk drive information, S.M.A.R.T. (Self-Monitoring, Analysis, and Reporting Technology) attributes, and System Management BIOS data through intuitive WinForms interfaces.

### Origin Story

While analyzing old C++ projects, the original Network Administrator application code was discovered.
One of its goals was to transfer S.M.A.R.T. data from server machines to clients through WinSock.
The original wrapper was written by **Andrew I. Reshin** in 2001 in C++.
At that time, the API to receive SMART data was undocumented on MSDN and had to be called with:

```cpp
#define DFP_RECEIVE_DRIVE_DATA 0x0007c088
```

This project represents a modern .NET port of that original C++ code, implemented without unsafe code while maintaining full functionality.

## Features

### Device Information Panel
- **Real-time device detection** - Automatically refreshes when devices are added/removed
- **Drive geometry information** - Cylinders, heads, sectors, and capacity
- **Partition information** - GPT and MBR partition table details
- **Storage device details** - Device numbers and volume data
- **Power status monitoring** - Device power state tracking
- **Channel configuration** - Primary/Secondary, Master/Subordinate detection

### S.M.A.R.T. Data Analysis
- **Complete S.M.A.R.T. attributes** - All available disk health parameters
- **Attribute monitoring** - Current values, worst values, and thresholds
- **Raw value display** - Detailed raw data with formatting
- **Drive identification** - Serial number, model, firmware version
- **Capability detection** - Drive capabilities and supported features
- **LBA sector information** - Total addressable sectors

### SMBIOS Information
- **System firmware data** - Complete SMBIOS table browsing
- **Type filtering** - Filter by specific SMBIOS table types
- **Baseboard information** - Manufacturer, product, and version details
- **Structured view** - Organized display of all SMBIOS structures
- **Export/Import capability** - Save and load firmware dumps (.sfw files)

## Requirements

- **Operating System**: Windows (Windows 7 or later recommended)
- **Framework**: 
  - .NET Framework 3.5 or later
  - .NET 8.0 or later
- **Dependencies**:
  - SAL.Windows
  - AlphaOmega.DeviceIoControl
  - AlphaOmega.SystemFirmware

## Installation

## Installation
To install the DeviceInfo Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.DeviceInfo/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)

## Usage

### Accessing Device Information

1. Open the host SAL application
2. Navigate to **Tools → WinAPI → Device Info**
3. Select a drive from the dropdown
4. View detailed information including:
   - General device properties
   - Geometry and partition data
   - S.M.A.R.T. attributes and health status

### Viewing SMBIOS Data

1. Navigate to **Tools → WinAPI → SMBIOS**
2. Browse all SMBIOS tables and structures
3. Filter by type using the dropdown
4. Export firmware data using the Save button
5. Load previously saved firmware dumps using the Load button

### Working with S.M.A.R.T. Data

The S.M.A.R.T. panel displays critical disk health attributes:
- **ID**: Attribute identifier (hex)
- **Name**: Human-readable attribute name
- **Value**: Current normalized value (0-255)
- **Worst**: Worst value ever recorded
- **Threshold**: Failure threshold
- **Raw**: Raw data value with formatting

## Architecture

### Project Structure

```
Plugin.DeviceInfo/
├── Controls/               # Custom UI controls
│   ├── BiosTypeListView.cs
│   ├── ReflectionListView.cs
│   ├── DdlSmBiosType.cs
│   └── NameValueLabel.cs
├── Bll/                    # Business logic layer
│   ├── TypeExtender.cs
│   ├── NodeExtender.cs
│   └── XmlReflectionReader.cs
├── PluginWindows.cs        # Main plugin entry point
├── PanelDevice.cs          # Device info UI panel
└── PanelSmBios.cs          # SMBIOS info UI panel
```

### Key Components

- **PluginWindows**: Main plugin class implementing `IPlugin` interface
- **PanelDevice**: Device information and S.M.A.R.T. data display
- **PanelSmBios**: SMBIOS table viewer with filtering
- **ReflectionListView**: Generic list view for displaying object properties
- **BiosTypeListView**: Specialized view for BIOS type structures

## Multi-Targeting

The project supports both legacy and modern .NET platforms:
- **.NET Framework 3.5** - For compatibility with older Windows systems
- **.NET 8.0** - Modern .NET with latest performance improvements

## Dependencies

This plugin relies on the following libraries:
- **SAL.Windows** - Plugin host framework
- **AlphaOmega.DeviceIoControl** - Win32 DeviceIoControl wrapper
- **AlphaOmega.SystemFirmware** - SMBIOS parsing and manipulation

## Building

```bash
# Clone the repository
git clone https://github.com/DKorablin/Plugin.DeviceInfo.git

# Build the solution
dotnet build Plugin.DeviceInfo.sln -c Release
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## Credits

- **Original C++ Wrapper**: Andrew I. Reshin (2001)
- **.NET Port & Current Maintainer**: Danila Korablin (2013-2025)

## Related Projects

- [AlphaOmega.DeviceIoControl](https://github.com/DKorablin/DeviceIoControl) - Core DeviceIoControl wrapper
- [AlphaOmega.SystemFirmware](https://github.com/DKorablin/SystemFirmware) - SMBIOS parsing library

## Acknowledgments

Special thanks to Andrew I. Reshin for the original C++ implementation (2001) that made this project possible.