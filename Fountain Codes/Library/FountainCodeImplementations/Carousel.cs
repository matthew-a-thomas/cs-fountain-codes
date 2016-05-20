
namespace Library.FountainCodeImplementations
{
	/// <summary>
	/// A fountain code implementation that sends the original data symbols in order and then repeats continually. This presents the user with the Coupon Collector's Problem (https://en.wikipedia.org/wiki/Coupon_collector%27s_problem)
	/// </summary>
	public class Carousel : IFountainCodeImplementation
	{
		/// <summary>
		/// The number of original data symbols that this carousel will handle
		/// </summary>
		public int NumSymbols { get; private set; }

		/// <summary>
		/// Sets up a new carousel that will handle the given number of symbols
		/// </summary>
		/// <param name="numSymbols"></param>
		public Carousel(int numSymbols)
		{
			this.NumSymbols = numSymbols;
		}

		/// <summary>
		/// Returns a boolean array with the (symbolID % NumSymbols)th bit set. This corresponds to sending that original data symbol by itself
		/// </summary>
		/// <param name="symbolID"></param>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public bool[] GenerateCoefficients(long symbolID, ref int complexity)
		{
			var coefficients = new bool[this.NumSymbols]; complexity += this.NumSymbols;
			coefficients[symbolID % this.NumSymbols] = true; complexity++;
			return coefficients;
		}
	}
}
