# XML Tools Plugin

[![Auto build](https://github.com/DKorablin/Plugin.Tools.Xml/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Tools.Xml/releases/latest)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A [SAL](https://github.com/DKorablin/SAL.Windows) plugin that provides lightweight XML tooling for routine validation, querying, and transformation tasks directly within the host application.

## Features

### XSLT Tester
Apply an XSLT stylesheet to XML data and preview the output in real time.
- Enter the XSLT in the top pane and the source XML in the bottom pane.
- Press the toolbar button to execute the transformation and view the result.

### XPath Tester
Evaluate XPath expressions against XML data and inspect selected nodes.
- Enter an XPath expression in the top field (autocomplete from a history of up to 10 recent queries).
- Enter the source XML in the lower pane and press the toolbar button to see the matching nodes.

### XSD Validator / Generator
Validate XML documents against an XSD schema or generate an XSD schema from an existing XML document.
- **Validate** — paste both XML and XSD, then press *Check* to verify conformance.
- **Generate** — paste XML and press *Generate* to infer an XSD schema using the standard .NET tooling.

## Settings

| Setting | Description |
|---|---|
| **XML** | Default XML document pre-loaded into all tool panels on open. |
| **XPath MRU** | Semicolon-separated list of the 10 most recently used XPath expressions. |

## Installation

1. Download latest GitHub release package (.zip or .nupkg).
2. Place the plugin assembly into the host application plugin directory (SAL / host supporting Windows environment):
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog/)
	- [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.MDI (AvaloniaUI)](https://dkorablin.github.io/Flatbed-MDI-AvaloniaUI)
3. Restart the host application; Plugin.Tools.Xml should appear in the menu toolbar (Tools -> XML -> [XSLT, X-Path, XSD]).

## Requirements

- .NET Framework 4.8 **or** .NET 8.0 (Windows)
- SAL host application

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).