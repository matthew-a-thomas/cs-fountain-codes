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
		private readonly int _overhead;

		/// <summary>
		/// The number of symbols that are in the original data
		/// </summary>
		private readonly int _numSymbols;

		/// <summary>
		/// A record of all the encoding symbols that have been received so far
		/// </summary>
		private readonly Dictionary<bool[], Symbol<byte>> _collectedEncodingSymbols;

	    /// <summary>
	    /// Creates a new solver
	    /// </summary>
	    /// <param name="numSymbols">The number of symbols that are in the original data we're trying to solve for</param>
	    /// <param name="overhead">The number of encoding symbols to collect in addition to numSymbols</param>
	    public Receiver(int numSymbols, int overhead)
		{
			_overhead = overhead;
			_numSymbols = numSymbols;
			_collectedEncodingSymbols = new Dictionary<bool[], Symbol<byte>>();
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
			_collectedEncodingSymbols[coefficients] = value;
			complexity++;

			// See if we've collected enough to try to decode the collected symbols
		    if (_collectedEncodingSymbols.Count < _numSymbols + _overhead)
                return null;
		    // We'll be solving a system of equations, so set that up now
		    var equations = new bool[_collectedEncodingSymbols.Count][]; // All the equations
		    complexity += _collectedEncodingSymbols.Count;
		    var solutions = new Symbol<byte>[_collectedEncodingSymbols.Count]; // All the solutions to the equations
		    complexity += _collectedEncodingSymbols.Count;
		    var row = 0;
		    foreach (var equation in _collectedEncodingSymbols) // Each boolean array is like an equation
		    {
		        equations[row] = equation.Key;
		        solutions[row] = equation.Value;
		        row++;
		        complexity++;
		    }
		    // Try to solve the system of equations
		    var galois = GaussianEliminationGaloisField<byte>.Create(equations, ref complexity); // This returns null if the system of equations couldn't be solved. Otherwise, it prepares things to accept the solutions later. O(n^3)
		    complexity += 3;
		    // The system of equations can be solved
		    var solved = galois?.Solve(solutions, ref complexity); // Solve it!
		    return solved; // We got the original data back!
		}
	}
}
