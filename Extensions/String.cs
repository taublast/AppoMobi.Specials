using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppoMobi.Specials;

public static class StringExtensions

{
	public static bool IsEither(this string value, params string[] strings)
	{
		foreach (var line in strings)
			if (value == line)
				return true;
		return false;
	}


	public static string ExplainToString(this int num, string zero, string one, string with_one, string with_two,
		string other)
  {
		return CorrectStringUponNumber(num, zero, one, with_one, with_two, other);
	}


	public static string CorrectStringUponNumber(int num, string zero, string one, string with_one, string with_two,
		string other)
  {
		var ret = "";
		var lastDigit = "";
		var iDigit = 0;
		if (num > 0)
		{
			lastDigit = num.ToString().Substring(num.ToString().Length - 1);
			iDigit = int.Parse(lastDigit);
		}

		if (num < 1)
		{
			ret = zero;
		}
		else
		{
			if (num == 1)
			{
				ret = string.Format(one, num);
			}
			else if (iDigit == 1 && !(num >= 10 && num <= 20))
			{
				ret = string.Format(with_one, num);
			}
			else
			{
				if (iDigit >= 2 && iDigit <= 4 && !(num >= 10 && num <= 20))
					ret = string.Format(with_two, num);
				else
					ret = string.Format(other, num);
			}
		}

		return ret;
	}


	public static bool ToBool(this string value)
	{
		try
		{
			return value.Trim().ToLower() == "true";
		}
		catch (Exception e)
		{
			return false;
		}
	}

	public static string ToStringSafe(this object value)
	{
		try
		{
			return value.ToString();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return string.Empty;
		}
	}


	public static bool IsNullOrWhitespace(this string s)
	{
		return string.IsNullOrWhiteSpace(s);
	}

	public static string Merge(this string This, string newString)
	{
		if (!string.IsNullOrEmpty(newString)) return newString;
		return This;
	}

	public static bool Contains(this string[] stringArray, string value)
	{
		var pos = Array.IndexOf(stringArray, value);
		if (pos > -1)
			// the array contains the string and the pos variable
			// will have its position in the array
			return true;
		return false;
	}

	public static int Find(this string[] stringArray, string value)
	{
		var pos = Array.IndexOf(stringArray, value);
		return pos;
	}

	public static T ToEnum<T>(this string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	//public static T ToEnum<T>(this string value, T defaultValue)
	//{
	//    if (string.IsNullOrEmpty(value))
	//    {
	//        return defaultValue;
	//    }

	//    T result;
	//    return Enum.TryParse<T>(value, true, out result) ? result : defaultValue;
	//}

	public static double ToDouble(this string str)
	{
		var ret = 0.0;
		try
		{
			ret = double.Parse(str.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
		}

		return ret;
	}


	public static float ToFloat(this string str)
	{
		var ret = 0.0f;
		try
		{
			ret = float.Parse(str.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
		}

		return ret;
	}

	public static decimal ToDecimal(this string str)
	{
		var ret = 0m;
		try
		{
			ret = decimal.Parse(str.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
		}

		return ret;
	}

	public static bool IsNumber(this char str)
	{
		var ret = false;

		if (str >= '0' && str <= '9')
			ret = true;

		return ret;
	}

	public static bool IsNumber(this string str)
	{
		var ret = false;
		try
		{
			var number = int.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);
			ret = true;
		}
		catch
		{
		}

		return ret;
	}

	public static int ToInteger(this string str)
	{
		var ret = 0;
		try
		{
			ret = int.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		catch
		{
		}

		return ret;
	}

	public static long ToLong(this string str)
	{
		long ret = 0;
		long.TryParse(str, out ret);
		return ret;
	}

	public static string ApplyMaskFromEnd(this string input, string mask, char symbol = '#')
	{
		if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input))
			return input;
		var output = "";
		var inputPos = input.Length - 1;
		for (var pos = mask.Length - 1; pos > -1; pos--)
		{
			var masked = mask[pos];
			if (masked != symbol)
			{
				output += masked;
				continue;
			}

			var character = input[inputPos];
			output += character;
			inputPos--;
			if (inputPos < 0)
				break;
		}

		//mirror
		var charArray = output.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}


	//public static string ApplyMask(this string input, string mask, char symbol = '#')
	//{
	//    if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input))
	//        return input;
	//    var output = "";
	//    var inputPos = 0;
	//    for (int pos = 0; pos < mask.Length; pos++)
	//    {
	//        if (inputPos > input.Length - 1)
	//            break;

	//        var masked = mask[pos];
	//        if (masked != symbol)
	//        {
	//            output += masked;
	//            continue;
	//        }
	//        var character = input[inputPos];
	//        output += character;
	//        inputPos++;
	//    }

	//    //mirror
	//    char[] charArray = output.ToCharArray();
	//    //Array.Reverse(charArray);
	//    return new string(charArray);
	//}

	public static string ApplyMask(this string input, string mask, char symbol = '#', bool showNext = false,
		bool cut = false)
	{
		if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input))
			return input;
		var output = "";
		var inputPos = 0;
		for (var pos = 0; pos < mask.Length; pos++)
		{
			if (inputPos > input.Length - 1)
			{
				if (showNext)
					try
					{
						var next = mask[pos];
						if (next != symbol) output += next;
					}
					catch (Exception e)
					{
					}

				break;
			}

			var masked = mask[pos];
			if (masked != symbol)
			{
				output += masked;
				continue;
			}

			var character = input[inputPos];
			output += character;
			inputPos++;
		}

		if (!cut) //add what was left after mask
		{
			var add = input.Substring(inputPos - 1);
			output += add;
		}

		//mirror
		var charArray = output.ToCharArray();
		//Array.Reverse(charArray);
		return new string(charArray);
	}


