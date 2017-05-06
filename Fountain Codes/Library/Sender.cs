using Library.FountainCodeImplementations;
using System;

namespace Library
{
	/// <summary>
	/// A class which generates encoding symbols by XOR'ing together parts of a message
	/// </summary>
	public class Sender
	{
	    /// <summary>
		/// The data that will be combined into encoding symbols
		/// </summary>
		private readonly Symbol<byte>[] _data;

		/// <summary>
		/// The implementation we should use to generate coefficients
		/// </summary>
		private readonly IFountainCodeImplementation _implementation;

		/// <summary>
		/// The ID of the next encoding symbol to send
		/// </summary>
		private long _symbolId;

		/// <summary>
		/// The number of bytes in each symbol
		/// </summary>
		private readonly long _symbolSize;

		/// <summary>
		/// Sets up a new sender that will combine the given data according to the coefficients that come from the given function
		/// </summary>
		/// <param name="data"></param>
		/// <param name="implementation">An implementation that generates a boolean array of coefficients. The set bits indicate which data symbols get XOR'd together to generate an encoding symbol</param>
		public Sender(Symbol<byte>[] data, IFountainCodeImplementation implementation)
		{
			_data = data;
			_implementation = implementation;
			_symbolId = 0;
			_symbolSize = Symbol<byte>.GetUniformSize(data);
		}

		/// <summary>
		/// Generates a (probably unique) encoding symbol. The returned Tuple contains an array of bytes representing the index being encoded, and an array of encoded data. Each element of the encoded data has been encoded with sequential indices starting with the given index
		/// </summary>
		/// <param name="complexity">The number of operations that had to be performed</param>
		/// <returns></returns>
		public Tuple<bool[], Symbol<byte>> GenerateNext(ref int complexity)
		{
			// Get a set of coefficients for this symbol
			var coefficients = _implementation.GenerateCoefficients(_symbolId++, ref complexity);
			if (coefficients.Length != _data.Length)
				throw new Exception("The implementation that was given in the constructor of this object doesn't generate a coefficient array of the correct length");

			// Now XOR together all the symbols flagged by the coefficients array
			var value = new Symbol<byte>(_symbolSize); complexity += (int)_symbolSize;
			for (var i = 0; i < coefficients.Length; i++) // O(n)
			{
				complexity++;
			    if (!coefficients[i])
                    continue;
			    complexity++;
			    value ^= _data[i];
			}

			// Return the encoding symbol
			return new Tuple<bool[], Symbol<byte>>(coefficients, value);
		}
	}
}
