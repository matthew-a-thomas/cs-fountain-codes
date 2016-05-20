using Library.GaussianElimination;
using Library.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
		Polynomial lastPolynomial;
		long lastSymbolID;
		int numSymbols;
		Polynomial primitivePolynomial;

		/// <summary>
		/// Returns the number of symbols that this carousel fountain code is set up to handle
		/// </summary>
		public int NumSymbols
		{
			get { return this.numSymbols; }
		}

		/// <summary>
		/// Generates a sophisticated carousel that will handle the given number of symbols
		/// </summary>
		/// <param name="numSymbols"></param>
		public SophisticatedCarousel(int numSymbols)
		{
			this.numSymbols = numSymbols;
			this.primitivePolynomial = PrecomputedPrimitivePolynomials.Get(numSymbols);

			// Set up the last polynomial and symbol ID so that the first symbol that gets generated (most likely with ID 0) can take the shortcut
			this.lastPolynomial = Polynomial.One; // Start out as the number one. Note this causes the first generated coefficient to encode the second symbol, skipping the first
			this.lastSymbolID = -1;
		}

		/// <summary>
		/// Generates the set of pointers for which symbols need to be XOR'd together to make the encoding symbol for the given symbol ID
		/// </summary>
		/// <param name="symbolID"></param>
		/// <param name="complexity"></param>
		/// <returns></returns>
		/// <remarks>This method takes a shortcut and is much faster when you generate successive symbol IDs</remarks>
		public bool[] GenerateCoefficients(long symbolID, ref int complexity)
		{
			if (symbolID != this.lastSymbolID + 1)
			{
				// We need to compute the last polynomial before we can multiply it by X
				this.lastPolynomial = Polynomial.One;
				for (var i = 0; i < symbolID - 1; i++)
				{
					this.lastPolynomial *= Polynomial.X;
				}
			}
			// Take the shortcut of just multiplying by the polynomial's generator element repeatedly to cycle through the multiplicative group of this finite field
			this.lastPolynomial *= Polynomial.X; complexity++;
			this.lastPolynomial %= this.primitivePolynomial; complexity += this.numSymbols;
			this.lastSymbolID = symbolID;

			// Pull out the coefficients from the generated polynomial
			var coefficients = new bool[this.NumSymbols];
			this.lastPolynomial.Coefficients.CopyTo(coefficients, 0); complexity += this.numSymbols; // Note the polynomial's coefficients will be condensed (no trailing falses) so we need to expand them
			return coefficients;
		}
	}
}