	public static string ToTitleCase(this string str, string separator = " ", bool forceLower = false)
  {
		if (string.IsNullOrEmpty(str))
			return str;

		var splitter = separator.ToCharArray();
		var auxStr = str;
		if (forceLower)
			auxStr = str.ToLowerInvariant();
		var auxArr = auxStr.Split(splitter);
		var result = "";
		var firstWord = true;
		foreach (var word in auxArr)
		{
			if (string.IsNullOrEmpty(word))
			{
				result += separator;
				continue;
			}

			if (!firstWord)
				result += separator;
			else
				firstWord = false;

			result += word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1);
		}

		return result;
	}


	public static string ToPhraseCase(this string str, bool keepCase = false)
  {
		if (string.IsNullOrEmpty(str))
			return str;

		var auxStr = str.Trim().ToLower();
		var auxArr = auxStr.Split(' ');

		if (keepCase)
		{
			var result1 = str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1);
			return result1;
		}

		var result = str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1).ToLower();
		return result;
	}


	public static string AddAtNewLine(this string str, string add)
  {
		var ret = str;
		if (!string.IsNullOrEmpty(str)) ret += "\r\n";
		return ret += add;
	}


	public static string InsertAtNewLine(this string str, string add)
  {
		if (string.IsNullOrEmpty(add)) return str;

		var ret = str;
		if (!string.IsNullOrEmpty(str)) ret = "\r\n" + ret;
		return add + ret;
	}


	public static string OverrideWith(this string str, string overrideThis)
  {
		if (!string.IsNullOrEmpty(overrideThis))
			return overrideThis;
		return str;
	}


	public static string HtmlFirstWordUppercase(this string str, string separator = " ", bool forceLower = false)
  {
		if (string.IsNullOrEmpty(str))
			return str;

		var splitter = separator.ToCharArray();
		var auxStr = str.Trim(); //bug fixed
		if (forceLower)
			auxStr = str.ToLower();
		var auxArr = auxStr.Split(splitter);
		var result = "";
		var firstWord = true;
		foreach (var word in auxArr)
		{
			var outWord = word;
			if (!firstWord)
			{
				result += separator;
			}
			else
			{
				firstWord = false;
				outWord = $"<strong>{word.ToUpperInvariant()}</strong>";
			}

			result += outWord;
		}

		return result;
	}


	public static string FirstWordUppercase(this string str, string separator = " ", bool forceLower = false)
  {
		if (string.IsNullOrEmpty(str))
			return str;

		var splitter = separator.ToCharArray();
		var auxStr = str.Trim(); //bug fixed
		if (forceLower)
			auxStr = str.ToLower();
		var auxArr = auxStr.Split(splitter);
		var result = "";
		var firstWord = true;
		foreach (var word in auxArr)
		{
			var outWord = word;
			if (!firstWord)
			{
				result += separator;
			}
			else
			{
				firstWord = false;
				outWord = $"{word.ToUpperInvariant()}";
			}

			result += outWord;
		}

		return result;
	}


	public static string HtmlFirstWordStrong(this string str, string separator = " ", bool forceLower = false)
  {
		if (string.IsNullOrEmpty(str))
			return str;

		var splitter = separator.ToCharArray();
		var auxStr = str.Trim(); //bug fixed
		if (forceLower)
			auxStr = str.ToLower();
		var auxArr = auxStr.Split(splitter);
		var result = "";
		var firstWord = true;
		foreach (var word in auxArr)
		{
			var outWord = word;
			if (!firstWord)
			{
				result += separator;
			}
			else
			{
				firstWord = false;
				outWord = $"<strong>{word}</strong>";
			}

			result += outWord;
		}

		return result;
	}


	public static string SafeTrim(this string input)
  {
		if (string.IsNullOrEmpty(input)) return "";
		return input.Trim();
	}


	public static bool SafeContains(this string input, string value)
  {
		if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
			return false;
		return input.Contains(value);
	}


	public static bool SafeContainsInLower(this string input, string value)
  {
		if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
			return false;
		return input.ToLower().Contains(value.ToLower());
	}


	public static bool SafeHasTagInLower(this string input, string value)
  {
		if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
			return false;

		var tags = input.ToLower().TagsToList();

		return tags.Contains(value.ToLower());
	}


	public static bool HasContent(this string input)
  {
		if (string.IsNullOrEmpty(input)) return false;
		if (input.Length < 1) return false;
		return true;
	}


	public static bool HasContent(this object input)
  {
		if (input == null) return false;
		return true;
	}


	public static bool HasNoContent(this string input)
  {
		return !HasContent(input);
	}


	public static string SafeReplace(this string input, string a1, string a2)
  {
		if (string.IsNullOrEmpty(input)) return input;
		return input.Replace(a1, a2);
	}

	public static List<string> TagsToList(this string input, char divider)
  {
		var outList = new List<string>();
		if (input != null)
		{
			var list = input.Trim().Split(divider).ToList();
			foreach (var item in list)
			{
				var newItem = item.Trim();
				if (!string.IsNullOrEmpty(newItem))
					outList.Add(newItem);
			}

			return outList;
		}

		return outList;
	}


	public static List<int> TagsToIntList(this string input, string divider = ",")
  {
		var outList = new List<int>();
		if (input != null)
		{
			var list = input.Trim().Split(new[] { divider }, StringSplitOptions.None).ToList();
			//                var list = input.Trim().Split(divider).ToList();
			foreach (var item in list)
			{
				var newItem = item.Trim();
				if (!string.IsNullOrEmpty(newItem))
					outList.Add(newItem.ToInteger());
			}

			return outList;
		}

		return outList;
	}

	/// <summary>
	///     Process every line in list with its divider..
	/// </summary>
	/// <param name="inputList"></param>
	/// <param name="divider"></param>
	/// <returns></returns>
	public static List<string> TagsToList(this List<string> inputList, string divider = ",")
	{
		var ret = new List<string>();
		foreach (var input in inputList)
		{
			var add = input.TagsToList(divider);
			ret.AddRange(add);
		}

		return ret;
	}


	public static List<string> TagsToList(this string input, string divider = ",")
	{
		var outList = new List<string>();
		if (input != null)
		{
			var list = input.Trim().Split(new[] { divider }, StringSplitOptions.None).ToList();
			//                var list = input.Trim().Split(divider).ToList();
			foreach (var item in list)
			{
				var newItem = item.Trim();
				if (!string.IsNullOrEmpty(newItem))
					outList.Add(newItem);
			}

			return outList;
		}

		return outList;
	}


	public static string ReplaceOnce(this string source, string what, string with, int times = 1)
	{
		if (string.IsNullOrEmpty(source))
			return source;

		var regex = new Regex(Regex.Escape(what));
		var newText = regex.Replace(source, with, times);

		return newText;
	}

	public static string RemoveTag(this string input, string tag, string separator = ",")
  {
		if (string.IsNullOrEmpty(tag))
			return input;

		if (input == null)
			input = "";

		var tags = input.TagsToList(separator);
		while (tags.Contains(tag)) tags.Remove(tag);

		return tags.ToTags(separator);
	}

	public static string WithTag(this string input, string tag, string separator = ",")
  {
		if (string.IsNullOrEmpty(tag))
			return input;

		if (input == null)
			input = "";

		var tags = input.TagsToList(separator);

		if (!tags.Contains(tag))
			tags.Add(tag);

		return tags.ToTags(separator);
	}

	public static string WithTags(this string input, string tags, string separator = ",")
  {
		if (string.IsNullOrEmpty(tags))
			return input;

		if (input == null)
			input = "";

		var tagss = input.TagsToList(separator);

		foreach (var tag in tags.TagsToList(separator))
			if (!tagss.Contains(tag))
				tagss.Add(tag);


		return tagss.ToTags(separator);
	}

	public static string ToTags(this IEnumerable<string> input, string separator = ",")
  {
		if (input != null)
			return string.Join(separator, input);
		return "";
	}


	public static string Left(this string value, int maxLength = 1)
  {
		if (string.IsNullOrEmpty(value))
			return value;
		if (maxLength > value.Length)
			return value;
		return value.Substring(0, maxLength);
	}


	public static string Right(this string value, int maxLength)
  {
		if (string.IsNullOrEmpty(value))
			return
				value;
		if (maxLength > value.Length)
			return value;
		return value.Substring(value.Length - maxLength);
	}

	public static bool IsEmail(this string value)
  {
		if (value.Contains("@")) return true;
		return false;
	}
}