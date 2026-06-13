# Plugin.CryptoUI

[![Auto build](https://github.com/DKorablin/Plugin.CryptoUI/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.CryptoUI/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A Windows Forms plugin for certificate generation, conversion, and cryptographic operations based on the Bouncy Castle cryptography library. Part of the SAL (Software Abstraction Layer) plugin ecosystem.

## Features

### Certificate Generation
- **Self-Signed Certificates**: Create self-signed X.509 certificates (.crt)
- **PKCS#12 Certificates**: Generate certificates with private keys (.pfx) protected by password
- Configurable validity periods (from/to dates)
- Adjustable encryption strength (default: `2048-bit`)
- Multiple signature algorithms support (default: `SHA256WITHRSA`)

### Certificate Conversion
- **PEM to PKCS#12**: Convert certificate (.crt) and privacy-enhanced mail (.pem) files to PKCS#12 format (.pfx)
- **Certificate to CSR**: Convert certificates to Certificate Signing Request (CSR) in PEM format

### APNS Key Generation
- Generate Elliptic Curve (EC) key pairs for Apple Push Notification Service
- Create private keys (.p8) for generating APNS access tokens
- Create public keys (.pem) for validating JWT access tokens
- Configurable EC algorithms (default: `secp256r1`)
- Support for various X9 object identifiers

## Installation
To install the CryptoUI Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.ReflectionSearch/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)

## Technical Details

### Target Frameworks
- **.NET Framework 4.8** - For legacy Windows applications
- **.NET 8.0 (Windows)** - For modern Windows applications

### Dependencies
- **Portable.BouncyCastle** (v1.9.0) - Cryptographic operations
- **SAL.Windows** (v1.2.12) - Plugin framework and UI infrastructure

### Architecture
The plugin uses a modular architecture where each cryptographic operation is implemented as a separate module implementing the `ICertificateUI` interface:
- `GenerateCertificate` - Certificate generation
- `ConvertPemToPkcs12` - PEM to PKCS#12 conversion
- `ConvertCertToCsrPem` - Certificate to CSR conversion
- `GenerateApns` - APNS key pair generation


### GitHub Packages
The package is also available on GitHub Packages:
```bash
dotnet add package AlphaOmega.SAL.Plugin.CryptoUI --source https://nuget.pkg.github.com/DKorablin/index.json
```

### Binary Releases
Pre-built binaries for both .NET Framework 4.8 and .NET 8.0 are available in the [Releases](https://github.com/DKorablin/Plugin.CryptoUI/releases/latest) section. Each release includes the plugin and its dependencies packaged together.

## Usage

This plugin integrates with the SAL.Windows plugin framework. Once loaded, it provides a user interface with the following workflow:

1. Select a cryptographic operation from the available modules
2. Configure operation parameters in the property grid
3. Execute the operation
4. Save the result to a file

### Example Operations

**Generate a Self-Signed Certificate:**
- Subject: `CN=example.com, O=Example Org, C=US`
- Valid From: `2024-01-01`
- Valid To: `2025-01-01`
- Strength: `2048`
- Algorithm: `SHA256WITHRSA`
- Password: *(leave empty for self-signed)*

**Convert PEM to PKCS#12:**
- Certificate Path: `path/to/certificate.crt`
- PEM Path: `path/to/private-key.pem`
- Password: `your-secure-password`

**Generate APNS Keys:**
- EC Algorithm: `secp256r1`
- Generate Public Key: `true`
- Generate Private Key: `true`

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- .NET Framework 4.8 Developer Pack

### Build Steps
```bash
# Clone the repository with submodules
git clone --recursive https://github.com/DKorablin/Plugin.CryptoUI.git
cd Plugin.CryptoUI

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build -c Release
```

## CI/CD Pipeline

The project uses GitHub Actions for automated builds and releases:
- **Automated Versioning**: Version numbers are automatically incremented based on commit history
- **Multi-Target Build**: Builds for both .NET Framework 4.8 and .NET 8.0
- **Code Signing**: Release assemblies are digitally signed
- **NuGet Publishing**: Packages are published to GitHub Packages
- **GitHub Releases**: Binary releases are created with all dependencies included

## Project Structure

```
Plugin.CryptoUI/
├── Data/                          # Core cryptographic operations
│   ├── GenerateCertificate.cs     # Certificate generation logic
│   ├── ConvertPemToPkcs12.cs      # PEM to PKCS#12 conversion
│   ├── ConvertCertToCsrPem.cs     # Certificate to CSR conversion
│   ├── GenerateApns.cs            # APNS key generation
│   └── ICertificateUI.cs          # Module interface
├── UI/                            # Custom UI editors
│   ├── CertFileOpener.cs          # Certificate file selector
│   ├── PemFileOpener.cs           # PEM file selector
│   ├── EncryptionAlgorithmEditor.cs
│   ├── EcAlgorithmEditor.cs
│   └── X9ObjectIdentifierEditor.cs
├── PanelCryptoUI.cs               # Main plugin panel
├── PluginWindows.cs               # Plugin entry point
└── BouncyCastleReflection.cs      # Bouncy Castle helpers
```

## Related Projects

- [SAL.Flatbed](https://github.com/DKorablin/SAL.Flatbed) - Software Abstraction Layer BCL
- [SAL.Windows](https://github.com/DKorablin/SAL.Windows) - Software Abstraction Layer BCL for Windows
- [Flatbed.Dialog.Lite](https://github.com/DKorablin/Flatbed.Dialog.Lite) - Dependency used in releases

## Support

For issues, questions, or contributions, please visit the [GitHub Issues](https://github.com/DKorablin/Plugin.CryptoUI/issues) page.