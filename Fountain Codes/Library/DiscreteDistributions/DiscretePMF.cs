using System;

namespace Library.DiscreteDistributions
{
	/// <summary>
	/// Implements a probability mass function based on any discrete cumulative distribution function. https://en.wikipedia.org/wiki/Probability_mass_function
	/// </summary>
	public class DiscretePmf
	{
	    private readonly double[] _cdf;
	    private readonly Random _random;

		/// <summary>
		/// Gets the number of elements in the CDF that was provided to the constructor
		/// </summary>
		public int Max => _cdf.Length;

	    /// <summary>
		/// Creates a new probability mass function from the given cumulative distribution function and uniform random number generator. https://en.wikipedia.org/wiki/Probability_mass_function
		/// </summary>
		/// <param name="random"></param>
		/// <param name="cdf">The values in this array must be sorted from least to greatest, and the last element must equal 1. https://en.wikipedia.org/wiki/Cumulative_distribution_function</param>
		public DiscretePmf(Random random, double[] cdf)
		{
			// Sanity checks
			if (cdf == null || cdf.Length == 0)
				throw new Exception("Please provide a cumulative distribution function that contains at least one number");
		    // ReSharper disable once CompareOfFloatsByEqualityOperator
			if (cdf[cdf.Length - 1] != 1.0)
				throw new Exception("Please provide a cumulative distribution function that has the number 1 as its last element");
			for (var i = 1; i < cdf.Length; i++)
			{
				if (cdf[i] <= cdf[i - 1])
					throw new Exception("Please provide a cumulative distribution function that has its values sorted from least to greatest");
			}

			// Store values
			_cdf = cdf;
			_random = random;
		}

		/// <summary>
		/// Generates a random point from this probability mass function
		/// </summary>
		/// <returns>An integer between 0 (inclusive) and cdf.Length (exclusive)</returns>
		public int Generate()
		{
			var rand = _random.NextDouble(); // A uniform random variable
			var searchResult = Array.BinarySearch(_cdf, rand); // Search for the smallest index of the cdf that has a value at least as large as the random number we picked
			var index = searchResult < 0 ? ~searchResult : searchResult; // BinarySearch returns a negative number of the next largest index if no exact match is found

			return index;
		}

		/// <summary>
		/// Returns a horizontal bar graph of the given width showing average output of this probability mass function.
		/// </summary>
		/// <param name="resolution">The number of numbers to draw from the random number generator that was given to the constructor. Using at least 10e5 gives only a small amount of variance between calls to this function</param>
		/// <param name="width">Maximum width in characters for each line in the returned array</param>
		/// <returns>An array of lines. Each line is a string of * characters, the number of which shows the relative weight of that index</returns>
		public string[] Visualize(int resolution, int width)
		{
			// Generate lots of numbers from this PMF
			var numRuns = 10e5;
			var values = new int[_cdf.Length];
			for (var i = 0; i < numRuns; i++)
			{
				var index = Generate();
				values[index]++;
			}

			// Now generate each line
			var result = new string[values.Length];
			for (var i = 0; i < values.Length; i++)
			{
				result[i] = new string('*', (int)Math.Ceiling(values[i] * (double)(width) / numRuns));
			}
			return result;
		}
	}
}
