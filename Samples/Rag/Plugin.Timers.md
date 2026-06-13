# Timers storage plugin
[![Auto build](https://github.com/DKorablin/Plugin.Timers/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Timers/releases/latest)

## Overview
Timers storage for service infrastructure.
This is a core component for each service infrastructure where you need to invoke any process after some time.
It provides a unified interface for managing various types of timers, ensuring reliable execution of background tasks and periodic operations.

## Supported Frameworks
- .NET Framework 4.8
- .NET 8.0 (Windows)

## Installation
To install the Timers Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.Timers/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.WorkerService](https://dkorablin.github.io/Flatbed-WorkerService)

## Architecture
The plugin is built upon the `SAL.Flatbed` ecosystem and follows a modular architecture:

- **Plugin Entry Point**: The `Plugin` class implements `IPlugin` and `IPluginSettings`, serving as the main interface for host applications.
- **Timer Management**: A `TimerFactory` creates and orchestrates timer instances, abstracting the underlying timer implementations.
- **Timer Types**: Supports multiple timer mechanisms to suit different threading models:
  - `TimersTimer`: Wraps `System.Timers.Timer`.
  - `ThreadingTimer`: Wraps `System.Threading.Timer`.
  - `WindowsTimer`: Wraps `System.Windows.Forms.Timer` (UI thread safe).
- **Settings & Configuration**: Robust settings management allow configuring timer intervals, work hours, and types without recompilation. Can be serialized via `Newtonsoft.Json`.
- **UI Integration**: Includes `ConfigCtrl` and custom editors (`TimeSpanEditor`) for easy configuration within Windows Forms environments.