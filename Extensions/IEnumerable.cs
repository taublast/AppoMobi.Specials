using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AppoMobi.Specials;

public static class IEnumerableExtensions
{
	public static int FindIndex(this IEnumerable collection, object value)
	{
		if (collection is IList list) return list.IndexOf(value);
		var searchIndex = 0;
		foreach (var item in collection)
		{
			if (item == value) return searchIndex;
			++searchIndex;
		}

		return -1;
	}

	public static object FindValue(this IEnumerable collection, int index)
	{
		if (collection is IList list) return list[index];
		var searchIndex = 0;
		foreach (var item in collection)
		{
			if (searchIndex == index) return item;
			++searchIndex;
		}

		throw new IndexOutOfRangeException();
	}

	public static int Count(this IEnumerable collection)
	{
		if (collection is ICollection list) return list.Count;
		var searchIndex = 0;
		foreach (var item in collection) ++searchIndex;
		return searchIndex;
	}

	public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
	{
		if (string.IsNullOrWhiteSpace(csvList))
			return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

		return csvList
			.TrimEnd(',')
			.Split(',')
			.AsEnumerable()
			.Select(s => s.Trim())
			.ToList();
	}

	public static Func<T, T> DynamicSelectGenerator<T>()
	{
		// get Properties of the T
		var fields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray();

		// input parameter "o"
		var xParameter = Expression.Parameter(typeof(T), "o");

		// new statement "new Data()"
		var xNew = Expression.New(typeof(T));

		// create initializers
		var bindings = fields.Select(o => o.Trim())
			.Select(o =>
				{
					// property "Field1"
					var mi = typeof(T).GetProperty(o);

					// original value "o.Field1"
					var xOriginal = Expression.Property(xParameter, mi);

					// set value "Field1 = o.Field1"
					return Expression.Bind(mi, xOriginal);
				}
			);

		// initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
		var xInit = Expression.MemberInit(xNew, bindings);

		// expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
		var lambda = Expression.Lambda<Func<T, T>>(xInit, xParameter);

		// compile to Func<Data, Data>
		return lambda.Compile();
	}


	public static object GetItem(this IEnumerable e, int index)
	{
		var enumerator = e.GetEnumerator();
		var i = 0;
		while (enumerator.MoveNext())
		{
			if (i == index)
				return enumerator.Current;
			i++;
		}

		return null;
	}

	public static int GetCount(this IEnumerable e)
	{
		var enumerator = e.GetEnumerator();
		var i = 0;
		while (enumerator.MoveNext()) i++;
		return i;
	}

	public static List<object> GetList(this IEnumerable e)
	{
		var enumerator = e.GetEnumerator();
		var list = new List<object>();
		while (enumerator.MoveNext()) list.Add(enumerator.Current);
		return list;
	}


	public static IQueryable<V> SelectByName<T, V>(this IQueryable<T> source, string FieldName)
  {
		var paramExp = Expression.Parameter(typeof(T), "x");
		var memberExp = Expression.PropertyOrField(paramExp, FieldName);
		var lambdaExp = Expression.Lambda<Func<T, V>>(memberExp, paramExp);

		return source.Select(lambdaExp);
	}


	public static async Task<bool> IsDifferent<T>(this IEnumerable<T> source, IEnumerable<T> otherList)
  {
		var index = -1;
		var different = false;
		foreach (var element in source)
		{
			index++;
			try
			{
				var string1 = await Task.Run(() => JsonConvert.SerializeObject(element));
				var string2 = await Task.Run(() => JsonConvert.SerializeObject(otherList.ToArray()[index]));

				if (string1 != string2)
				{
					different = true;
					break;
				}
			}
			catch (Exception e)
			{
				different = true;
				break;
			}
		}

		return different;
	}


	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, string propSelector)
  {
		// Building expression x=> x.FieldName
		var foo = Expression.Parameter(typeof(TSource), "x");
		var selection = Expression.PropertyOrField(foo, propSelector);

		var lambdaExp = Expression.Lambda<Func<TSource, TKey>>(selection, foo);

		return source.DistinctBy(lambdaExp.Compile(), null);
	}

	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector)
  {
		return source.DistinctBy(keySelector, null);
	}


	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

		return _();

		IEnumerable<TSource> _()
		{
			var knownKeys = new HashSet<TKey>(comparer);
			foreach (var element in source)
				if (knownKeys.Add(keySelector(element)))
					yield return element;
		}
	}

	public static IQueryable<TSource> DistinctBy<TSource, TKey>(this IQueryable<TSource> source,
		Expression<Func<TSource, TKey>> keySelector)
	{
		return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
	}


	public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
	{
		return source.Select((item, index) => (item, index));
	}
}