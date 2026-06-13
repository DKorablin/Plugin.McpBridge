# Port Test Client plugin
[![Auto build](https://github.com/DKorablin/Plugin.PortClient/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.PortClient/releases/latest)

A [SAL](https://github.com/DKorablin/SAL.Windows) plugin for testing open or closed ports on remote servers. Servers are organized into named **groups**, each containing one or more host entries. Every host can have multiple **port rules** attached with configurable protocol, socket type, and connection timeout.

## Features

- **Server groups** — organize hosts into logical groups for batch testing
- **Port rules** — define individual ports or ranges (e.g. `80,443,8000-8080`) per server
- **Protocol support** — TCP/UDP with configurable `SocketType` and `ProtocolType` per rule
- **ICMP ping** — test host reachability alongside port connectivity
- **IPv4 / IPv6** — explicit address-family selection per server
- **Configurable output** — customize the log message format using named placeholders:

  | Placeholder | Description |
  |---|---|
  | `{IpAddress}` | Resolved IP address |
  | `{Port}` | Port number |
  | `{ServerName}` | Server name |
  | `{ServerComments}` | Server description |
  | `{PortComments}` | Port rule description |
  | `{IsConnected}` | Connection result (`True`/`False`) |
  | `{Status}` | Human-readable status |
  | `{Elapsed}` | Time spent testing |

- **Log filtering** — choose to log all results, only open ports, or only closed ports
- **Persistent project file** — server and port configuration saved as `ServersSettings.xml`

## Requirements

- .NET Framework 4.8 **or** .NET 8 (Windows)
- SAL host application

## Settings

| Setting | Description | Default |
|---|---|---|
| Log errors | Log socket exceptions during testing | `false` |
| Log port state | Filter which port states are logged | `All` |
| Log message format | Output message template | `{IpAddress}:{Port} {ServerComments}{PortComments} {Status}` |