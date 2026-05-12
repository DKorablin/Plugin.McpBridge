using System.Reflection;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Events;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.McpBridge.Tools;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
internal sealed class ToolsFactory
{
	private readonly ITraceSource _trace;
	private readonly ToolsDiscoveryBase[] _targets;

	public ToolsFactory(ITraceSource? trace, IHost host)
	{
		List<ToolsDiscoveryBase> tools = new List<ToolsDiscoveryBase>()
		{
			new PluginSettingsTools(host),
			new PluginMethodsTools(host),
			new ShellTools(),
		};

		if(host is IHostWindows hostWindows)
			tools.Add(new WindowsTools(hostWindows));

		this._trace = trace;
		this._targets = tools.ToArray();
	}

	public ToolsFactory(ITraceSource trace, params ToolsDiscoveryBase[] toolsHosts)
	{
		if(toolsHosts == null || toolsHosts.Length == 0)
			throw new ArgumentException("At least one tools host must be provided.", nameof(toolsHosts));

		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._targets = toolsHosts;
	}

	public IEnumerable<(Object Target, ToolAttribute Tool, MethodInfo Method, String Description)> GetTools()
	{
		foreach(ToolsDiscoveryBase target in this._targets)
			foreach(var tool in target.GetTools())
				yield return tool;
	}

	public IEnumerable<AITool> CreateTools(String[]? permissions, EventHandler<AgentConfirmationEventArgs>? confirmationHandler)
	{
		Boolean allAllowed = permissions == null || permissions.Length == 0;
		foreach(var method in this.GetTools())
		{
			if(!allAllowed && Array.Exists(permissions!, p => p == method.Method.Name))
				continue;

			Delegate del = method.Method.CreateDelegate(GetDelegateType(method.Method), method.Target);
			ToolFacade wrapper = new ToolFacade(this._trace, del);
			if(method.Tool.ConfirmationRequired && confirmationHandler != null)//This ignore type is added because of DevUI host (We don't need to intercept DevUI request)
				wrapper.ConfirmationRequired += (s, e) => confirmationHandler(s, e);

			yield return wrapper;
		}
	}

	private static Type GetDelegateType(MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters();
		Type returnType = method.ReturnType;

		if(returnType == typeof(void))
		{
			Type[] paramTypes = parameters.Select(p => p.ParameterType).ToArray();
			return paramTypes.Length switch
			{
				0 => typeof(Action),
				1 => typeof(Action<>).MakeGenericType(paramTypes),
				2 => typeof(Action<,>).MakeGenericType(paramTypes),
				3 => typeof(Action<,,>).MakeGenericType(paramTypes),
				4 => typeof(Action<,,,>).MakeGenericType(paramTypes),
				_ => throw new NotSupportedException($"Too many parameters on method '{method.Name}'.")
			};
		}
		else
		{
			Type[] typeArgs = parameters.Select(p => p.ParameterType).Append(returnType).ToArray();
			return typeArgs.Length switch
			{
				1 => typeof(Func<>).MakeGenericType(typeArgs),
				2 => typeof(Func<,>).MakeGenericType(typeArgs),
				3 => typeof(Func<,,>).MakeGenericType(typeArgs),
				4 => typeof(Func<,,,>).MakeGenericType(typeArgs),
				5 => typeof(Func<,,,,>).MakeGenericType(typeArgs),
				_ => throw new NotSupportedException($"Too many parameters on method '{method.Name}'.")
			};
		}
	}
}
