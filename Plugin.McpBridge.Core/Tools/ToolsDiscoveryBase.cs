using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Plugin.McpBridge.Data;

namespace Plugin.McpBridge.Tools;

internal abstract class ToolsDiscoveryBase
{
	public virtual IEnumerable<ToolMethodDto> GetTools()
	{
		foreach(MethodInfo method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
		{
			var attr = method.GetCustomAttribute<ToolAttribute>();

			if(attr != null)
			{
				String description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? String.Empty;

				Delegate del = method.CreateDelegate(GetDelegateType(method), this);
				AIFunction function = AIFunctionFactory.Create(del);
				yield return new ToolMethodDto(attr.ConfirmationRequired, method.Name, description, function);
			}
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
		} else
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