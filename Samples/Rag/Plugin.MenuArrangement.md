# Menu Arrangement Plugin for SAL Windows Forms
[![Auto build](https://github.com/DKorablin/Plugin.MenuArrangement/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.MenuArrangement/releases/latest)

Provides UI and storage logic for arranging and persisting the order of menu items in SAL-based Windows host applications.

## Features
- Reorder menu items via drag & drop panel
- Persist custom order between application sessions
- Works with SAL Flatbed host Menu integration
- Targets .NET Framework 3.5 and .NET 8 (Windows)

## Installation
1. Download one of the compatible host applications that support SAL plugins:
	- [Flatbed Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed MDI](https://dkorablin.github.io/Flatbed-MDI/)
	- [Flatbed MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon/)
2. Extract to a folder of your choice.
3. Put the plugin to the `Plugins` subfolder of the host application.

## Usage
1. Open the Menu Arrangement panel from the host plugin panels list / configuration UI.
2. Drag items to the desired order.
3. Save / Apply changes. The order is stored via the plugin's custom storage (see `CustomMenuOrderStorage`).
4. Restart or reopen the application: the order is restored automatically.

## Persistence
Ordering data is saved using the plugin's storage abstraction. Review `CustomMenuOrderStorage.cs` for extension or customization.