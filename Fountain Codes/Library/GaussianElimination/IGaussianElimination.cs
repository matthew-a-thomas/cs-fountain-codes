
namespace Library.GaussianElimination
{
	/// <summary>
	/// Defines something that does Gaussian Elimination to solve a system of equations
	/// </summary>
	internal interface IGaussianElimination<T> where T : struct
	{
		/// <summary>
		/// Performs the sequence of steps in reverse on the given input
		/// </summary>
		/// <param name="variables"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		Symbol<T>[] Generate(Symbol<T>[] variables, ref int complexity);

		/// <summary>
		/// Follows the precomputed steps to give the values of the variables. Returns nulls where symbols can't be solved
		/// </summary>
		/// <param name="solutions"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		Symbol<T>[] Solve(Symbol<T>[] solutions, ref int complexity);
	}
}
