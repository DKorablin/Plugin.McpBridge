using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge.Tests;

/// <summary>Factory for creating test doubles for IWindow</summary>
internal static class WindowTestFactory
{
	/// <summary>Custom control for testing that can be cast to IWindow</summary>
	/// <remarks>This is a workaround since IWindow comes from an external package</remarks>
	public class TestWindowControl : Control, IWindow
	{
		public IPluginDescription Plugin { get; set; }

		public Object Control { get; set; }

		public String Caption { get; set; }

		public Object Object { get; set; }

		public event EventHandler Shown;

		public event EventHandler<CancelEventArgs> Closing;

		public event EventHandler Closed;

		public void AddEventHandler(String eventName, EventHandler<DataEventArgs> handler)
		{
			// Empty implementation for testing
		}

		public void Close()
		{
			// Empty implementation for testing
		}

		public void RemoveEventHandler(String eventName, EventHandler<DataEventArgs> handler)
		{
			// Empty implementation for testing
		}

		public void SetDockAreas(DockAreas dockAreas)
		{
			// Empty implementation for testing
		}

		public void SetTabPicture(Object icon)
		{
			// Empty implementation for testing
		}
	}

	public class TestPluginDescription : IPluginDescription
	{
		String IPluginDescription.ID => throw new NotImplementedException();

		String IPluginDescription.Source => throw new NotImplementedException();

		public IPlugin Instance { get; }

		IPluginTypeInfo IPluginDescription.Type => throw new NotImplementedException();

		String IPluginDescription.Name => throw new NotImplementedException();

		Version IPluginDescription.Version => throw new NotImplementedException();

		String IPluginDescription.Description => throw new NotImplementedException();

		String IPluginDescription.Company => throw new NotImplementedException();

		String IPluginDescription.Copyright => throw new NotImplementedException();

		public TestPluginDescription(IPlugin plugin)
			=> this.Instance = plugin;
	}

	/// <summary>Creates a test window instance with sensible defaults</summary>
	public static TestWindowControl CreateTestWindow(IPlugin plugin)
	{
		var mockControl = new Control();
		var window = new TestWindowControl
		{
			Caption = "Test Window",
			Plugin = new TestPluginDescription(plugin),
			Control = mockControl,
			Object = mockControl
		};

		return window;
	}
}