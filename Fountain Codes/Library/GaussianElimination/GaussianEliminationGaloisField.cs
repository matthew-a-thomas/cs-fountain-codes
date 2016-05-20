using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.GaussianElimination
{
	/// <summary>
	/// A class which does Gaussian Elimination to solve a system of equations. The coefficients are in GF(2) and the solutions are in GF(8N), so only the swap and XOR operations are used. Inspired by https://en.wikipedia.org/wiki/Gaussian_elimination
	/// </summary>
	public class GaussianEliminationGaloisField<T> : IGaussianElimination<T> where T : struct
	{
		/// <summary>
		/// An operation that can be performed between two bytes
		/// </summary>
		enum Operation
		{
			Swap,
			XOR
		}

		/// <summary>
		/// Something which performs an operation from an index to another index
		/// </summary>
		class Step
		{
			public Operation Operation { get; private set; }
			public long FromIndex { get; private set; }
			public long ToIndex { get; private set; }

			public Step(Operation operation, long fromIndex, long toIndex)
			{
				this.Operation = operation;
				this.FromIndex = fromIndex;
				this.ToIndex = toIndex;
			}
		}

		/// <summary>
		/// The number of columns in the coefficient matrix
		/// </summary>
		long numColumns;

		/// <summary>
		/// The number of rows in the coefficient matrix
		/// </summary>
		long numRows;

		/// <summary>
		/// The sequence of steps to perform to solve the system of equations
		/// </summary>
		LinkedList<Step> steps;

		private GaussianEliminationGaloisField(long numColumns, long numRows)
		{
			this.numColumns = numColumns;
			this.numRows = numRows;
			this.steps = new LinkedList<Step>();
		}

		/// <summary>
		/// Creates a new solver object by calculating the steps required to solve for the given coefficients.
		/// 
		/// If the given coefficients can't be solved for then null is returned.
		/// 
		/// This function has time complexity O(n^2) where n is the number of rows
		/// </summary>
		/// <param name="coefficients"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public static GaussianEliminationGaloisField<T> Create(bool[][] coefficients, ref int complexity)
		{
			var numRows = GaussianEliminationGaloisField<T>.NumRows(coefficients); complexity++;
			var numColumns = GaussianEliminationGaloisField<T>.NumColumns(coefficients); complexity += 2;

			{ // Create a copy of the coefficients, ensuring that all the coefficients have the same number of elements
				var newCoefficients = new bool[numRows][]; complexity += (int)numRows;
				for (var i = 0; i < numRows; i++)
				{
					complexity++;
					var source = coefficients[i];

					// Make sure all the coefficients have the same number of elements
					if (source.Length != numColumns)
						throw new Exception("Not all the coefficients have the same number of elements");

					// Copy this coefficient
					var destination = (newCoefficients[i] = new bool[numColumns]); complexity += (int)numColumns;
					for (var j = 0; j < numColumns; j++)
					{
						complexity++;
						destination[j] = source[j];
					}
				}
				coefficients = newCoefficients;
			}

			// Set up a new solver object
			var solver = new GaussianEliminationGaloisField<T>(numColumns, numRows); complexity += 3;

			// Put matrix into row echelon form
			for (long k = 0; k < Math.Min(numRows, numColumns); k++) // O(n^2)
			{
				complexity++;
				// Find the pivot point
				long i_max = 0;
				for (long i = k; i < numRows; i++) // O(n)
				{
					complexity++;
					if (coefficients[i][k])
					{
						i_max = i;
						break;
					}
				}
				if (!coefficients[i_max][k])
					return null;
				// Swap rows k and i_max
				if (i_max != k)
				{
					solver.steps.AddLast(GaussianEliminationGaloisField<T>.SwapRows(coefficients, i_max, k));
					complexity += (int)numColumns;
				}
				// XOR pivot with all rows below the pivot
				for (var i = k + 1; i < numRows; i++) // O(n)
				{
					complexity++;
					if (coefficients[i][k])
					{
						solver.steps.AddLast(GaussianEliminationGaloisField<T>.XORRows(coefficients, k, i)); // We can just XOR since we're dealing with Galois Fields
						complexity += (int)numColumns;
					}
				}
			}

			// Put the matrix into reduced row echelon form using back substitution
			for (long k = Math.Min(numRows, numColumns) - 1; k > 0; k--) // O(n^2)
			{
				complexity++;
				if (!coefficients[k][k])
					return null;
				// See which other rows need to be XOR'd with this one
				for (long i = k - 1; i >= 0; i--) // O(n)
				{
					complexity++;
					if (coefficients[i][k])
					{
						solver.steps.AddLast(GaussianEliminationGaloisField<T>.XORRows(coefficients, k, i));
						complexity += (int)numColumns;
					}
				}
			}

			// Make sure the top part of the coefficients matrix is the identity matrix and that the bottom part is only zeros
			for (long row = 0; row < numRows; row++) // O(n^2)
			{
				complexity++;
				for (long column = 0; column < numColumns; column++) // O(n)
				{
					complexity++;
					if ((row == column) ^ (coefficients[row][column])) // There should be a coefficient and there's not, or there shouldn't be and there is
						return null;
				}
			}

			return solver;
		}

		/// <summary>
		/// Performs the sequence of steps in reverse on the given input.
		/// </summary>
		/// <param name="variables"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public Symbol<T>[] Generate(Symbol<T>[] variables, ref int complexity)
		{
			if (variables.LongLength != this.numColumns)
				throw new Exception("There isn't the right number of variables given. There should be the same number as there are columns in the coefficients matrix (" + this.numColumns + "), but " + variables.LongLength + " variables were given");
			long symbolSize = Symbol<T>.GetUniformSize(variables);
			var expanded = new Symbol<T>[this.numRows];
			variables.CopyTo(expanded, 0);
			for (long i = variables.LongLength; i < expanded.LongLength; i++)
			{
				expanded[i] = new Symbol<T>(symbolSize);
			}
			return GaussianEliminationGaloisField<T>.SolveInner(this.steps.Reverse(), expanded, ref complexity);
		}

		/// <summary>
		/// Returns the number of columns in the given array
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		static long NumColumns(bool[][] array)
		{
			return array[0].LongLength;
		}

		/// <summary>
		/// Returns the number of rows in the given array
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		static long NumRows(bool[][] array)
		{
			return array.LongLength;
		}

		/// <summary>
		/// Follows the precomputed steps to give the values of the variables
		/// </summary>
		/// <param name="solutions"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public Symbol<T>[] Solve(Symbol<T>[] solutions, ref int complexity)
		{
			if (solutions.LongLength != this.numRows)
				throw new Exception("There isn't the right number of solutions. There should be as many solutions as there are rows in the coefficients matrix (" + this.numRows + "), but there are " + solutions.LongLength + " solutions given");
			Symbol<T>.GetUniformSize(solutions); complexity += solutions.Length;
			var solved = GaussianEliminationGaloisField<T>.SolveInner(this.steps, solutions, ref complexity);
			var trimmed = new Symbol<T>[this.numColumns]; complexity += (int)this.numColumns;
			for (long i = 0; i < trimmed.LongLength; i++)
			{
				complexity++;
				trimmed[i] = solved[i];
			}
			return trimmed;
		}

		/// <summary>
		/// Performs the given sequence of steps against the given input
		/// </summary>
		/// <param name="steps"></param>
		/// <param name="input"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		private static Symbol<T>[] SolveInner(IEnumerable<Step> steps, Symbol<T>[] input, ref int complexity)
		{
			input = input.Clone() as Symbol<T>[]; complexity += input.Length;
			foreach (var step in steps)
			{
				complexity++;
				switch (step.Operation)
				{
					case Operation.Swap:
						// Swap the two elements
						var first = input[step.FromIndex];
						input[step.FromIndex] = input[step.ToIndex];
						input[step.ToIndex] = first;
						break;
					case Operation.XOR:
						// XOR one with another
						input[step.ToIndex] ^= input[step.FromIndex];
						break;
					default:
						throw new Exception("This step has an unknown operation: " + step.Operation.ToString());
				}
			}
			return input;
		}

		/// <summary>
		/// Swaps two rows in the given two-dimensional array
		/// </summary>
		/// <param name="equations"></param>
		/// <param name="fromRow"></param>
		/// <param name="toRow"></param>
		/// <returns></returns>
		static Step SwapRows(bool[][] equations, long fromRow, long toRow)
		{
			if (fromRow != toRow)
			{
				for (var column = GaussianEliminationGaloisField<T>.NumColumns(equations) - 1; column >= 0; column--)
				{
					var from = equations[fromRow][column];
					var to = equations[toRow][column];
					equations[fromRow][column] = to;
					equations[toRow][column] = from;
				}
				return new Step(Operation.Swap, fromRow, toRow);
			}
			else
			{
				throw new Exception("Are you sure you want to swap row " + fromRow + " with itself?");
			}
		}

		/// <summary>
		/// XORs one row with another
		/// </summary>
		/// <param name="equations"></param>
		/// <param name="fromRow"></param>
		/// <param name="toRow"></param>
		/// <returns></returns>
		static Step XORRows(bool[][] equations, long fromRow, long toRow)
		{
			if (fromRow != toRow)
			{
				for (var column = GaussianEliminationGaloisField<T>.NumColumns(equations) - 1; column >= 0; column--)
				{
					equations[toRow][column] ^= equations[fromRow][column];
				}
				return new Step(Operation.XOR, fromRow, toRow);
			}
			else
			{
				throw new Exception("Are you sure you want to XOR row " + fromRow + " with itself?");
			}
		}
	}
}
