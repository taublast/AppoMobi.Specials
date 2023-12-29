using System.Collections.Generic;

namespace AppoMobi.Specials;

public static class UsefulExtensions
{
	/// <summary>
	///     Sweep over text
	/// </summary>
	/// <param name="Text"></param>
	/// <returns></returns>
	public static IEnumerable<string> WordList(this string Text)
	{
		var cIndex = 0;
		int nIndex;
		while ((nIndex = Text.IndexOf(' ', cIndex + 1)) != -1)
		{
			var sIndex = cIndex == 0 ? 0 : cIndex + 1;
			yield return Text.Substring(sIndex, nIndex - sIndex);
			cIndex = nIndex;
		}

		yield return Text.Substring(cIndex + 1);
	}

	//foreach (string word in "incidentno and fintype or unitno".WordList())
	//System.Console.WriteLine("'" + word + "'");

	public static string CutText(this string text, int count = 200)
  {
		if (text != null && text.Length > count)
			return text.Remove(count) + "...";
		return text;
	}
}