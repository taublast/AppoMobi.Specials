using System;

namespace AppoMobi.Specials;

public static class DateTimeExtensions
{
	public static bool IsPast(this DateTime? value)
	{
		if (value == null)
			return false;

		var dateAndTime = DateTime.Now;
		//var today = dateAndTime.Date;
		if (value < dateAndTime)
			return true;

		return false;
	}

	/// <summary>
	///     Used to be named ApiTime
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	public static string ToStringSimple(this DateTime time)
	{
		return $"DateTime({time.Year},{time.Month},{time.Day},{time.Hour},{time.Minute},{time.Second})";
	}

	public static string ToStringUtc(this DateTime time)
	{
		return $"DateTime({time.Ticks}, DateTimeKind.Utc)";
	}

	public static DateTime AsUtc(this DateTime dateTime)
	{
		if (dateTime.Kind == DateTimeKind.Utc) return dateTime;
		return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
	}

	public static DateTime UtcToLocal(this DateTime time)
	{
		var convertedDate = DateTime.SpecifyKind(
			time,
			DateTimeKind.Utc);


		var ret = convertedDate.ToLocalTime();


		return ret;
	}
}