
namespace Library.FountainCodeImplementations
{
	/// <summary>
	/// The core of a Fountain Code
	/// </summary>
	public interface IFountainCodeImplementation
	{
		/// <summary>
		/// Returns the number of original data symbols this implementation is good for
		/// </summary>
		int NumSymbols { get; }

		/// <summary>
		/// Calculates and returns the needed number of coefficients for the given symbol ID. Flagged bits indicate which symbols need to be XOR'd together for this encoding symbol
		/// </summary>
		/// <param name="symbolId"></param>
		/// <param name="complexity">The number of operations that had to be performed to generate these coefficients</param>
		/// <returns></returns>
		bool[] GenerateCoefficients(long symbolId, ref int complexity);
	}
}
