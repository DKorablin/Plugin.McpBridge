# Plugin.HttpClient

[![Auto build](https://github.com/DKorablin/Plugin.HttpClient/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.HttpClient/releases/latest)

Plugin.HttpClient is a dual-target (.NET Framework 4.8 / .NET 8) SAL plugin that provides:

* An HTTP/HTTPS test client (request editing, templating, auth, headers, cookies, certificates, proxy)
* A lightweight in-process mock/test HTTP server with routing to saved project items
* Project persistence (binary, XML, JSON) with template substitution for dynamic values
* Assembly endpoint import (reflection-based route discovery)
* Result logging (status, headers, body), formatting (JSON pretty print), and history tracking

Use it to explore APIs, build repeatable test suites, mock endpoints, and capture responses inside SAL host applications.

## Developer Perspective

Focus: lightweight, minimal dependencies, dual-target (.NET Framework 3.5 / .NET 8) with limited conditional compilation.

Core components:
* HttpProject / HttpProjectItem: hierarchical request & response tree with template substitution.
* Serialization: Binary (legacy), DataContract XML, DataContract JSON (ctor differences handled for .NET 8) with lazy non-serialized helpers.
* TemplateEngine: applies placeholders to outgoing requests and extracts response values for chained scenarios.
* RequestBuilder: reflection property mapping (filtered by CategoryAttribute) from HttpItem to HttpWebRequest; emits curl & PowerShell scripts.
* Mock server: HttpServerFacade + HttpListenerWrapper route inbound calls to stored items, simple auth, history logging.
* Endpoint import: AssemblyAnalyzer scans PE metadata (AlphaOmega.Debug) without loading into AppDomain to discover route-prefixed types.
* Result formatting: captures status/headers/body/elapsed, pretty-prints JSON.
* Collections: SerializableDictionary & HttpProjectItemCollection control XML structure and traversal.

Cross-target notes:
* Shared logic sticks to APIs available in .NET 3.5 (HttpWebRequest instead of HttpClient).
* Conditional compilation used for DataContractJsonSerializer constructor on NET8.

Testing workflow:
* Import routes from assemblies, set expected responses, exercise via embedded server.
* Validate template extraction with placeholder tokens in mock responses.

Build & packaging:
* Directory.Build.props centralizes version and output path per TargetFramework.
* Public types retained for serializer access.

Future / migration:
* Plan BinaryFormatter removal (provide modern alternative under NET8).
* Optional ASP.NET Core hosting (Minimal API) for richer protocol support alongside HttpListener for NET35.