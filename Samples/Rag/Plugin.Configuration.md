# Plugin.Configuration
[![Auto build](https://github.com/DKorablin/Plugin.Configuration/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Configuration/releases/latest)

[![UI Screenshot](.github/assets/UI-1-200.png)](.github/assets/UI-1.png)

UI plugin that adds a rich, unified configuration and management experience for SAL Windows host applications. It provides a modal/tool window (PluginsDlg) that lets end‑users discover, inspect, configure, search, and unload runtime plugins exposed by the SAL framework.

## Key Features
* Tools > Plugins menu integration (added at runtime by PluginWindows.OnConnection)
* Plugin catalog listing (name + optional icon extracted from IKernelInfo.ApplicationIcon)
* Detailed metadata panel: name, author/company, version, source, description
* Live search across plugin metadata, property names, attribute values and comparable property values (Utils.GetPluginSearchMembers)
* Per‑plugin settings editing through a PropertyGrid when a plugin implements IPluginSettings
* Optional advanced settings tab – dynamically injected UserControl returned by reflection of PluginMessage.GetPluginOptionsControl
* Reset selected plugin settings (removes persisted assembly parameters)
* Unload non‑kernel plugins at runtime
* Persistence of window size, location and splitter distance (PluginSettings) between sessions
* Context menus for plugin list and settings property grid (reset property value, toggle description/help panel)
* Multi‑targeting: .NET Framework 3.5 and .NET 8 (single codebase for legacy and modern hosts)
* Structured tracing via TraceSource (all levels enabled) using host trace listeners
* NuGet packaging metadata (AlphaOmega.SAL.Plugin.Configuration) with MIT license

## Architecture Overview
| Component | Responsibility |
|-----------|----------------|
| PluginWindows | Implements IPlugin and IPluginSettings&lt;PluginSettings&gt;; injects menu, manages lifetime of PluginsDlg, trace source, settings load/save via host ISettingsProvider. |
| PluginsDlg | Windows Forms UI for browsing and configuring plugins. Handles dynamic tabs, search, selection logic, unload/reset actions, and persistence of UI state. |
| PluginSettings | Serializable settings container (size, location, splitter distance) with INotifyPropertyChanged. Persisted through SAL settings provider. |
| OptionsCtrl | Placeholder base control for the “Options” tab; replaced by plugin‑specific controls when available. |
| Utils | Reflection-based search helper building a sequence of search tokens from plugin description + settings object. |
| Resources | Localized prompt strings and the window icon bitmap. |

### Lifecycle
1. Host constructs PluginWindows(hostWindows).
2. OnConnection creates a Tools.Plugins menu item (IMenuItem) and wires Click.
3. User invokes the menu; PluginsDlg is created (lazy) and shown (non-modal tool window style).
4. Dialog loads plugin list from hostWindows.Plugins, applying search filter if active.
5. Selecting a plugin populates metadata, optional settings, and optional custom options control.
6. Closing the dialog (user close) disposes it; OnDisconnection cleans up menu + dialog.

### Settings Persistence
Settings are loaded once (lazy) by PluginWindows.Settings using HostWindows.Plugins.Settings(this).LoadAssemblyParameters(). On close, PluginsDlg writes current UI metrics back onto PluginSettings; SAL’s settings provider then persists individual property changes via SaveAssemblyParameter calls when edited/reset.

### Search Mechanism
Utils.GetPluginSearchMembers aggregates:
* Public instance property names of IPluginDescription
* Plugin settings property names
* Attribute property names/values for each settings property (deep reflection, generic)
* Comparable, readable property values (ToString) for indexing
This produces a broad token set enabling flexible, case‑insensitive substring matching.

### Extensibility Points
* Basic settings: any plugin implementing IPluginSettings exposes a settings object to the PropertyGrid.
* Advanced UI: plugins may implement a method addressed by PluginMessage.GetPluginOptionsControl returning a UserControl. Reflection discovers IPluginMethodInfo and invokes it; control is then hosted in the Options tab.
* Unload capability: plugins not implementing IPluginKernel can be removed at runtime via hostWindows.Plugins.UnloadPlugin.

### Multi-Targeting Strategy
The project uses TargetFrameworks net35;net8.0-windows to serve both legacy and modern host environments. Windows Forms API surface is chosen to remain compatible with .NET 3.5 while taking advantage of .NET 8 builds for future hosts. Conditional compilation is not currently required; shared code runs on both frameworks.

### Tracing
PluginWindows.CreateTraceSource&lt;T&gt;() creates a TraceSource named after the assembly + optional suffix. Default listener is removed and host/system listeners are attached, enabling unified logging. All SourceLevels are enabled for maximum diagnostics.

### Packaging
NuGet metadata (PackageId, Title, Description, Authors, License, Tags, RepositoryUrl, Readme) is defined in the .csproj. README is packed into docs\README.md. Set Version externally (e.g., via CI). Runtime assets for SAL.Windows are excluded (ExcludeAssets runtime) to defer host responsibility for platform binaries.

### UI Summary
Tabs:
* Options – placeholder or dynamically injected UserControl from plugin.
* Settings – PropertyGrid for basic settings (only visible when selected plugin supports IPluginSettings).

Context Menus:
* Plugin list: Reset settings, Unload plugin (respecting kernel constraints).
* PropertyGrid: Reset selected property, toggle description/help.

Search:
* Ctrl+F opens search bar; Enter executes; Escape closes.

Keyboard Shortcuts:
* Ctrl+C on plugin list copies plugin name.
* Ctrl+A inside metadata text boxes selects all.

### Localization
Prompt strings (reset confirmation, unload confirmation) and icon bitmap are loaded from Resources enabling future culture extensions by adding .resx variants.

### Integration / Usage
1. Place plugin into chosen SAL Host application (Flatbed.Dialog.Lite is included in the release as an example). You can find more host applications here: https://dkorablin.github.io/
2. Add different plugins that are supporting configuration.
3. Launch host; user sees Tools > Plugins.
4. End-user opens Plugins window to manage plugins.
5. Persisted UI state restores on next run.

### Building
Standard dotnet build or Visual Studio build produces both target frameworks. Release configuration can generate a single-file publish for net8.0-windows (PublishSingleFile true).

### Safety & Constraints
* Unloading kernel (core) plugins is blocked (IPluginKernel check).
* Reflection is limited to public instance properties and one known method name constant (PluginMessage.GetPluginOptionsControl).
* Search operates on metadata only; no code execution apart from safe property getters.

### Future Enhancements (Ideas)
* Persist per-plugin UI preferences (e.g., last selected tab).
* Add sorting and filtering options (e.g., by version, author).
* Provide export/import of plugin settings.
* Add theming / high DPI improvements.
* Async loading for large plugin sets.

## License
MIT

## Repository
https://github.com/DKorablin/Plugin.Configuration