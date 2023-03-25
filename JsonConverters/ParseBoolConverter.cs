using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AppoMobi.Specials.JsonConverters;

public class ParseBoolConverter : JsonConverter
{
	public override bool CanConvert(Type t)
	{
		return t == typeof(bool) || t == typeof(int);
	}

	public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null) return null;
		try
		{
			var maybeBool = serializer.Deserialize<bool>(reader);
			return maybeBool;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}

		try
		{
			var maybeInt = serializer.Deserialize<int>(reader);
			return maybeInt == 1;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}

		throw new Exception($"[ParseBoolConverter] Cannot unmarshal {reader.Value} to bool");
	}

	public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
	{
		if (untypedValue == null)
		{
			serializer.Serialize(writer, null);
			return;
		}

		var value1 = (bool)untypedValue;
		serializer.Serialize(writer, value1.ToString());
	}
}