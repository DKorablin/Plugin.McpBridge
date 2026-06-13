# Plugin.Members

[![Auto build](https://github.com/DKorablin/Plugin.Members/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Members/releases/latest)

A [Plugin.Members](https://github.com/DKorablin/Plugin.Members) plugin that reflects and displays all public members — methods, properties, events, and fields — of every loaded plugin at runtime.

## Features

- Lists all loaded plugins in a dedicated **View → Members** dialog window.
- For each plugin, displays its public **methods**, **properties**, **events**, and **fields** with their types and current values.
- Opens a document panel for any method that shows its **input parameters** in an editable tree and **output / return values** after invocation.
- Reads compiler-generated **XML documentation files** and shows summaries, remarks, and return descriptions inline.
- Supports **Ctrl+C** to copy any value from the parameter grids to the clipboard.

## Requirements

- Host application must expose the `IHostWindows` interface — the plugin does not load in console or headless hosts.
- Windows operating system (Windows Forms UI).

## Installation

1. Download one of the compatible host applications that support SAL plugins:
    - [Flatbed Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
    - [Flatbed Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite/)
    - [Flatbed MDI](https://dkorablin.github.io/Flatbed-MDI/)
    - [Flatbed MDI (Avalon)](https://dkorablin.github.io/Flatbed-MDI-Avalon/)
    - [Flatbed MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI/)
    - [Flatbed Worker Service](https://dkorablin.github.io/Flatbed-WorkerService/)
2. Extract to a folder of your choice.
3. Put the plugin to the `Plugins` subfolder of the host application.
4. Modify application.settings file and add `SAL_Path` variable with the path to the plugins folder or start the host application with the `/SAL_Path` command line argument to specify the plugin directory if needed.

## Usage

Once the plugin is loaded by the host, a **Members** entry appears under the **View** menu. Clicking it opens the plugin browser dialog:

1. Select a plugin from the left-hand list to inspect its public members.
2. Double-click a **method** row to open its document panel.
3. Fill in any input parameters in the **Input** grid and invoke the method.
4. The **Output** grid shows the return value and any `out`/`ref` parameters.

## Target Frameworks

| Framework | Notes |
|-----------|-------|
| .NET Framework 4.8 | Legacy host support |
| .NET 8 (Windows) | Recommended |

## Dependencies

- [SAL.Windows](https://www.nuget.org/packages/SAL.Windows) — SAL host and plugin interfaces
- [Microsoft.VisualStudio.VirtualTreeGrid](https://www.nuget.org/packages/Microsoft.VisualStudio.VirtualTreeGrid) — high-performance tree/grid control