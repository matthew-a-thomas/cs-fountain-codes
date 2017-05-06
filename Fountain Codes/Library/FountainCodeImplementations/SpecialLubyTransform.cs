using Library.Randomness;
using System;

namespace Library.FountainCodeImplementations
{
	/// <summary>
	/// Behaves like the Luby Transform code with delta set to zero. In other words, there's a 100% chance that the chosen degree will be (numSymbols / r)
	/// </summary>
	public class SpecialLubyTransform : IFountainCodeImplementation
	{
		/// <summary>
		/// Luby's expected "ripple" size
		/// </summary>
		private readonly int _r;

		/// <summary>
		/// The source of randomness to use
		/// </summary>
		private readonly Random _random;

		/// <summary>
		/// The number of symbols that this implementation can handle
		/// </summary>
		public int NumSymbols { get; private set; }

		/// <summary>
		/// Sets up a special case of Luby Transform which acts as though delta is zero. In other words, there's a 100% chance that the generated degree will be (numSymbols / r)
		/// </summary>
		/// <param name="random"></param>
		/// <param name="numSymbols"></param>
		/// <param name="r"></param>
		public SpecialLubyTransform(Random random, int numSymbols, int r)
		{
			NumSymbols = numSymbols;
			_r = r;
			_random = random;
		}

		/// <summary>
		/// Generates an array of coeffients in which (the smallest odd number at least as large as (NumSymbols / r)) of them are set
		/// </summary>
		/// <param name="symbolId">The symbol ID has no bearing on how coefficients are generated in this implementation</param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public bool[] GenerateCoefficients(long symbolId, ref int complexity)
		{
			// Pick the degree, which is the number of bits set in the coefficients array
			var degree = NumSymbols / _r;
			if (degree % 2 == 0)
				degree++; // It's impossible to solve a system of equations if you've only got even numbers of coefficients set in your systems of equations (http://math.stackexchange.com/a/1751691/284627), so let's make the number odd
			complexity++;

			// Set up the coefficients
			var coefficients = new bool[NumSymbols]; complexity += NumSymbols;
			for (var i = 0; i < degree; i++) // Make sure that (degree) of them are set
			{
				complexity++;
				coefficients[i] = true;
			}
			Permutator<bool>.Permutate(coefficients, _random); // Mix them up randomly
			complexity += NumSymbols;

			return coefficients;
		}
	}
}
