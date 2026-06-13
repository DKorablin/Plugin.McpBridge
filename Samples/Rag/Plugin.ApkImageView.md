# APK (Android Package) Content Viewer Plugin
[![Auto build](https://github.com/DKorablin/Plugin.ApkImageView/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.ApkImageView/actions)

[![UI Screenshot](.github/assets/UI-1-200.png)](.github/assets/UI-1.png)

## Overview

This project is a plugin designed to dissect and display the contents of Android Package (APK) files. It provides a user-friendly interface to navigate the internal structure of an APK archive, allowing for in-depth analysis of its components.

The plugin can parse and display various file formats commonly found within APKs, presenting them in a structured and human-readable format.

## Features

-   **Hierarchical View**: Browse APK contents in a tree-like table of contents.
-   **Manifest Viewer**: Decodes and displays `AndroidManifest.xml` from its binary XML format.
-   **Resource Inspector**: Parses and shows the contents of `resources.arsc` files.
-   **DEX File Support**: Reads and displays information from Dalvik Executable (`.dex`) files.
-   **Native Library Parsing**: Handles ELF (`.so`) shared libraries.
-   **Java Class Viewer**: Basic support for viewing Java Class (`.class`) files.
-   **Archive Exploration**: Treats the `.apk` file as a `.zip` archive, allowing access to all contained files.

## Supported Formats

The plugin has specialized viewers and parsers for the following formats:

-   Android Package (`.apk`)
-   Android Binary XML (`AndroidManifest.xml`)
-   Android Resource Table (`.arsc`)
-   Dalvik Executable (`.dex`)
-   ELF Shared Libraries (`.so`)
-   Java Class files (`.class`)

## Technology

-   **Language**: C#
-   **Frameworks**: .NET 8 and .NET Framework 4.8
-   **UI**: Windows Forms

## Installation
To install the APK Content Viewer Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.ApkImageView/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)