namespace Plugin.McpBridge.Extenders;

internal static class TypeExtender
{
	public static Type GetRealType(this Type type)
	{
		if(type.IsGenericType)
		{
			Type genericType = type.GetGenericTypeDefinition();
			if(genericType == typeof(System.Nullable<>)
				|| genericType == typeof(System.Collections.Generic.IEnumerator<>)
				|| genericType == typeof(System.Collections.Generic.IEnumerable<>)
				/*|| genericType == typeof(System.Collections.Generic.SortedList<,>)*/)
				return type.GetGenericArguments()[0].GetRealType();
		}
		if(type.HasElementType)
			//if(type.BaseType == typeof(Array))//+For out and ref parameters
			return type.GetElementType().GetRealType();
		return type;
	}
}