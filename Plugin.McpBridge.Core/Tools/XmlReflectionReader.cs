using System.Reflection;
using System.Xml;
using Plugin.McpBridge.Data;
using SAL.Flatbed;

namespace Plugin.McpBridge.Tools;

internal class XmlReflectionReader
{
	private readonly Dictionary<Assembly, XmlDocument> _documents = new Dictionary<Assembly, XmlDocument>();
	private readonly Dictionary<String, XmlReflectionDto> _documentationCache = new Dictionary<String, XmlReflectionDto>();
	private readonly Object _lock = new Object();

	public XmlDocument? LoadDocument(Assembly asm)
	{
		if(asm.GlobalAssemblyCache)
			return null;

		if(this._documents.TryGetValue(asm, out var document))
			return document;

		lock(_lock)
		{
			String path = GetXmlPath(asm.Location);
			if(!File.Exists(path))
			{
				path = GetXmlPath(new Uri(asm.CodeBase).LocalPath);
				if(!File.Exists(path))
					return null;
			}

			document = new XmlDocument();
			document.Load(path);
			this._documents.Add(asm, document);
			return document;
		}
	}

	private static String GetXmlPath(String assemblyLocation)
	{
		String? path = Path.GetDirectoryName(assemblyLocation);
		String xmlFileName = Path.GetFileNameWithoutExtension(assemblyLocation) + ".xml";
		return Path.Combine(path, xmlFileName);
	}

	public XmlReflectionDto? FindDocumentation(IPluginDescription plugin, IPluginMemberInfo member)
	{
		Assembly? asm = plugin.Instance?.GetType().Assembly;
		if(asm == null || asm.GlobalAssemblyCache)
			return null;

		return this.FindDocumentation(asm, XmlReflectionReader.GetMemberName(member));
	}

	private XmlReflectionDto? FindDocumentation(Assembly asm, String memberName)
	{
		if(asm == null || asm.GlobalAssemblyCache)
			return null;

		String key = asm.GetName().Name + ">" + memberName;

		if(this._documentationCache.TryGetValue(key, out XmlReflectionDto? result))
			return result;

		lock(_lock)
		{
			XmlDocument? doc = this.LoadDocument(asm);
			if(doc == null)
				result = null;
			else
			{
				var navigator = doc.CreateNavigator();
				var memberNode = navigator.SelectSingleNode(String.Format("/doc/members/member[@name=\"{0}\"]", memberName));
				if(memberNode == null)
					result = null;
				else
				{
					var summaryNode = memberNode.SelectSingleNode("summary");
					String summary = summaryNode == null ? String.Empty : summaryNode.InnerXml.Trim().Replace("  ", " ");
					var paramNodes = memberNode.Select("param");
					var parameters = new Dictionary<String, String>();
					while(paramNodes.MoveNext())
					{
						String? name = paramNodes.Current?.GetAttribute("name", String.Empty);
						if(!String.IsNullOrEmpty(name))
							parameters[name] = paramNodes.Current!.InnerXml.Trim().Replace("  ", " ");
					}
					result = new XmlReflectionDto(summary, parameters);
				}
			}
			this._documentationCache.Add(key, result);
			return result;
		}
	}

	private static String GetMemberName(IPluginMemberInfo member)
	{
		Char prefix;
		switch(member.MemberType)
		{
		case MemberTypes.Field:
			prefix = 'F';
			break;
		case MemberTypes.Property:
			prefix = 'P';
			break;
		case MemberTypes.TypeInfo:
			prefix = 'T';
			break;
		case MemberTypes.Method:
			prefix = 'M';
			break;
		default: throw new NotImplementedException();
		}

		String baseName = prefix + ":" + member.TypeName + "." + member.Name;
		if(member.MemberType != MemberTypes.Method)
			return baseName;

		IPluginParameterInfo[] parameters = ((IPluginMethodInfo)member).GetParameters().ToArray();
		if(parameters == null || parameters.Length == 0)
			return baseName;

		String args = String.Join(",", parameters.Select(a => XmlReflectionReader.GetXmlDocTypeName(a, a.IsOut)));
		return baseName + "(" + args + ")";
	}

	private static String GetXmlDocTypeName(IPluginTypeInfo argument, Boolean isOut = false)
	{
		if(argument == null)
			return String.Empty;
		if(isOut)
			return argument.TypeName + "@";
		if(argument.IsArray)
			return argument.TypeName + "[]";
		if(argument.IsGeneric)
		{
			String genericBase = argument.TypeName;
			String genericArgs = String.Join(",", argument.UnderlyingMembers.Select(a => XmlReflectionReader.GetXmlDocTypeName(a)));
			return genericBase + "{" + genericArgs + "}";
		}

		return argument.TypeName;
	}
}