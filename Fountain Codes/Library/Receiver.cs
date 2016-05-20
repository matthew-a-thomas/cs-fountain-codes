using Library.GaussianElimination;
using System.Collections.Generic;

namespace Library
{
	/// <summary>
	/// A class which collects enough encoding symbols (which are each the result of XOR'ing together particular symbols) to recover a message
	/// </summary>
	public class Receiver
	{
		/// <summary>
		/// The number of additional symbols to wait for after receiving numSymbols of them before attempting to decode
		/// </summary>
		int overhead;

		/// <summary>
		/// The number of symbols that are in the original data
		/// </summary>
		int numSymbols;

		/// <summary>
		/// A record of all the encoding symbols that have been received so far
		/// </summary>
		Dictionary<bool[], Symbol<byte>> collectedEncodingSymbols;

		/// <summary>
		/// The number of bytes in each symbols
		/// </summary>
		long symbolSize;

		/// <summary>
		/// Creates a new solver
		/// </summary>
		/// <param name="numSymbols">The number of symbols that are in the original data we're trying to solve for</param>
		/// <param name="symbolSize">The number of bytes in each symbol</param>
		/// <param name="overhead">The number of encoding symbols to collect in addition to numSymbols</param>
		public Receiver(int numSymbols, long symbolSize, int overhead)
		{
			this.overhead = overhead;
			this.numSymbols = numSymbols;
			this.collectedEncodingSymbols = new Dictionary<bool[], Symbol<byte>>();
			this.symbolSize = symbolSize;
		}

		/// <summary>
		/// Receives an encoding symbol, returning the decoded data if decoding was successful
		/// </summary>
		/// <param name="coefficients">The bits that are set indicate which symbols were XOR'd together to make this encoding symbol</param>
		/// <param name="value">The result of XOR'ing together symbols</param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public Symbol<byte>[] Solve(bool[] coefficients, Symbol<byte> value, ref int complexity) // O(n^3)
		{
			// Collect this encoding symbol
			this.collectedEncodingSymbols[coefficients] = value;
			complexity++;

			// See if we've collected enough to try to decode the collected symbols
			if (this.collectedEncodingSymbols.Count >= this.numSymbols + this.overhead)
			{
				// We'll be solving a system of equations, so set that up now
				var equations = new bool[this.collectedEncodingSymbols.Count][]; // All the equations
				complexity += this.collectedEncodingSymbols.Count;
				var solutions = new Symbol<byte>[this.collectedEncodingSymbols.Count]; // All the solutions to the equations
				complexity += this.collectedEncodingSymbols.Count;
				var row = 0;
				foreach (var equation in this.collectedEncodingSymbols) // Each boolean array is like an equation
				{
					equations[row] = equation.Key;
					solutions[row] = equation.Value;
					row++;
					complexity++;
				}
				// Try to solve the system of equations
				var galois = GaussianEliminationGaloisField<byte>.Create(equations, ref complexity); // This returns null if the system of equations couldn't be solved. Otherwise, it prepares things to accept the solutions later. O(n^3)
				complexity += 3;
				if (galois != null)
				{
					// The system of equations can be solved
					var solved = galois.Solve(solutions, ref complexity); // Solve it!
					return solved; // We got the original data back!
				}
			}
			return null;
		}
	}
}
