using System;

namespace AppoMobi.Specials.Localization;

public static class LocalisationExtensions

{
	public static string ToLocalizedString(this Enum myEnum)
  {
		var typeVal = myEnum.GetType();
		var index = 0;
		var hehe = ResourceEnumConverter.GetValues(typeVal);
		foreach (var pair in hehe)
		{
			if (Convert.ToInt32(myEnum) == index) return pair.Value;


			index++;
		}

		return myEnum.ToString();
	}
}