using System;
using Newtonsoft.Json.Linq;

namespace AppoMobi.Specials;

public static class JObjectExtensions
{
	public static string GetField(this JObject obj, string fieldName)
  {
		var ret = "";
		try
		{
			ret = obj[fieldName].ToString().Replace("\"", "");
		}
		catch (Exception e)
		{
		}

		return ret;
	}

	public static JToken GetToken(this JObject obj, string fieldName)
  {
		JToken ret = null;
		try
		{
			ret = obj[fieldName];
		}
		catch (Exception e)
		{
		}

		return ret;
	}

	public static JToken GetToken(this JToken obj, string fieldName)
  {
		JToken ret = null;
		try
		{
			ret = obj[fieldName];
		}
		catch (Exception e)
		{
		}

		return ret;
	}
}