# WMI Client Plugin

[![Auto build](https://github.com/DKorablin/Plugin.WmiClient/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.WmiClient/releases/latest)
[![License](https://img.shields.io/badge/license-Apache--1.0-blue.svg)](LICENSE)

A powerful Windows Management Instrumentation (WMI) testing and exploration tool for the SAL plugin framework. This plugin provides a comprehensive interface for browsing WMI namespaces, executing WQL queries, invoking WMI methods, monitoring WMI events, and exploring WMI class descriptions.

## Features

### 🔍 **WQL Query Explorer**
- Execute custom WQL (WMI Query Language) queries against any WMI namespace
- Browse and filter WMI classes
- Interactive parameter grid for building queries
- Text-based query editor for advanced queries
- Real-time result display with formatted property values
- Export queries as PowerShell scripts or C# batch files

### ⚙️ **WMI Method Invocation**
- Discover and invoke WMI methods on classes and instances
- Automatic detection of static and instance methods
- Interactive parameter input with type validation
- Display method return values and output parameters
- Support for both intrinsic and extrinsic methods
- Property getter/setter support

### 📡 **WMI Event Monitoring**
- Subscribe to WMI events in real-time
- Support for both intrinsic and extrinsic events
- Multiple simultaneous event subscriptions
- Start/stop individual event watchers
- Event filtering with customizable conditions
- Pooling interval configuration for extrinsic events

### 📖 **WMI Class Description**
- Browse WMI namespaces and classes
- View detailed class descriptions and MOF (Managed Object Format) definitions
- Explore class qualifiers, properties, and methods
- Navigate WMI provider hierarchy

### 🛠️ **Additional Features**
- **Multi-target Support**: Runs on both .NET Framework 3.5 and .NET 8.0
- **Remote WMI Access**: Connect to remote machines with custom credentials
- **Code Generation**: Export WMI operations as PowerShell or C# scripts
- **Execution Timeout**: Configurable timeout for WMI operations
- **Hex Value Display**: Toggle between decimal and hexadecimal display
- **Array Formatting**: Customizable array display with element limit
- **Namespace Navigation**: Quick dropdown navigation for namespaces and classes

## Installation

1. Download the release archive (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.WmiClient should appear in the plugin list (Tools -> WinAPI -> WMI -> [WQL Query, WMI Method, WMI Event, Description]).

## Requirements

- **Operating System**: Windows (WMI is a Windows-specific technology)
- **Framework**: 
  - .NET Framework 3.5 or higher, OR
  - .NET 8.0 or higher
- **Host Application**: SAL (Software Analysis Library) framework with Windows Forms support

## Usage

### Basic WQL Query
1. Navigate to **Tools** → **WinAPI** → **WMI** → **WQL Query**
2. Select a namespace (e.g., `root\CIMv2`)
3. Select a WMI class (e.g., `Win32_Process`)
4. Click **Run** to execute the query
5. View results in the interactive list view

### Invoking WMI Methods
1. Navigate to **Tools** → **WinAPI** → **WMI** → **WMI Method**
2. Select a namespace and class with methods
3. Select an instance (for instance methods) or skip for static methods
4. Choose a method from the dropdown
5. Fill in required parameters
6. Click **Run** to invoke the method

### Monitoring WMI Events
1. Navigate to **Tools** → **WinAPI** → **WMI** → **WMI Event**
2. Select an event class (e.g., `__InstanceCreationEvent`)
3. Configure event filters in the parameter grid
4. Click **Run** to start monitoring
5. View incoming events in real-time

### Exporting to Scripts
1. After configuring a query, method, or event subscription
2. Click the **Export Code** button (💾)
3. Choose PowerShell Script (`.ps1`) or C# Batch (`.bat`)
4. Save and run the generated script independently

## Configuration

The plugin supports the following settings:

| Setting | Description | Default |
|---------|-------------|---------|
| **MachineName** | Target computer name or IP address | `localhost` |
| **ExecutionTimeout** | Timeout for WMI operations (seconds) | `30` |
| **PoolingInterval** | Polling interval for extrinsic events (seconds) | `5` |
| **ShowAsHexValue** | Display numeric values in hexadecimal format | `false` |
| **MaxArrayDisplay** | Maximum array elements to display | `10` |
| **Username** | Username for remote connections | _(empty)_ |
| **Password** | Password for remote connections | _(empty)_ |

Settings can be configured through the plugin's settings panel in the SAL host application.

## Architecture

### Core Components

- **PluginWindows**: Main plugin entry point and host integration
- **WmiData**: Core WMI data access layer for queries
- **WmiDataClass**: WMI class operations (methods, properties)
- **WmiDataEvent**: Event subscription and monitoring
- **WmiObserver**: Asynchronous WMI object enumeration

### UI Panels

- **PanelWqlQuery**: WQL query execution interface
- **PanelWmiMethod**: Method invocation interface  
- **PanelWmiEvent**: Event monitoring interface
- **PanelWmiDescription**: Class description browser
- **PanelWmiBase**: Base class for common functionality

## Target Frameworks

This plugin supports multi-targeting:

- **.NET Framework 4.8** (`net48`) - Legacy support
- **.NET 8.0** (`net8.0-windows`) - Modern .NET support

The project automatically selects the appropriate System.Management reference based on the target framework.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the Apache License 1.0 - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built on the [SAL (Software Analysis Library)](https://github.com/DKorablin/SAL.Windows) framework
- Uses [FastColoredTextBox (FCTB)](https://github.com/PavelTorgashov/FastColoredTextBox) for code editing