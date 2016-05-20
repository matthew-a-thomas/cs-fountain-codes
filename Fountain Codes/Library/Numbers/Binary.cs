using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Numbers
{
	/// <summary>
	/// A class to help with binary numbers
	/// </summary>
	public static class Binary
	{
		/// <summary>
		/// Computes the ith gray code value
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static ulong ComputeGrayCode(ulong i)
		{
			return i ^ (i >> 1);
		}

		/// <summary>
		/// Returns the binary representation of the given value. The string will be at least numBinaryDigits long
		/// </summary>
		/// <param name="numBinaryDigits"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetBinaryString(int numBinaryDigits, long value)
		{
			var binary = Convert.ToString(value, 2);
			if (binary.Length >= numBinaryDigits)
				return binary;
			else
				return new string('0', numBinaryDigits - binary.Length) + binary;
		}

		/// <summary>
		/// Randomly populates a boolean array
		/// </summary>
		/// <param name="numBits"></param>
		/// <param name="random"></param>
		/// <returns></returns>
		public static bool[] GetRandomBits(int numBits, Random random)
		{
			var bits = new bool[numBits];
			for (var i = 0; i < numBits; i++)
			{
				bits[i] = (random.Next() % 2 == 0);
			}
			return bits;
		}

		/// <summary>
		/// Determines if the given bit is set in the given number
		/// </summary>
		/// <param name="number"></param>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static bool IsBitSet(ulong number, byte bit)
		{
			return ((number >> bit) & 1UL) == 1UL;
		}

		/// <summary>
		/// Returns the result of flipping the given bit index of the given number
		/// </summary>
		/// <param name="number"></param>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static ulong FlipBit(ulong number, byte bit)
		{
			var mask = 1UL << bit;
			var result = number ^ mask;
			return result;
		}

		/// <summary>
		/// Returns the number of non-zero bits in the binary representation of the given number
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte GetNumNonZeroBits(ulong value)
		{
			byte count = 0;
			for (byte i = 0; i < 64; i++)
			{
				count += (byte)(value & 1); // Add one if the right-most bit is set
				value >>= 1; // Divide by two
			}
			return count;
		}

		/// <summary>
		/// Gets the next number in a permutation that has the same number of bits set as the given number.
		/// 
		/// The number of different permutations of an n-bit long number with m bits set is given by (n choose m).
		/// The PascalsTriangle class in this toolkit can generate that number for you.
		/// 
		/// Based on http://stackoverflow.com/a/13614164/3063273
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static long NextPermutation(long n)
		{
			if (n == 0)
				return 0;
			var lo = n & -n;             // lowest one bit
			var lz = (n + lo) & ~n;      // lowest zero bit above lo
			n |= lz;                     // add lz to the set
			n &= ~(lz - 1);              // reset bits below lz
			n |= (lz / lo / 2) - 1;      // put back right number of bits at end
			return n;
		}

		/// <summary>
		/// Returns the result of setting the given bit index of the given number
		/// </summary>
		/// <param name="number"></param>
		/// <param name="bitIndex"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static ulong SetBit(ulong number, byte bitIndex, bool newValue)
		{
			var mask = 1UL << bitIndex;
			ulong result;
			if (newValue)
			{
				result = number | mask;
			}
			else
			{
				result = number & ~mask;
			}
			return result;
		}
	}
}
