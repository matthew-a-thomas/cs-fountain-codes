using Library.GaussianElimination;

namespace Library.FountainCodeImplementations
{
	/// <summary>
	/// A fountain code which cycles through all the elements of the multiplicative group of a finite field to determine which symbols get XOR'd together for encoding symbols
	/// </summary>
	public class SophisticatedCarousel : IFountainCodeImplementation
	{
		/// <summary>
		/// The polynomial that represents the coefficients of the last symbol that was generated
		/// </summary>
		private Polynomial _lastPolynomial;

	    private long _lastSymbolId;
	    private readonly int _numSymbols;
	    private readonly Polynomial _primitivePolynomial;

		/// <summary>
		/// Returns the number of symbols that this carousel fountain code is set up to handle
		/// </summary>
		public int NumSymbols => _numSymbols;

	    /// <summary>
		/// Generates a sophisticated carousel that will handle the given number of symbols
		/// </summary>
		/// <param name="numSymbols"></param>
		public SophisticatedCarousel(int numSymbols)
		{
			_numSymbols = numSymbols;
			_primitivePolynomial = PrecomputedPrimitivePolynomials.Get(numSymbols);

			// Set up the last polynomial and symbol ID so that the first symbol that gets generated (most likely with ID 0) can take the shortcut
			_lastPolynomial = Polynomial.One; // Start out as the number one. Note this causes the first generated coefficient to encode the second symbol, skipping the first
			_lastSymbolId = -1;
		}

		/// <summary>
		/// Generates the set of pointers for which symbols need to be XOR'd together to make the encoding symbol for the given symbol ID
		/// </summary>
		/// <param name="symbolId"></param>
		/// <param name="complexity"></param>
		/// <returns></returns>
		/// <remarks>This method takes a shortcut and is much faster when you generate successive symbol IDs</remarks>
		public bool[] GenerateCoefficients(long symbolId, ref int complexity)
		{
			if (symbolId != _lastSymbolId + 1)
			{
				// We need to compute the last polynomial before we can multiply it by X
				_lastPolynomial = Polynomial.One;
				for (var i = 0; i < symbolId - 1; i++)
				{
					_lastPolynomial *= Polynomial.X;
				}
			}
			// Take the shortcut of just multiplying by the polynomial's generator element repeatedly to cycle through the multiplicative group of this finite field
			_lastPolynomial *= Polynomial.X; complexity++;
			_lastPolynomial %= _primitivePolynomial; complexity += _numSymbols;
			_lastSymbolId = symbolId;

			// Pull out the coefficients from the generated polynomial
			var coefficients = new bool[NumSymbols];
			_lastPolynomial.Coefficients.CopyTo(coefficients, 0); complexity += _numSymbols; // Note the polynomial's coefficients will be condensed (no trailing falses) so we need to expand them
			return coefficients;
		}
	}
}
