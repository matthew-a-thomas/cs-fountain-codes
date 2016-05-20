using Library.FountainCodeImplementations;
using System;
using System.Collections.Generic;

namespace Library
{
	/// <summary>
	/// A class which generates encoding symbols by XOR'ing together parts of a message
	/// </summary>
	public class Sender
	{
		/// <summary>
		/// Allows duplicates
		/// </summary>
		private class Comparer : IComparer<byte>
		{
			public int Compare(byte x, byte y)
			{
				if (x > y)
					return -1;
				else
					return 1;
			}
		}

		/// <summary>
		/// The data that will be combined into encoding symbols
		/// </summary>
		Symbol<byte>[] data;

		/// <summary>
		/// The implementation we should use to generate coefficients
		/// </summary>
		IFountainCodeImplementation implementation;

		/// <summary>
		/// The ID of the next encoding symbol to send
		/// </summary>
		long symbolID;

		/// <summary>
		/// The number of bytes in each symbol
		/// </summary>
		long symbolSize;

		/// <summary>
		/// Sets up a new sender that will combine the given data according to the coefficients that come from the given function
		/// </summary>
		/// <param name="data"></param>
		/// <param name="implementation">An implementation that generates a boolean array of coefficients. The set bits indicate which data symbols get XOR'd together to generate an encoding symbol</param>
		public Sender(Symbol<byte>[] data, IFountainCodeImplementation implementation)
		{
			this.data = data;
			this.implementation = implementation;
			this.symbolID = 0;
			this.symbolSize = Symbol<byte>.GetUniformSize(data);
		}

		/// <summary>
		/// Generates a (probably unique) encoding symbol. The returned Tuple contains an array of bytes representing the index being encoded, and an array of encoded data. Each element of the encoded data has been encoded with sequential indices starting with the given index
		/// </summary>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public Tuple<bool[], Symbol<byte>> GenerateNext(ref int complexity)
		{
			// Get a set of coefficients for this symbol
			var coefficients = this.implementation.GenerateCoefficients(this.symbolID++, ref complexity);
			if (coefficients.Length != this.data.Length)
				throw new Exception("The implementation that was given in the constructor of this object doesn't generate a coefficient array of the correct length");

			// Now XOR together all the symbols flagged by the coefficients array
			Symbol<byte> value = new Symbol<byte>(this.symbolSize); complexity += (int)this.symbolSize;
			for (var i = 0; i < coefficients.Length; i++) // O(n)
			{
				complexity++;
				if (coefficients[i])
				{
					complexity++;
					value ^= this.data[i];
				}
			}

			// Return the encoding symbol
			return new Tuple<bool[], Symbol<byte>>(coefficients, value);
		}
	}
}
