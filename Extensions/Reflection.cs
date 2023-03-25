using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppoMobi.Specials.Extensions;

public static class Reflection

{
	public static object CombineObjects(this object item, object add)
	{
		if (item == null)
			return null;

		var ret = new ExpandoObject() as IDictionary<string, object>;

		var props = item.GetType().GetProperties();
		foreach (var property in props)
			if (property.CanRead)
				ret[property.Name] = property.GetValue(item);

		props = add.GetType().GetProperties();
		foreach (var property in props)
			if (property.CanRead)
				ret[property.Name] = property.GetValue(add);

		return ret;
	}


	/// <summary>
	///     JsonConvert.PopulateObject with ObjectCreationHandling.Replace. This is not raising OnPropertyChanged!!! Use
	///     MapProps..
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	public static void Populate(object source, object target)
	{
		var serializerSettings = new JsonSerializerSettings
		{
			Error = HandleDeserializationError,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace
		};
		var json = JsonConvert.SerializeObject(source, serializerSettings);
		JsonConvert.PopulateObject(json, target, serializerSettings);
	}

	public static IEnumerable<T> CloneList<T>(this IEnumerable<T> source)
  {
		if (ReferenceEquals(source, null)) return default;

		// In the PCL we do not have the BinaryFormatter
		return JsonConvert.DeserializeObject<IEnumerable<T>>(
			JsonConvert.SerializeObject(source, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Error = HandleDeserializationError
			}));
	}


	public static T Clone<T>(T source)
  {
		if (source == null)
			return default;


		var cloneJson = JsonConvert.SerializeObject(source, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Error = HandleDeserializationError
		});

		var tmpItem = JsonConvert.DeserializeObject(cloneJson, source.GetType(), new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Error = HandleDeserializationError
		});

		return (T)tmpItem;
	}

	public static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
	{
		var currentError = errorArgs.ErrorContext.Error.Message;
		errorArgs.ErrorContext.Handled = true;
		Debug.WriteLine($"[REFLECTION] HandleDeserializationError: {currentError}");
	}


	public static T ConvertTo<T>(object source)
	{
		var json = JsonConvert.SerializeObject(source, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Error = HandleDeserializationError
		});

		return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Error = HandleDeserializationError
		});
	}


	public static T Patch<T>(T target, dynamic source) where T : class
  {
		JsonConvert.PopulateObject(JsonConvert.SerializeObject(source), target, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Error = HandleDeserializationError
		});
		return target;
	}


	public static bool IsDifferent(object source, object other)
  {
		var different = false;


		try
		{
			// skip [JsonIgnore]
			//var attr = source.GetType().GetAttribute<JsonIgnoreAttribute>();
			//if (attr != null)
			//{
			//    return different;
			//}

			var jsSettings = new JsonSerializerSettings();
			jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

			var string1 = JsonConvert.SerializeObject(source, jsSettings);
			var string2 = JsonConvert.SerializeObject(other, jsSettings);
			if (string1 != string2) different = true;
		}
		catch (Exception e)
		{
			//different = true;
		}

		return different;
	}

	// https://stackoverflow.com/a/39679855/7149454

	public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
  {
		dynamic awaitable = @this.Invoke(obj, parameters);
		await awaitable;
		return awaitable.GetAwaiter().GetResult();
	}


	/// <summary>
	///     Set props values with reflection, if fails with sub props use Populate
	/// </summary>
	/// <param name="sourceItem"></param>
	/// <param name="targetItem"></param>
	public static void MapProps(dynamic sourceItem, dynamic targetItem)
	{
		var source = ((Type)sourceItem.GetType()).GetProperties();
		var sourcePropsInfos = source.DistinctBy(x => x.Name).ToArray();

		var target = ((Type)targetItem.GetType()).GetProperties();
		var targetPropsInfos = target.DistinctBy(x => x.Name).ToArray();

		foreach (var propertyInfo in sourcePropsInfos)
		{
			var propName = propertyInfo.Name;
			try
			{
				if (!string.IsNullOrEmpty(propName))
				{
					var val = GetPropertyValueFor(sourceItem, propertyInfo);
					var targetProperty = targetPropsInfos.FirstOrDefault(x => x.Name == propName);
					targetProperty.SetValue(targetItem, val, null);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[MapProps] {propName}: {ex}");
			}
		}
	}


	public static string GetUntilOrFull(this string text, string stopAt = ",")
  {
		if (!string.IsNullOrWhiteSpace(text))
		{
			var charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

			if (charLocation > 0) return text.Substring(0, charLocation);
		}

		return text;
	}

	public static ICollection CastCollection(ICollection self, Type innerType)
  {
		var methodInfo = typeof(ICollection).GetMethod("Cast");
		var genericMethod = methodInfo.MakeGenericMethod(innerType);
		return genericMethod.Invoke(null, new[] { self }) as ICollection;
	}


	public static IEnumerable Cast(IEnumerable self, Type innerType)
  {
		var methodInfo = typeof(Enumerable).GetMethod("Cast");
		var genericMethod = methodInfo.MakeGenericMethod(innerType);
		return genericMethod.Invoke(null, new[] { self }) as IEnumerable;
	}

	public static bool MapToProperty(this string value, PropertyInfo propertyInfo, dynamic model)
  {
		var refF = propertyInfo.ReflectedType;
		var outVal = propertyInfo.GetValue(model);
		var outType = propertyInfo.PropertyType;

		if (outVal is Enum)
		{
			if (!string.IsNullOrEmpty(value))
			{
				var vvalue = Enum.Parse(outType, value);
				var dVal = Convert.ToInt32(vvalue);
				propertyInfo.SetValue(model, dVal);
			}

			return true;
		}

		if (outType == typeof(byte) || outType == typeof(byte?) || outType == typeof(short) ||
			outType == typeof(short?))
		{
			if (!string.IsNullOrEmpty(value))
			{
				var check = value.TagsToList().First().ToLower().Trim();
				var isOn = check == "1" || check == "true" || check == "on" || check == "yes";
				try
				{
					if (isOn)
						propertyInfo.SetValue(model, (byte)1);
					else
						propertyInfo.SetValue(model, (byte)0);
				}
				catch
				{
					if (isOn)
						propertyInfo.SetValue(model, (short)1);
					else
						propertyInfo.SetValue(model, (short)0);
				}
			}
			else
			{
				//nullable empty
				try
				{
					propertyInfo.SetValue(model, null);
				}
				catch (Exception e)
				{
					propertyInfo.SetValue(model, 0);
				}
			}

			return true;
		}

		if (propertyInfo.PropertyType.FullName.Contains("DateTime"))
		{
			if (propertyInfo.PropertyType.FullName.Contains("Nullable") && string.IsNullOrEmpty(value))
			{
				propertyInfo.SetValue(model, null);
			}
			else
			{
				var dVal = DateTime.Parse(value).AsUtc();
				propertyInfo.SetValue(model, dVal);
			}

			return true;
		}

		if (propertyInfo.PropertyType.FullName.Contains("TimeSpan"))
		{
			if (propertyInfo.PropertyType.FullName.Contains("Nullable") && string.IsNullOrEmpty(value))
			{
				propertyInfo.SetValue(model, null);
			}
			else
			{
				TimeSpan dVal;
				try
				{
					//am/pm
					var dateTime = DateTime.ParseExact(value, "hh:mm tt", CultureInfo.InvariantCulture);
					dVal = dateTime.TimeOfDay;
				}
				catch (Exception e)
				{
					//24h
					dVal = TimeSpan.Parse(value);
				}

				propertyInfo.SetValue(model, dVal);
			}

			return true;
		}

		if (propertyInfo.PropertyType.FullName.Contains("Int32"))
		{
			if (!string.IsNullOrEmpty(value))
			{
				//normal int
				var iVal = int.Parse(value);
				propertyInfo.SetValue(model, iVal);
			}
			else
			{
				//nullable empty
				propertyInfo.SetValue(model, null);
			}

			return true;
		}

		if (propertyInfo.PropertyType.FullName.Contains("Int64"))
		{
			if (!string.IsNullOrEmpty(value))
			{
				//normal int
				var iVal = long.Parse(value);
				propertyInfo.SetValue(model, iVal);
			}
			else
			{
				//nullable empty
				propertyInfo.SetValue(model, null);
			}

			return true;
		}
		else if (propertyInfo.PropertyType.FullName.Contains("Double"))
		{
			value = value.Replace(',', '.');
			double iVal;
			double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out iVal);
			propertyInfo.SetValue(model, iVal);
			return true;
		}
		else if (propertyInfo.PropertyType.FullName.Contains("Decimal"))
		{
			decimal iVal;
			decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out iVal);
			propertyInfo.SetValue(model, iVal);
			return true;
		}
		else if (propertyInfo.PropertyType.FullName.Contains("Bool"))
		{
			if (!string.IsNullOrEmpty(value))
			{
				var check = value.TagsToList().First().ToLower().Trim();
				var isOn = check == "1" || check == "true" || check == "on" || check == "yes";

				// the value can be for ex.: "true,false" - new,old
				//var bRes = GetUntilOrFull(value);
				//var bVal = bool.Parse(bRes);
				propertyInfo.SetValue(model, isOn);
			}
			else
			{
				//nullable bool
				propertyInfo.SetValue(model, null);
			}

			return true;
		}

		return false;
	}


	public static bool MapToProperty(dynamic item, string propertyName, dynamic propertyValue)
  {
		try
		{
			var typeItem = item.GetType();
			var property = typeItem.GetProperty(propertyName);

			return MapToProperty(propertyValue.ToString(), property, item);
		}
		catch (Exception e)
		{
			return false;
		}
	}


	public static bool TrySetPropertyValue(dynamic item, string propertyName, dynamic propertyValue)
  {
		try
		{
			var typeItem = item.GetType();
			var property = typeItem.GetProperty(propertyName);
			if (property == null) return false;
			property.SetValue(item, propertyValue);
			return true;
		}
		catch (Exception e)
		{
			return false;
		}
	}

	public static dynamic TryGetStaticPropertyValue(Type typeItem, string propertyName)
	{
		try
		{
			return GetStaticPropertyValue(typeItem, propertyName);
		}
		catch (Exception e)
		{
		}

		return null;
	}

	public static dynamic GetStaticPropertyValue(Type typeItem, string propertyName)
	{
		if (typeItem == null) return null;

		var property = typeItem.GetProperty(propertyName);
		if (property == null)
			return null;

		return property.GetValue(null);
	}

	public static void TrySetStaticPropertyValue(Type typeItem, string propertyName, dynamic propertyValue)
	{
		try
		{
			SetStaticPropertyValue(typeItem, propertyName, propertyValue);
		}
		catch (Exception e)
		{
		}
	}


	public static dynamic SetStaticPropertyValue(Type typeItem, string propertyName, dynamic propertyValue)
	{
		if (typeItem == null) return null;

		var property = typeItem.GetProperty(propertyName);
		if (property == null)
			return null;

		return property.SetValue(null, propertyValue);
	}

	public static dynamic GetPropertyValueFor(dynamic item, string propertyName)
	{
		if (item == null) return null;
		var typeItem = item.GetType();
		var property = typeItem.GetProperty(propertyName);
		if (property == null) return null;
		return property.GetValue(item);
	}

	public static IEnumerable<FieldInfo> GetAllHiddenFields(this Type t)
	{
		if (t == null)
			return Enumerable.Empty<FieldInfo>();

		var flags = BindingFlags.NonPublic |
					//BindingFlags.Static | 
					BindingFlags.Instance;
		//BindingFlags.DeclaredOnly;
		return t.GetFields(flags).Concat(GetAllHiddenFields(t.BaseType));
	}


	public static dynamic GetPropertyValueFor(dynamic item, PropertyInfo property)
  {
		if (item == null) return null;
		if (property == null) return null;
		return property.GetValue(item);
	}


	public static dynamic GetPropertyValueUnsafeFor(dynamic item, string propertyName)
  {
		if (item == null) return null;
		var typeItem = item.GetType();
		var property = typeItem.GetProperty(propertyName);
		//if (property == null) return null; <- raise an exception if null
		return property.GetValue(item);
	}


	public static bool SetPropertyValue(dynamic item, string propertyName, dynamic propertyValue)
  {
		var typeItem = item.GetType();
		var property = typeItem.GetProperty(propertyName);
		if (property == null) return false;
		property.SetValue(item, propertyValue);
		return true;
	}


	//public static bool SetPropertyValueFor(dynamic item, string propertyName, object propertyValue)

	//{
	//    var typeItem = item.GetType();
	//    var property = typeItem.GetProperty(propertyName);
	//    if (property == null) return false;
	//    property.SetValue(item, propertyValue);
	//    return true;
	//}


	public static dynamic TryGetPropertyValueFor(dynamic item, string propertyName)
  {
		try
		{
			var typeItem = item.GetType();
			var property = typeItem.GetProperty(propertyName);
			if (property == null) return null;
			return property.GetValue(item);
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
			return null;
		}
	}


	public static bool TryInvokeMethod(dynamic item, string methodName)
  {
		var dt = item?.GetType();
		if (dt == null) return false;
		var method = dt.GetMethod(methodName);
		dynamic result = null;
		if (method != null)
			try
			{
				method.Invoke(item, null);
				return true;
			}
			catch (Exception e)
			{
			}

		return false;
	}


	//
	//public static bool TryInvokeMethod(dynamic item, string methodName, object param)
	//
	//{
	//    var dt = item?.GetType();
	//    if (dt == null) return false;
	//    var method = dt.GetMethod(methodName);
	//    dynamic result = null;
	//    object[] parameters = new object[] { new object[] { param } };
	//    if (method != null)
	//    {
	//        try
	//        {
	//            result = method.Invoke(item, parameters);
	//            return true;
	//        }
	//        catch (Exception e)
	//        {
	//        }               
	//    }
	//    return false;
	//}


	public static bool TryInvokeMethod(dynamic item, string methodName, dynamic param)
  {
		var dt = item?.GetType();
		if (dt == null) return false;
		var method = dt.GetMethod(methodName);
		dynamic result = null;
		object[] parameters = { param };
		if (method != null)
			try
			{
				result = method.Invoke(item, parameters);
				return true;
			}
			catch (Exception e)
			{
			}

		return false;
	}
}