using System;
using System.Collections.Generic;
using System.Linq;

namespace AppoMobi.Specials.Extensions;

public static class RndExtensions

{
	private static int lastRandomChecksum = -1;

	private static Random Rnd = new((int)DateTime.Now.Ticks);

	public static double NextDouble(this Random rnd, double from, double to, double step)
  {
		var delta = to - from;
		var nbOfSteps = (int)(delta / step);
		var randomStep = rnd.Next(0, nbOfSteps);
		return step * randomStep + from; //will be really double if step was fractional
	}

	public static double Next(this Random rnd, int from, int to, int step)
  {
		var delta = to - from;
		var nbOfSteps = delta / step;
		var randomStep = rnd.Next(0, nbOfSteps);
		return step * randomStep + from;
	}

	public static void Randomize(int seed)
	{
		Rnd = new Random(seed);
		lastRandomChecksum = -1;
	}

	public static void RandomizeTime()
	{
		var seed = (int)DateTime.Now.Ticks;
		Rnd = new Random(seed);
		lastRandomChecksum = -1;
	}

	public static bool CreateRandomBoolean()
	{
		return CreateRandom(0, 1) == 1;
	}

	/// <summary>
	///     Generate rnadom, try not to repeat. Use checksum
	/// </summary>
	/// <param name="from"></param>
	/// <param name="max"></param>
	/// <param name="lastRandomChecksum">from + max + lastRandom</param>
	/// <returns></returns>
	public static int CreateRandom(int from, int max, int? doNotRepeatChecksum = null)
	{
		var index = Rnd.Next(from, max + 1);

		//Debug.WriteLine($"[RND] {index}");

		if (max - from > 0)
		{
			if (doNotRepeatChecksum != null)
				while (from + max + index == doNotRepeatChecksum.Value)
					index = Rnd.Next(0, max + 1);
			else
				while (from + max + index == lastRandomChecksum)
					index = Rnd.Next(0, max + 1);
		}

		//Debug.WriteLine($"[RND] final {index}");

		lastRandomChecksum = from + max + index;

		return index;
	}

	public static IQueryable<T> OneRandom<T>(this IQueryable<T> list, int? doNotRepeatChecksum = null)
	{
		var max = list.Count();

		if (max < 1)
			return Enumerable.Empty<T>().AsQueryable();

		if (max == 1)
			return list;

		var index = CreateRandom(0, max - 1, doNotRepeatChecksum);

		return list.Skip(index).Take(1);
	}

	public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> list)
	{
		return list.OrderBy(r => Rnd.Next());
	}

	public static IOrderedEnumerable<T> Shuffled<T>(this ICollection<T> list)
	{
		return list.OrderBy(r => Rnd.Next());
	}

	public static IQueryable<T> Shuffled<T>(this IQueryable<T> list)
	{
		return list.OrderBy(r => Rnd.Next());
	}
}