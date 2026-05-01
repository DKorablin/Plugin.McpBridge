using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Events;

namespace Plugin.McpBridge.Tools;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
internal sealed class ToolsFactory
{
	private readonly TraceSource _trace;
	private readonly Object[] _targets;

	public ToolsFactory(TraceSource trace, params Object[] toolsHosts)
	{
		if(toolsHosts == null || toolsHosts.Length == 0)
			throw new ArgumentException("At least one tools host must be provided.", nameof(toolsHosts));

		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._targets = toolsHosts;
	}

	/// <summary>Scans the executing assembly for methods decorated with <see cref="ToolAttribute"/>, mirroring the logic of <see cref="ToolsFactory.GetTools"/> without requiring live service instances.</summary>
	public static IEnumerable<(String MethodName, String Description)> DiscoverTools(Assembly discoveryAssembly)
	{
		HashSet<String> seen = new HashSet<String>(StringComparer.Ordinal);
		foreach(Type type in discoveryAssembly.GetTypes())
			foreach(MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				ToolAttribute? tool = method.GetCustomAttribute<ToolAttribute>();
				if(tool == null || !seen.Add(method.Name))
					continue;
				String description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? String.Empty;
				yield return (method.Name, description);
			}
	}

	public IEnumerable<(Object Target, ToolAttribute Tool, MethodInfo Method)> GetTools()
	{
		foreach(Object target in this._targets)
			foreach(MethodInfo method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				var attr = method.GetCustomAttribute<ToolAttribute>();
				if(attr != null)
					yield return (target, attr, method);
			}
	}

	public IEnumerable<AITool> CreateTools(String[]? permissions, EventHandler<AgentConfirmationEventArgs> confirmationHandler)
	{
		Boolean allAllowed = permissions == null || permissions.Length == 0;
		foreach(var method in this.GetTools())
		{
			if(!allAllowed && Array.Exists(permissions!, p => p == method.Method.Name))
				continue;

			Delegate del = method.Method.CreateDelegate(GetDelegateType(method.Method), method.Target);
			ToolFacade wrapper = new ToolFacade(this._trace, del);
			if(method.Tool.ConfirmationRequired)
				wrapper.ConfirmationRequired += (s, e) => confirmationHandler(s, e);

			yield return wrapper;
		}
	}

	public IEnumerable<AITool> CreateTools2(String[]? permissions)
	{
		Boolean allAllowed = permissions == null || permissions.Length == 0;
		foreach(var method in this.GetTools())
		{
			if(!allAllowed && Array.Exists(permissions!, p => p == method.Method.Name))
				continue;

			Delegate del = method.Method.CreateDelegate(GetDelegateType(method.Method), method.Target);
			yield return AIFunctionFactory.Create(del, new AIFunctionFactoryOptions{
				AdditionalProperties = new Dictionary<String, Object?>
				{
					{ nameof(ToolAttribute), method.Tool }
				}
			});
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
