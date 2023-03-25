using System;
using System.Resources;

namespace AppoMobi.Specials.Localization;

public class LocalizedEnumConverter : ResourceEnumConverter
{
	public LocalizedEnumConverter(Type type)
		: base(type, new ResourceManager(type.AssemblyQualifiedName, type.Assembly)) //"AppoMobi.Common.ResX.ResStrings"
	{
		// Assembly.GetExecutingAssembly()
		//var debug = type.AssemblyQualifiedName;
		//var debug1 = type.Assembly;
	}


	public static Type thisType => typeof(LocalizedEnumConverter);
}