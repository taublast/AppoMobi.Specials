using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace AppoMobi.Specials.Localization;

public class LocalizeData
{
	private readonly ResourceManager _resourceManager;

	public LocalizeData(Type resources)
	{
		//_resourceManager = new global::System.Resources.ResourceManager("AppoMobi.NetCore.ResX.ResStrings", typeof(ResStrings).Assembly);
		_resourceManager = new ResourceManager(resources.FullName, resources.Assembly);
	}

	public string ConvertToString(Enum value, CultureInfo culture)
	{
		return GetValueText(culture, value);
	}

	public List<KeyValuePair<Enum, string>> GetValues(Type enumType, CultureInfo culture)
	{
		var result = new List<KeyValuePair<Enum, string>>();
		var converter = TypeDescriptor.GetConverter(enumType);
		foreach (Enum value in Enum.GetValues(enumType))
		{
			var pair = new KeyValuePair<Enum, string>(value, converter.ConvertToString(null, culture, value));
			result.Add(pair);
		}

		return result;
	}

	private string GetValueText(CultureInfo culture, object value)
	{
		var type = value.GetType();
		var resourceName = string.Format("{0}_{1}", type.Name, value);
		var result = _resourceManager.GetString(resourceName, culture);
		if (result == null)
			result = resourceName;
		return result;
	}

	//static public List<KeyValuePair<Enum, string>> GetValues(Type enumType)
	//{
	//    return GetValues(enumType, CultureInfo.CurrentUICulture);
	//}
}