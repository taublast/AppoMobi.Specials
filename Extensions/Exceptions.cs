using System;
using System.Collections.Generic;

namespace AppoMobi.Specials;

public static class ExtensionsExtensions

{
	public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
	{
		if (ex == null) throw new ArgumentNullException("ex");

		var innerException = ex;
		do
		{
			yield return innerException;
			innerException = innerException.InnerException;
		} while (innerException != null);
	}


	public static IEnumerable<string> GetInnerExceptionsMessages(this Exception ex)
  {
		if (ex == null) throw new ArgumentNullException("ex");

		var innerException = ex;
		do
		{
			yield return innerException.Message;
			innerException = innerException.InnerException;
		} while (innerException != null);
	}
}