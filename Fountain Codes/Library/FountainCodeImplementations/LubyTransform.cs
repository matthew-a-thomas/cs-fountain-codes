using Library.DiscreteDistributions;
using Library.Randomness;
using System;

namespace Library.FountainCodeImplementations
{
	/// <summary>
	/// An implementation of Luby Transform. To better understand the underlying principals, it's recommended to read Michael Luby's "LT Codes" paper given in the Proceedings of the 43 rd Annual IEEE Symposium on Foundations of Computer Science
	/// </summary>
	public class LubyTransform : IFountainCodeImplementation
	{
		/// <summary>
		/// The number of original data symbols that this implementation is good for
		/// </summary>
		public int NumSymbols { get; private set; }
		
		/// <summary>
		/// The probability distribution to use to pick the degree of each encoding symbol
		/// </summary>
		private readonly DiscretePmf _pmf;

		/// <summary>
		/// The source of randomness to use
		/// </summary>
		private readonly Random _random;

		/// <summary>
		/// Creates a new Luby Transform implementation with the given parameters
		/// </summary>
		/// <param name="random">A source of randomness. Make sure this source of randomness is thread-safe</param>
		/// <param name="numSymbols">The number of original data symbols that this implementation will deal with</param>
		/// <param name="r">The expected "ripple" size (see Michael Luby's "LT Codes" paper). Basically this increases the probability that numSymbols/r bits are set in the generated coefficients. 2 seems to work well</param>
		/// <param name="delta">The probability that a random walk of length k deviates from its mean by more than ln(k/delta)*sqrt(k) is at most this number. In other words, a smaller delta makes the underlying Robust Soliton Distribution more robust, so a smaller number seems to be better</param>
		public LubyTransform(Random random, int numSymbols, int r, double delta)
		{
			NumSymbols = numSymbols;
			_pmf = new DiscretePmf(random, RobustSolitonDistribution.GenerateCdf(numSymbols, r, delta));
			_random = random;
		}

		/// <summary>
		/// Generates a random set of coefficients. The number of set coefficients is a number drawn from the Robust Soliton Distribution that was initialized in the constructor
		/// </summary>
		/// <param name="symbolId">For Luby Transform, the symbol ID has no bearing on the coefficients</param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public bool[] GenerateCoefficients(long symbolId, ref int complexity)
		{
			// Set up the set of coefficients
			var coefficients = new bool[NumSymbols]; complexity += NumSymbols;
			var degree = _pmf.Generate(); // This is the number of bits that will be set in the coefficients array. O(log(n))
			complexity += (int)Math.Ceiling(Math.Log(NumSymbols, 2.0));
			for (var i = 0; i < degree; i++) // O(n)
			{
				complexity++;
				coefficients[i] = true;
			}
			// Mix up the coefficients array
			Permutator<bool>.Permutate(coefficients, _random); // O(n)
			complexity += NumSymbols;

			return coefficients;
		}
	}
}
