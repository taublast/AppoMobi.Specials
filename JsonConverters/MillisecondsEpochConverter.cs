using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AppoMobi.Specials.JsonConverters;

public class MillisecondsEpochConverter : DateTimeConverterBase
{
	private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds.ToString());
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null) return null;
		long l;
		if (!long.TryParse(reader.Value.ToString(), out l)) return null;
		return _epoch.AddMilliseconds(l);
	}
}