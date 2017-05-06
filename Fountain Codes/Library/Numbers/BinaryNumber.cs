using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Library.Numbers
{
	/// <summary>
	/// An unsigned binary number of arbitrary length. Note this is just an extension of LinkedList&lt;bool&gt;, so you can iterate through each of the digits and can add/remove digits. The most significant digit is first
	/// </summary>
	public class BinaryNumber : LinkedList<bool>
	{
		/// <summary>
		/// Makes the equivalent of a compacted number 0
		/// </summary>
		// ReSharper disable once RedundantBaseConstructorCall
		private BinaryNumber() : base() { }

		/// <summary>
		/// Makes a binary number based on the collection of bits given in Big Endian format
		/// </summary>
		/// <param name="collection"></param>
		private BinaryNumber(IEnumerable<bool> collection)
			: base(collection)
		{
			Compact();
		}

		/// <summary>
		/// Builds a new (compacted) binary number from the given binary string
		/// </summary>
		/// <param name="binaryString"></param>
		// ReSharper disable once UnusedMember.Global
		public BinaryNumber(string binaryString)
		{
			var started = false;
			foreach (var digit in binaryString)
			{
				if (digit == '1')
				{
					AddLast(true);
					started = true;
				}
				else if (started)
				{
					AddLast(false);
				}
			}
		}

		/// <summary>
		/// Constructs a new BinaryNumber from the given integer
		/// </summary>
		/// <param name="integer"></param>
		public BinaryNumber(BigInteger integer)
		{
			if (integer.Sign < 0)
				throw new Exception("A binary number is unsigned, but you passed in a negative number");
			foreach (var b in integer.ToByteArray()) // ToByteArray gives the least-significant byte first
			{
				for (byte i = 0; i < 8; i++)
				{
					AddFirst(Binary.IsBitSet(b, i)); // Since we're going backword, add bits onto the beginning each time
				}
			}
			Compact();
		}

		/// <summary>
		/// Removes leading falses
		/// </summary>
		private void Compact()
		{
			while (First != null && !First.Value)
				RemoveFirst();
		}

        public override bool Equals(object obj) => this == (BinaryNumber)obj;

	    public override int GetHashCode()
	    {
	        var hash = 0;
	        var i = 0;
	        foreach (var bit in this)
	        {
                if (bit)
                    hash += i;
                ++i;
	        }
            return hash;
	    }

	    /// <summary>
        /// Returns the least significant bit
        /// </summary>
        /// <returns></returns>
        public bool LeastSignificantBit()
		{
			if (Last != null)
				return Last.Value;
			else
				return false;
		}

		/// <summary>
		/// Finds the index of the most significant bit that's set. The index is the number of digits to the left from the end (1 if there's only a single bit and it's set). Returns 0 if no bits are set
		/// </summary>
		/// <returns></returns>
		public int MostSignificantBitIndex()
		{
			var index = Count;
			foreach (var bit in this)
			{
				if (bit)
					return index;
				index--;
			}
			return 0;
		}

		/// <summary>
		/// Rotates all the bits left, appending a false as the least-significant bit
		/// </summary>
		public void RotateLeft()
		{
			AddLast(false);
		}

		/// <summary>
		/// Rotates all the bits right, discarding the least-significant bit
		/// </summary>
		public void RotateRight()
		{
			RemoveLast();
		}

		/// <summary>
		/// Converts this binary number into a BigInteger
		/// </summary>
		/// <returns></returns>
		public BigInteger ToBigInteger()
		{
			var total = new BigInteger();

			var degree = MostSignificantBitIndex();
			var i = 0;
			var node = Last;
			var multipleOfTwo = new BigInteger(1);
			while (node != null && i++ < degree)
			{
				if (node.Value)
					total += multipleOfTwo;

				multipleOfTwo *= 2;
				node = node.Previous;
			}
			return total;
		}

		/// <summary>
		/// Returns a string representation of this binary number. The most significant bit comes first
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Join("", this.ToArray()).Replace("True", "1").Replace("False", "0");
		}

		/// <summary>
		/// Performs an abstract operation on two numbers, returning the compacted result
		/// </summary>
		/// <param name="a">One binary number</param>
		/// <param name="b">The other binary number</param>
		/// <param name="bitwiseFn">A function with parameters a_i, b_i, state and returning an operation on those</param>
		/// <param name="stateFn">A function that calculates the new state based on a_i, b_i, and the state</param>
		/// <param name="remainderBitwiseFn">A function that returns an operation on r_i, state. r_i is a part of the remaining digits (when one binary number is longer than the other)</param>
		/// <param name="remainderStateFn">A function that calculates the state in the remainder case</param>
		/// <param name="followupFn">A function that can optionally include an additional digit based on the state after everything completes</param>
		/// <returns></returns>
		private static BinaryNumber Operate(BinaryNumber a, BinaryNumber b, Func<bool, bool, bool, bool> bitwiseFn, Func<bool, bool, bool, bool> stateFn, Func<bool, bool, bool> remainderBitwiseFn, Func<bool, bool, bool> remainderStateFn, Func<bool, bool> followupFn)
		{
			var result = new BinaryNumber();

			var aNode = a.Last;
			var bNode = b.Last;
			var state = false;

			// Do the bitwise operation
			while (aNode != null && bNode != null)
			{
				// Calculate values
				var aI = aNode.Value;
				var bI = bNode.Value;
				var bitwiseResult = bitwiseFn != null ? bitwiseFn(aI, bI, state) : false;
				state = stateFn != null ? stateFn(aI, bI, state) : false;

				// Prepend the calculated value
				result.AddFirst(bitwiseResult);

				// Proceed
				aNode = aNode.Previous;
				bNode = bNode.Previous;
			}

			// Either a or b might be longer than the other, so extend the result with the remaining bits
			// At this point, only up to one of (aNode, bNode) is non-null
			if (bNode != null)
				aNode = bNode;
			// If either aNode or bNode were non-null, that node is now called "aNode". If they were both null then aNode is null
			while (aNode != null)
			{
				// Calculate values in the remainder case
				var rI = aNode.Value;
				var remainderResult = remainderBitwiseFn != null ? remainderBitwiseFn(rI, state) : false;
				state = remainderStateFn != null ? remainderStateFn(rI, state) : false;

				// Prepend the calculated value
				result.AddFirst(remainderResult);

				// Proceed
				aNode = aNode.Previous;
			}

			// Now do something with the remaining state bit
			result.AddFirst(followupFn(state));

			// Finally, compact the result
			result.Compact();

			return result;
		}

		/// <summary>
		/// Performs the bitwise XOR of a and b, returning a new binary number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator ^(BinaryNumber a, BinaryNumber b)
		{
			return Operate(
				a,
				b,
				(aI, bI, state) => // Bitwise function
				    aI ^ bI,
				(aI, bI, state) => // Calculate state in the bitwise case
				    false,
				(rI, state) => // Remainder function
				    rI,
				(rI, state) => // Calculate state in the remainder case
				    false,
				state => // Do something with the state
				    false);
		}

		/// <summary>
		/// Performs the bitwise AND of a and b, returning the new binary number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator &(BinaryNumber a, BinaryNumber b)
		{
			return Operate(
				a,
				b,
				(aI, bI, state) => // Bitwise function
				{
				    return aI & bI;
				},
				(aI, bI, state) => // Calculate state in the bitwise case
				{
				    return false;
				},
				(rI, state) => // Remainder function
				{
				    return false;
				},
				(rI, state) => // Calculate state in the remainder case
				{
				    return false;
				},
				(state) => // Do something with the state
				{
				    return false;
				});
		}

		/// <summary>
		/// Performs the bitwise OR of a and b, returning the new binary number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator |(BinaryNumber a, BinaryNumber b)
		{
			return Operate(
				a,
				b,
				(aI, bI, state) => // Bitwise function
				{
				    return aI | bI;
				},
				(aI, bI, state) => // Calculate state in the bitwise case
				{
				    return false;
				},
				(rI, state) => // Remainder function
				{
				    return rI;
				},
				(rI, state) => // Calculate state in the remainder case
				{
				    return false;
				},
				(state) => // Do something with the state
				{
				    return false;
				});
		}

		/// <summary>
		/// Performs the bitwise NOT of a, returning the new number. Note that this will only flip bits after (and including) the most significant bit that is set, so ~(~A) != A. E.g. ~100 = 11, but ~11 = 0
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static BinaryNumber operator ~(BinaryNumber a)
		{
			return Operate(
				a,
				a,
				(aI, bI, state) => // Bitwise function
				{
				    return !aI;
				},
				(aI, bI, state) => // Calculate state in the bitwise case
				{
				    return false;
				},
				(rI, state) => // Remainder function
				{
				    return false;
				},
				(rI, state) => // Calculate state in the remainder case
				{
				    return false;
				},
				(state) => // Do something with the state
				{
				    return false;
				});
		}

		/// <summary>
		/// Returns true if the two numbers represent the same number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(BinaryNumber a, BinaryNumber b)
		{
			var comparison = a ^ b;
			return comparison.Count == 0;
		}

		/// <summary>
		/// Returns the opposite of a == b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(BinaryNumber a, BinaryNumber b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Determines if a is greater than b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >(BinaryNumber a, BinaryNumber b)
		{
			var bMsbi = b.MostSignificantBitIndex();
			var aMsbi = a.MostSignificantBitIndex();
			if (bMsbi > aMsbi)
				return false; // The highest set bit in b is higher than in a
			else if (aMsbi > bMsbi)
				return true; // The highest set bit in a is higher than in b

			// Both a and b have their highest bits set in the same place. Let's start from the end and work our way up to that point
			var aNode = a.Last;
			var bNode = b.Last;
			var aWinning = false;
			var count = 0;
			while (aNode != null && bNode != null && count++ <= aMsbi)
			{
				var aI = aNode.Value;
				var bI = bNode.Value;

				if (aI & !bI)
					aWinning = true;
				else if (bI & !aI)
					aWinning = false;

				aNode = aNode.Previous;
				bNode = bNode.Previous;
			}
			return aWinning;
		}

		/// <summary>
		/// Determines if a is at least as large as b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >=(BinaryNumber a, BinaryNumber b)
		{
			return a > b || a == b;
		}

		/// <summary>
		/// Determines if a is less than b
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <(BinaryNumber a, BinaryNumber b)
		{
			return !(a > b) && !(a == b);
		}

		/// <summary>
		/// Determines if b is at least as large as a
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <=(BinaryNumber a, BinaryNumber b)
		{
			return !(a > b);
		}

		/// <summary>
		/// Performs the addition of a and b, returning a new binary number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator +(BinaryNumber a, BinaryNumber b)
		{
			return Operate(
				a,
				b,
				(aI, bI, state) => // Bitwise function
				{
				    return aI ^ bI ^ state;
				},
				(aI, bI, state) => // Calculate state in the bitwise case
				{
				    return (aI & bI) | (state & (aI ^ bI));
				},
				(rI, state) => // Remainder function
				{
				    return rI ^ state;
				},
				(rI, state) => // Calculate state in the remainder case
				{
				    return rI & state;
				},
				(state) => // Do something with the state
				{
				    return state;
				});
		}

		/// <summary>
		/// Performs the subtraction of b from a, returning a new binary number. Throws an exception if b > a
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator -(BinaryNumber a, BinaryNumber b)
		{
			if (b > a)
				throw new Exception("Can only subtract a number from a number at least as large");
			return Operate(
				a,
				b,
				(aI, bI, state) => // Bitwise function
				{
				    return aI ^ bI ^ state;
				},
				(aI, bI, state) => // Calculate state in the bitwise case
				{
				    return (aI & bI & state) | (!aI & (bI | state));
				},
				(rI, state) => // Remainder function
				{
				    return rI ^ state;
				},
				(rI, state) => // Calculate state in the remainder case
				{
				    return !rI & state;
				},
				(state) => // Do something with the state
				{
				    if (state)
				        throw new Exception("For some reason there's still a carry bit set at the end of the subtraction, even though a is supposed to be at least as large as b. This isn't supposed to happen, and means that the BinaryNumber class has a bug in its code somewhere");
				    else
				        return false;
				});
		}

		/// <summary>
		/// Performs multiplication of a and b, returning the new binary number
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static BinaryNumber operator *(BinaryNumber a, BinaryNumber b)
		{
			var result = new BinaryNumber();

			var aPrime = new BinaryNumber(a); // Makes a copy of a
			var bNode = b.Last;
			// Do long multiplication
			while (bNode != null)
			{
				if (bNode.Value)
					result += aPrime;

				aPrime.RotateLeft();

				bNode = bNode.Previous;
			}

			return result;
		}

		/// <summary>
		/// Performs integer division (the floor of the quotient) of dividend / divisor
		/// </summary>
		/// <param name="dividend"></param>
		/// <param name="divisor"></param>
		/// <returns></returns>
		public static BinaryNumber operator /(BinaryNumber dividend, BinaryNumber divisor)
		{
			BinaryNumber remainder;
			BinaryNumber quotient;
			LongDivision(dividend, divisor, out remainder, out quotient);
			return quotient;
		}

		/// <summary>
		/// Performs the remainder after integer division
		/// </summary>
		/// <param name="dividend"></param>
		/// <param name="modulus"></param>
		/// <returns></returns>
		public static BinaryNumber operator %(BinaryNumber dividend, BinaryNumber modulus)
		{
			BinaryNumber remainder;
			BinaryNumber quotient;
			LongDivision(dividend, modulus, out remainder, out quotient);
			return remainder;
		}

		/// <summary>
		/// Performs an integer division, returning the quotient and remainder after division
		/// </summary>
		/// <param name="dividend"></param>
		/// <param name="divisor"></param>
		/// <param name="remainder"></param>
		/// <param name="quotient"></param>
		public static void LongDivision(BinaryNumber dividend, BinaryNumber divisor, out BinaryNumber remainder, out BinaryNumber quotient)
		{
			var modifiedDivisor = new BinaryNumber(divisor);
			remainder = new BinaryNumber(dividend);
			while (remainder > modifiedDivisor)
				modifiedDivisor.RotateLeft();
			if (modifiedDivisor > remainder)
				modifiedDivisor.RotateRight();
			quotient = new BinaryNumber();
			while (remainder > divisor)
			{
				if (modifiedDivisor > remainder)
				{
					quotient.AddLast(false);
				}
				else
				{
					remainder -= modifiedDivisor;
					quotient.AddLast(true);
				}
				modifiedDivisor.RotateRight();
			}
			quotient.Compact();
		}
	}
}
