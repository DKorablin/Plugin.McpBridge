# Winlogon notification plugin
[![Auto build](https://github.com/DKorablin/Plugin.Winlogon/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.Winlogon/releases/latest)

This plugin monitors and provides notifications for various Windows session events, such as user logon/logoff, workstation lock/unlock, screensaver activity, and system suspend.

It can be used in combination with other plugins by subscribing to its events. By default, it can write messages to Trace listeners, which can be viewed in the Output window if redirected.

## Features

*	Tracks user logon and logoff.
*	Tracks workstation lock and unlock.
*	Tracks screensaver start and stop.
*	Tracks when the system is suspending.
*	Provides distinct events for each of these activities.
*	Includes an option to automatically log these events.
*	Calculates and logs cumulative "working hours" for the current day based on user activity.

## Installation
To install the Winlogon notification Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.Winlogon/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.WorkerService](https://dkorablin.github.io/Flatbed-WorkerService)

## Events

The plugin exposes the following events:

*	`Logon`: Fired when a user logs on.
*	`Logoff`: Fired when a user logs off.
*	`DisplayLock`: Fired when the workstation is locked.
*	`DisplayUnlock`: Fired when the workstation is unlocked.
*	`StartScreenSaver`: Fired when the screensaver starts.
*	`StopScreenSaver`: Fired when the screensaver stops.
*	`Suspending`: Fired when the system is about to be suspended.

### Event Data

All events, except for `Suspending`, provide a `DataEventArgs` object that contains the following data:

*	`UserName` (String): The name of the user associated with the event.
*	`SessionId` (UInt32): The ID of the session where the event occurred.

The `Suspending` event provides `DataEventArgs.Empty` as it is not associated with a specific user.

## Configuration

The plugin has one configuration setting:

*   **EnableLogging** (Boolean): When set to `true`, the plugin will log all captured events to the configured `Trace` listeners. The default value is `false`.

## Usage Example

Below is an example of how to attach to this plugin's events and access the data:

```csharp
// Get the plugin instance from the host
IPluginDescription winlogonPluginInfo = this.Host.Plugins.Plugins["6b7a3e25-8cef-4549-85a0-915114875dce"];
if (winlogonPluginInfo == null)
{
	System.Diagnostics.Debug.WriteLine("Winlogon plugin not found.");
	return;
}

// Subscribe to the events
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("Logon").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_Logon));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("Logoff").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_Logoff));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("DisplayLock").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_DisplayLock));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("DisplayUnlock").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_DisplayUnlock));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("StartScreenSaver").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_StartScreenSaver));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("StopScreenSaver").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_StopScreenSaver));
winlogonPluginInfo.Type.GetMember<IPluginEventInfo>("Suspending").AddEventHandler(new EventHandler<DataEventArgs>(Winlogon_Suspending));

// Event handlers

private void Winlogon_Logon(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_Logon: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_Logoff(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_Logoff: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_DisplayLock(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_DisplayLock: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_DisplayUnlock(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_DisplayUnlock: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_StartScreenSaver(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_StartScreenSaver: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_StopScreenSaver(object sender, DataEventArgs e)
{
	string userName = e.GetData<string>("UserName");
	uint sessionId = e.GetData<uint>("SessionId");
	System.Diagnostics.Debug.WriteLine($"Winlogon_StopScreenSaver: User '{userName}' (Session: {sessionId})");
}

private void Winlogon_Suspending(object sender, DataEventArgs e)
{
	// This event does not contain user data.
	System.Diagnostics.Debug.WriteLine("Winlogon_Suspending: System is suspending.");
}