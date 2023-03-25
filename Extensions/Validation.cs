using System;
using System.Text.RegularExpressions;

namespace AppoMobi.Specials.Extensions;

public static class ValidationExtensions
{
	public static bool IsDigitsOnly(string str)
	{
		foreach (var c in str)
			if (c < '0' || c > '9')
				return false;
		return true;
	}

	public static string GenerateSnils()
	{
		var ok = false;
		var snils = "";

		while (!ok)
			try
			{
				var clean = OnlyDigits(Guid.NewGuid().ToString()).Left(9);
				//var maybe = new Random((int)(DateTime.Now.Ticks));
				//var clean = maybe.Next(100000000, 999999999).ToString();
				var totalSum = 0;
				for (int i = clean.Length - 1, j = 0; i >= 0; i--, j++)
				{
					var digit = int.Parse(clean[i].ToString());
					totalSum += digit * (j + 1);
				}

				var cleanSum = SNILSCheckControlSum(totalSum);
				snils = $"{clean}{cleanSum}";
				ok = ValidateSNILS(snils);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		return snils;
	}

	public static bool ValidateSNILS(string snils)
	{
		var workSnils = snils.Trim();

		if (!IsDigitsOnly(snils))
			return false;

		var result = false;

		if (workSnils.Length == 9)
		{
			if (SNILSContolCalc(workSnils) > -1) result = true;
		}
		else if (workSnils.Length == 11)
		{
			var controlSum = SNILSContolCalc(workSnils);
			var strControlSum = int.Parse(workSnils.Substring(9, 2));
			if (controlSum == strControlSum) result = true;
		}
		else
		{
			return false;
			//throw new Exception(String.Format("Incorrect SNILS number. {0} digits! (it can only be 9 or 11 digits!)", workSnils.Length));
		}

		return result;
	}

	public static string OnlyDigits(string subjectString)
	{
		string resultString = null;
		try
		{
			var regexObj = new Regex(@"[^\d]");
			resultString = regexObj.Replace(subjectString, "");
		}
		catch (ArgumentException ex)
		{
			// Syntax error in the regular expression
		}

		return resultString;
	}

	public static int SNILSContolCalc(string snils)
	{
		var workSnils = OnlyDigits(snils);

		if (workSnils.Length != 9 && workSnils.Length != 11)
			throw new Exception(string.Format("Incorrect SNILS number. {0} digits! (it can only be 9 or 11 digits!)",
				workSnils.Length));

		if (workSnils.Length == 11) workSnils = workSnils.Substring(0, 9);

		var totalSum = 0;
		for (int i = workSnils.Length - 1, j = 0; i >= 0; i--, j++)
		{
			var digit = int.Parse(workSnils[i].ToString());
			totalSum += digit * (j + 1);
		}

		return SNILSCheckControlSum(totalSum);
	}

	private static int SNILSCheckControlSum(int _controlSum)
	{
		int result;
		if (_controlSum < 100)
		{
			result = _controlSum;
		}
		else if (_controlSum <= 101)
		{
			result = 0;
		}
		else
		{
			var balance = _controlSum % 101;
			result = SNILSCheckControlSum(balance);
		}

		return result;
	}
}