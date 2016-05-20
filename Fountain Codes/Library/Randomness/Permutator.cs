using System;

namespace Library.Randomness
{
	/// <summary>
	/// Helps with permutating things
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class Permutator<T>
	{
		/// <summary>
		/// Permutates the given items in place so that an element has equal probability of ending up anywhere in the array
		/// </summary>
		/// <param name="items"></param>
		/// <param name="random">The random number generator to use for the permutation</param>
		/// <remarks>Follows the Fisher-Yates shuffle https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle, so this has time complexity O(n)</remarks>
		public static void Permutate(T[] items, Random random)
		{
			for (var i = items.Length - 1; i >= 1; i--)
			{
				var swapIndex = random.Next(i + 1); // Beware of https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#Implementation_errors
				if (i != swapIndex)
				{
					var temp = items[i];
					items[i] = items[swapIndex];
					items[swapIndex] = temp;
				}
			}
		}
	}
}
