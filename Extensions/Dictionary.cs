using System;
using System.Collections;
using System.Collections.Generic;

namespace AppoMobi.Specials;

public static class DictionaryExtensions

{
	public static string GetString(this IDictionary items, string key)
  {
		try
		{
			return items[key].ToString();
		}
		catch (Exception e)
		{
			return "";
		}
	}


	public static string GetString(this IDictionary<object, object> items, string key)
  {
		try
		{
			return items[key].ToString();
		}
		catch (Exception e)
		{
			return "";
		}
	}


	public static void SaveString(this IDictionary<object, object> items, string key, string value)
  {
		items[key] = value;
	}


	public static void SaveString(this IDictionary items, string key, string value)
  {
		items[key] = value;
	}


	public static object Get(this IDictionary items, string key)
  {
		return items[key];
	}


	public static void Set(this IDictionary items, string key, object value)
  {
		items[key] = value;
	}
}