using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Numbers
{
	/// <summary>
	/// A class which helps deal with numbers and byte arrays
	/// </summary>
	public static class Bytes
	{
		/// <summary>
		/// Returns a byte array of the given number that's just large enough to hold it
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static byte[] Compact(ulong number)
		{
			return Bytes.Compact(number, number);
		}

		/// <summary>
		/// Returns a byte array of the given number that's large enough to hold the given maximum number.
		/// 
		/// For example, for number=0x8F and maxNumber=0xFFFF, this returns {0x00, 0x8F}
		/// </summary>
		/// <param name="number"></param>
		/// <param name="maxNumber"></param>
		/// <returns></returns>
		public static byte[] Compact(ulong number, ulong maxNumber)
		{
			if (number > maxNumber)
				throw new Exception("The given number is greater than the given maxNumber");

			var allBytes = BitConverter.GetBytes(number);
			var numNeededBytes = Bytes.GetMinimumNumberOfBytesForHolding(maxNumber);
			var compactBytes = new byte[numNeededBytes];
			Array.Copy(allBytes, 0, compactBytes, 0, numNeededBytes);
			return compactBytes;
		}

		/// <summary>
		/// Returns the minimum number of bytes necessary to hold the given number.
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static byte GetMinimumNumberOfBytesForHolding(ulong number)
		{
			// Count how many bits high the highest-set bit is
			byte highestPowerOfTwo = 0;
			for (byte powerOfTwo = 63; powerOfTwo < 255; powerOfTwo--) // Stop once powerOfTwo==255 since that means we've rolled around past zero. We search down like this so that we don't have to search through more bits than necessary
			{
				if (Binary.IsBitSet(number, powerOfTwo))
				{
					highestPowerOfTwo = powerOfTwo;
					break;
				}
			}
			// It takes one bit to store 2^0, two bits to store 2^1 through 1+2^0, three bits to store 2^2 through 1+2^1, four bits to store 2^3 through 1+2^2
			var numBits = highestPowerOfTwo + 1;

			// ceil(numBits/8)
			var numBytes = (uint)numBits / 8;
			if (numBits > numBytes * 8)
				numBytes++;

			return (byte)numBytes;
		}

		/// <summary>
		/// Returns an array of bytes of the specified size
		/// </summary>
		/// <param name="numBytes"></param>
		/// <param name="random"></param>
		/// <returns></returns>
		public static byte[] GetRandomBytes(int numBytes, Random random)
		{
			var randomBytes = new byte[numBytes];
			random.NextBytes(randomBytes);
			return randomBytes;
		}

		/// <summary>
		/// Returns an unsigned 64-bit integer from the given array of bytes. The array can be up to 8 bytes long
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static ulong Uncompact(byte[] bytes)
		{
			var expanded = new byte[8];
			var difference = expanded.Length - bytes.Length;
			if (difference < 0)
				throw new Exception("The given array of bytes is too large to place into an unsigned 64-bit integer");
			for (var i = 0; i < expanded.Length; i++)
			{
				if (i < bytes.Length)
				{
					expanded[i] = bytes[i];
				}
				else
				{
					expanded[i] = 0;
				}
			}
			return BitConverter.ToUInt64(expanded, 0);
		}
	}
}
