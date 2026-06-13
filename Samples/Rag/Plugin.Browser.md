# Browser plugin
[![Auto build](https://github.com/DKorablin/Plugin.Browser/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Browser/releases/latest)

A [Software Application Layer](https://github.com/DKorablin/SAL.Windows) plugin that embeds a Microsoft Edge (WebView2) browser directly inside the host Windows Forms application, providing URL navigation and interactive DOM node inspection capabilities.

## Features

- **Embedded browser** — hosts a fully functional Microsoft Edge (WebView2) browser window docked inside the SAL host application
- **Simple browser view** (`DocumentBrowser`) — navigate to any URL with a persistent last-visited URL setting
- **Wizard browser view** (`DocumentBrowserWizard`) — advanced browser panel with interactive DOM node selection and XPath-based node mapping
- **DOM node interception** — hold `Ctrl` and hover or click any element in the wizard view to capture its XPath automatically
- **XPath HTML traversal** — built-in `XPathHtml` utility for programmatic DOM querying using an XPath-like syntax over `HtmlElement` trees
- **Host menu integration** — adds a **View → Browser** menu item to the host application automatically on connection

## Requirements

- Windows OS (Windows Forms application)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- Host application built on the [SAL.Windows](https://github.com/DKorablin/SAL.Windows) framework

## Target Frameworks

| Framework | Support |
|-----------|---------|
| .NET Framework 4.8 | ✔ |
| .NET 8 (Windows) | ✔ |

## Components

### `DocumentBrowser`
A dockable browser window that wraps a `WebView2` control. Navigates to the last-used URL on startup. Settings are persisted via `DocumentBrowserSettings`.

| Setting | Description |
|---------|-------------|
| `NavigateUrl` | The URL last navigated to; restored on next open |

### `DocumentBrowserWizard`
An advanced browser window designed for scraping and DOM automation workflows. On page load, a JavaScript interception script is injected that tracks `Ctrl`+hover and `Ctrl`+click events and reports the XPath of the targeted element back to the host.

| Setting | Description |
|---------|-------------|
| `NavigateUrl` | The URL to load for node parsing |
| `Nodes` | Dictionary of named XPath expressions used to extract data nodes |
| `CallerPluginId` | ID of the plugin that owns the wizard session |

### `XPathHtml`
A utility class (`IEnumerable<HtmlElement>`) that traverses an `HtmlElement` DOM tree using a simplified XPath-like path string.

```csharp
IEnumerable<HtmlElement> nodes = plugin.FindNodes(bodyElement, "TABLE/TBODY/TR[2]/TD[@class=value]");
```

Supported path segment formats:

| Syntax | Description |
|--------|-------------|
| `TAG` | Match all child elements with the given tag name |
| `TAG[N]` | Match the Nth element (1-based) with that tag |
| `TAG[@attr=value]` | Match elements where `attr` equals `value` |
| `id("someId")` | Match element by its `id` attribute directly |

## Usage

The plugin is loaded automatically by the SAL host. Once connected, a **Browser** entry appears under the **View** menu. Clicking it opens a `DocumentBrowser` window.

To open a `DocumentBrowserWizard` programmatically from another plugin:

```csharp
IWindow window = hostWindows.Windows.CreateWindow(plugin, typeof(DocumentBrowserWizard).ToString(), false, DockState.Document, args);
```

To query DOM nodes from code using the XPath utility:

```csharp
foreach (HtmlElement element in plugin.FindNodes(body, "DIV/UL/LI[1]"))
    Console.WriteLine(element.InnerText);
```

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).