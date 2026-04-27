using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Events;
using Plugin.McpBridge.Tools;

namespace Plugin.McpBridge.Helpers;

/// <summary>Creates <see cref="ToolFacade"/> instances from methods decorated with <see cref="ToolAttribute"/> using reflection.</summary>
internal sealed class ToolFactory
{
	private readonly TraceSource _trace;
	private readonly Object[] _targets;

	public ToolFactory(TraceSource trace, params Object[] toolsHosts)
	{
		if(toolsHosts == null || toolsHosts.Length == 0)
			throw new ArgumentException("At least one tools host must be provided.", nameof(toolsHosts));

		this._trace = trace ?? throw new ArgumentNullException(nameof(trace));
		this._targets = toolsHosts;
	}

	public IEnumerable<AITool> CreateTools(Settings.Tools permissions, EventHandler<AgentConfirmationEventArgs> confirmationHandler)
	{
		foreach(Object target in this._targets)
			foreach(MethodInfo method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				ToolAttribute? attr = method.GetCustomAttribute<ToolAttribute>();
				if(attr == null)
					continue;

				if(!permissions.HasFlag(attr.ToolName))
					continue;

				Delegate del = method.CreateDelegate(GetDelegateType(method), target);
				ToolFacade wrapper = new ToolFacade(this._trace, del);
				if(attr.ConfirmationRequired)
					wrapper.ConfirmationRequired += (Object? s, AgentConfirmationEventArgs e) => confirmationHandler(s, e);

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
