using System;

namespace Library
{
	/// <summary>
	/// Encapsulates an array things, allowing the XOR operator to be performed on them
	/// </summary>
	/// <typeparam name="T">The type of the thing that this symbols holds. Note that the given type must have the ^ (XOR) operator defined or this will crash at runtime</typeparam>
	public class Symbol<T> where T : struct
	{
		/// <summary>
		/// The encapsulated array of bytes
		/// </summary>
		public T[] Data;

		/// <summary>
		/// Creates a new Symbol from the given array of bytes
		/// </summary>
		/// <param name="data"></param>
		public Symbol(T[] data)
		{
			Data = data;
		}

		/// <summary>
		/// Creates a new symbol of the given order initialized to zero
		/// </summary>
		/// <param name="numBytes"></param>
		public Symbol(long numBytes)
		{
			Data = new T[numBytes];
			for (long i = 0; i < numBytes; i++)
			{
				Data[i] = default(T);
			}
		}

		/// <summary>
		/// Asserts that all the given symbols are the same size, returning that size
		/// </summary>
		/// <param name="symbols"></param>
		/// <returns></returns>
		public static long GetUniformSize(params Symbol<T>[] symbols)
		{
			var symbolSize = symbols[0].Data.LongLength;
			for (long i = 1; i < symbols.LongLength; i++)
			{
				if (symbols[i].Data.LongLength != symbolSize)
					throw new Exception("The given symbols aren't all the same size");
			}
			return symbolSize;
		}

		/// <summary>
		/// Converts a symbol to an array of bytes
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public static implicit operator T[](Symbol<T> symbol)
		{
			return symbol.Data;
		}

		/// <summary>
		/// Converts an array of bytes to a symbol
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static implicit operator Symbol<T>(T[] data)
		{
			return new Symbol<T>(data);
		}

		/// <summary>
		/// Performs the XOR operator on two symbols, returning a completely new symbol (doesn't modify either original symbol)
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Symbol<T> operator ^(Symbol<T> a, Symbol<T> b)
		{
			if (a.Data.LongLength != b.Data.LongLength)
				throw new Exception("Only symbols from the same order of finite field can be XOR'd together");
			var newData = new T[a.Data.LongLength];
			for (long i = 0; i < a.Data.LongLength; i++)
			{
				newData[i] = (T)((dynamic)a.Data[i] ^ b.Data[i]); // If only C# would let interfaces specify operators!
			}
			return new Symbol<T>(newData);
		}
	}
}
