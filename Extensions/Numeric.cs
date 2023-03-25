using System;
using System.Linq;

namespace AppoMobi.Specials.Extensions;

public static class NumericExtensions

{
	public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		return dtDateTime;
	}


	public static bool HasValue(this double value)
	{
		return !double.IsNaN(value) && !double.IsInfinity(value);
	}


	public static string ToUnicodeFractions(this double number, int precision = 4)
  {
		var nArray = "⁰¹²³⁴⁵⁶⁷⁸⁹";
		var dArray = "₀₁₂₃₄₅₆₇₈₉";

		if (!number.HasValue()) return "";

		int w, n, d;
		RoundToMixedFraction(number, precision, out w, out n, out d);
		var ret = $"{w}";
		if (w > 0)
		{
			if (n > 0)
			{
				var nUnicode = nArray.Substring(n, 1);
				var dUnicode = dArray.Substring(d, 1);
				ret = $"{w} {nUnicode}/{dUnicode}";
			}
			else
			{
				ret = $"{w}";
			}
		}
		else
		{
			if (n > 0)
			{
				var nUnicode = nArray.Substring(n, 1);
				var dUnicode = dArray.Substring(d, 1);
				ret = $"{nUnicode}/{dUnicode}";
			}
			else
			{
				ret = $"{w}"; // always 0
			}
		}

		return ret;
	}


	public static string ToUnicodeFractions4(this double number)
  {
		if (!number.HasValue()) return "";

		var precision = 4;

		int w, n, d;
		RoundToMixedFraction(number, precision, out w, out n, out d);
		var ret = $"{w}";
		if (w > 0)
		{
			if (n > 0) //has a fraction
			{
				//todo unicode
				var fraction = "error";
				switch (d)
				{
					case 2:
						fraction = "½";
						break;
					case 3:
						if (n == 1)
							fraction = "⅓";
						else
							fraction = "⅔";
						break;
					case 4:
						if (n == 1)
							fraction = "¼";
						else if (n == 2)
							fraction = "²/₄";
						else
							fraction = "¾";
						break;
				}

				//ret = $"{w} {n}/{d}";
				ret = $"{w} {fraction}";
			}
			else
			{
				ret = $"{w}";
			}
		}
		else
		{
			if (n > 0) //has a fraction
			{
				//todo unicode
				var fraction = "error";
				switch (d)
				{
					case 2:
						fraction = "½";
						break;
					case 3:
						if (n == 1)
							fraction = "⅓";
						else
							fraction = "⅔";
						break;
					case 4:
						if (n == 1)
							fraction = "¼";
						else if (n == 2)
							fraction = "²/₄";
						else
							fraction = "¾";
						break;
				}

				//ret = $"{w} {n}/{d}";
				ret = $"{fraction}";
			}
			else
			{
				ret = $"{w}";
			}
		}

		return ret;
	}


	public static string ToUnicodeFractions3(this double number, bool showOne = true)
  {
		if (!number.HasValue()) return "";

		var precision = 3;

		int w, n, d;
		RoundToMixedFraction(number, precision, out w, out n, out d);
		var ret = $"{w}";
		if (w > 0)
		{
			if (n > 0) //has a fraction
			{
				//todo unicode
				var fraction = "error";
				switch (d)
				{
					case 2:
						fraction = "½";
						break;
					case 3:
						if (n == 1)
							fraction = "⅓";
						else
							fraction = "⅔";
						break;
					case 4:
						if (n == 1)
							fraction = "¼";
						else if (n == 2)
							fraction = "²/₄";
						else
							fraction = "¾";
						break;
				}

				//ret = $"{w} {n}/{d}";
				ret = $"{w} {fraction}";
			}
			else
			{
				ret = $"{w}";
			}
		}
		else
		{
			if (n > 0) //has a fraction
			{
				//todo unicode
				var fraction = "error";
				switch (d)
				{
					case 2:
						fraction = "½";
						break;
					case 3:
						if (n == 1)
							fraction = "⅓";
						else
							fraction = "⅔";
						break;
					case 4:
						if (n == 1)
							fraction = "¼";
						else if (n == 2)
							fraction = "²/₄";
						else
							fraction = "¾";
						break;
				}

				//ret = $"{w} {n}/{d}";
				ret = $"{fraction}";
			}
			else
			{
				ret = $"{w}";
			}
		}

		if (!showOne)
			if (ret == "1")
				ret = "";

		return ret.Trim();
	}


	public static string ToFractions(this double number, int precision = 4)
  {
		if (!number.HasValue()) return "";

		int w, n, d;
		RoundToMixedFraction(number, precision, out w, out n, out d);
		var ret = $"{w}";
		if (w > 0)
		{
			if (n > 0)
				ret = $"{w} {n}/{d}";
			else
				ret = $"{w}";
		}
		else
		{
			if (n > 0)
				ret = $"{n}/{d}";
			else
				ret = $"{w}";
		}

		return ret;
	}

	private static void RoundToMixedFraction(double input, int accuracy, out int whole, out int numerator,
		out int denominator)
  {
		double dblAccuracy = accuracy;
		whole = (int)Math.Truncate(input);
		var fraction = Math.Abs(input - whole);
		if (fraction == 0)
		{
			numerator = 0;
			denominator = 1;
			return;
		}

		var n = Enumerable.Range(0, accuracy + 1).SkipWhile(e => e / dblAccuracy < fraction).First();
		var hi = n / dblAccuracy;
		var lo = (n - 1) / dblAccuracy;
		// if ((fraction - lo) < (hi - fraction)) n; // закомментил для округления в большую сторону
		if (n == accuracy)
		{
			whole++;
			numerator = 0;
			denominator = 1;
			return;
		}

		var gcd = GCD(n, accuracy);
		numerator = n / gcd;
		denominator = accuracy / gcd;
	}

	private static int GCD(int a, int b)
	{
		if (b == 0)
			return a;
		return GCD(b, a % b);
	}
}