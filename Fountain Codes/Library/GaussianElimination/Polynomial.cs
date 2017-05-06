using Library.Numbers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Library.GaussianElimination
{
	/// <summary>
	/// Represents a polynomial that's good for GF(2^n)
	/// </summary>
	public class Polynomial : IComparable<Polynomial>
	{
		/// <summary>
		/// Represents the polynomial 1
		/// </summary>
		public static readonly Polynomial One = new Polynomial(new[] { true });

		/// <summary>
		/// Represents the polynomial X
		/// </summary>
		public static readonly Polynomial X = new Polynomial(new[] { false, true });

		/// <summary>
		/// This polynomial's coefficients in Little-Endian order (e.g. 1 + x + x^2 + ...)
		/// </summary>
		private readonly bool[] _coefficients;

		/// <summary>
		/// Gets this polynomial's degree (the highest power of x e.g. deg(1 + x) = 1)
		/// </summary>
		public int Degree => _coefficients.Length - 1;

	    /// <summary>
		/// Gets a copy of this polynomial's coefficients. Keep in mind that they're in Little-Endian order (e.g. 1 + x + x^2)
		/// </summary>
		public bool[] Coefficients => _coefficients.Clone() as bool[];

	    /// <summary>
		/// Indicates whether this polynomial is 1
		/// </summary>
		public bool IsOne => Degree == 0;

	    /// <summary>
		/// Indicates whether this polynomial is zero
		/// </summary>
		public bool IsZero => Degree == -1;

	    /// <summary>
		/// Same as calling GetNumber()
		/// </summary>
		public BigInteger Number => GetNumber();

	    /// <summary>
		/// Creates a new polynomial object from the given coefficients. If the given coefficients can be trimmed down then a copy is made and then trimmed down. Otherwise the given array is used without copying
		/// </summary>
		/// <param name="coefficients">The coefficients from smallest degree to largest</param>
		public Polynomial(bool[] coefficients)
		{
			// Find the maximum degree (power of x) that the given coefficients represent
			int maxDegree;
		    // ReSharper disable once EmptyEmbeddedStatement
			for (maxDegree = coefficients.Length - 1; maxDegree >= 0 && !coefficients[maxDegree]; maxDegree--) ;

			// Now make a trimmed-down copy of the given coefficients
			if (maxDegree + 1 < coefficients.Length)
			{
				_coefficients = new bool[maxDegree + 1];
				for (var i = 0; i < _coefficients.Length; i++)
				{
					_coefficients[i] = coefficients[i];
				}
			}
			else
			{
				_coefficients = coefficients;
			}
		}

		/// <summary>
		/// Creates a new polynomial object from the given integer
		/// </summary>
		/// <param name="decimalForm"></param>
		public Polynomial(BigInteger decimalForm) : this(ConvertToPolynomialBits(decimalForm)) { }

		/// <summary>
		/// Accepts a list of bits that should be set in the coefficient. This is more convenient if you're dealing with a list like http://www.ams.org/journals/mcom/1992-59-200/S0025-5718-1992-1134730-7/S0025-5718-1992-1134730-7.pdf
		/// </summary>
		/// <param name="setBits"></param>
		public Polynomial(params int[] setBits)
			: this(new Func<bool[]>(() =>
				{
					var maxSetBit = 0;
					foreach (var setBit in setBits)
					{
						maxSetBit = Math.Max(maxSetBit, setBit);
					}
					var coefficients = new bool[maxSetBit + 1];
					foreach (var setBit in setBits)
					{
						coefficients[setBit] = true;
					}
					return coefficients;
				})())
		{ }

		/// <summary>
		/// Gets the equivalent binary number for the given integer, then reverses the order of the bits for use as a polynomial's coefficients
		/// </summary>
		/// <param name="decimalForm"></param>
		/// <returns></returns>
		private static bool[] ConvertToPolynomialBits(BigInteger decimalForm)
		{
			var binary = new BinaryNumber(decimalForm); // 10 => 1010. But note that this is in Big-Endian format, where we need Little-Endian
			var bits = new bool[binary.Count];
			var node = binary.Last;
			var i = 0;
			while (node != null)
			{
				bits[i] = node.Value;

				i++;
				node = node.Previous;
			}
			return bits;
		}

		/// <summary>
		/// Creates a deep copy of this polynomial
		/// </summary>
		/// <returns></returns>
		public Polynomial Clone()
		{
			return new Polynomial(Coefficients);
		}

		/// <summary>
		/// Returns a number comparing this polynomial to the other
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(Polynomial other)
		{
			return (GetNumber() - other.GetNumber()).Sign;
		}

		/// <summary>
		/// Indicates if the given object is a polynomial that equals this one
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
		    var polynomial = obj as Polynomial;
		    return polynomial != null && polynomial == this;
		}

        /// <summary>
        /// Computes the hash code of this <see cref="Polynomial"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = Degree;
            for (var i = 0; i < _coefficients.Length; ++i)
                if (_coefficients[i])
                    hashCode += i;
            return hashCode;
        }

        /// <summary>
        /// Returns the decimal form of the given index, assuming that this polynomial is a primitive polynomial for a finite field
        /// </summary>
        /// <param name="indexForm"></param>
        /// <returns></returns>
        public BigInteger GetDecimalForm(BigInteger indexForm)
		{
			var coefficients = new bool[(long)indexForm + 1];
			coefficients[coefficients.Length - 1] = true;
			var polynomial = new Polynomial(coefficients); // Now the polynomial is x^index
			polynomial %= this; // Find the remainder after division by this polynomial, which will give us the polynomial form
			return polynomial.GetNumber(); // This turns the polynomial form into a number
		}

		/// <summary>
		/// Returns the index form of the given decimal, assuming that this polynomial is a primitive polynomial for a finite field
		/// </summary>
		/// <param name="decimalForm"></param>
		/// <returns></returns>
		public BigInteger GetIndexForm(BigInteger decimalForm)
		{
			if (decimalForm.IsZero)
				return 0; // This is by definition

			// This is kind of the opposite of performing modulus. Instead of subtracting increasing smaller multiples of the primitive polynomial to cancel out the higher bits until there's only a remainder left, we start with the remainder and add on increasingly larger multiples of the primitive polynomial in order to cancel out the lower bits until only a single bit is set
			var binary = new BinaryNumber(decimalForm); // The binary version of the given decimal
			var polynomial = new BinaryNumber(GetNumber()); // The binary version of this polynomial. Note that BinaryNumber's bits are set from most- to least-significant, which is the opposite of a Polynomial

			// Define a method that will throw an exception if things don't work
			var throwException = new Action(() =>
			{
				throw new Exception("This number doesn't work as a primitive polynomial for the given number, because a power of the generator couldn't be found");
			});

			// Figure out the maximum number of shifts that we'll allow before giving up
			var maxShifts = BigInteger.Pow(2, polynomial.Count - 1); // We'll look for up to this power of the generator

			var shifts = new BigInteger(); // Keeps track of the number of shifts that have been made
			while (binary.Count > 1) // Keep looping as long as there is more than one bit left to cancel out
			{
				binary ^= polynomial; // XOR with the polynomial. This gets rid of successively more lower bits
				while (!binary.LeastSignificantBit() && binary.Count > 1) // Shift right to get rid of trailing zeros
				{
					binary.RotateRight();
					shifts++; // Keep track of the number of shifts that were made
				}
				if (shifts > maxShifts)
					throwException();
			}

			if (binary.First.Value) // We wound up with the number "1" by itself. This means that the index is x^shifts
				return shifts;
			else // We wound up with zero. This means that the index form of the given decimal is undefined
				throwException();

			// Code shouldn't get this far, but the compiler doesn't recognize that our exception method will always throw an exception
			throw new NotImplementedException();
		}

		/// <summary>
		/// Converts this polynomial into its base-10 equivalent (e.g. 1 + x => 3)
		/// </summary>
		/// <returns></returns>
		public BigInteger GetNumber()
		{
			BigInteger result = 0;
			for (var i = 0; i < _coefficients.Length; i++)
			{
				if (_coefficients[i])
					result += BigInteger.Pow(2, i);
			}
			return result;
		}

		/// <summary>
		/// Returns a string representation of this polynomial
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var parts = new List<string>(_coefficients.Length);
			for (var i = 0; i < _coefficients.Length; i++)
			{
			    if (!_coefficients[i])
                    continue;
			    if (i > 1)
			    {
			        parts.Add("x^" + i);
			    }
			    else if (i == 1)
			    {
			        parts.Add("x");
			    }
			    else
			    {
			        parts.Add("1");
			    }
			}
			return string.Join("+", parts.ToArray());
		}

		/// <summary>
		/// Returns a new polynomial that is the result of summing the two given polynomials
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Polynomial Add(Polynomial a, Polynomial b)
		{
			Polynomial smaller, larger;
			if (a.Degree < b.Degree)
			{
				smaller = a;
				larger = b;
			}
			else
			{
				smaller = b;
				larger = a;
			}

			var coefficients = larger.Coefficients;
			var i = 0;
			foreach (var coefficient in smaller.Coefficients)
			{
				coefficients[i++] ^= coefficient;
			}

			return new Polynomial(coefficients);
		}

		/// <summary>
		/// Returns the remainder after dividing the given polynomial by the given modulus polynomial
		/// </summary>
		/// <param name="polynomial"></param>
		/// <param name="modulus"></param>
		/// <returns></returns>
		public static Polynomial Mod(Polynomial polynomial, Polynomial modulus)
		{
			polynomial = new Polynomial(polynomial._coefficients.Clone() as bool[]);
			if (modulus._coefficients.Length == 0)
				throw new Exception("The given modulus is equivalent to the zero polynomial. Please give a non-zero polynomial");
			while (polynomial._coefficients.Length >= modulus._coefficients.Length)
			{
				for (var i = 0; i < modulus._coefficients.Length; i++)
				{
					polynomial._coefficients[i + (polynomial._coefficients.Length - modulus._coefficients.Length)] ^= modulus._coefficients[i];
				}
				polynomial = new Polynomial(polynomial._coefficients); // Reduces the length of the polynomial's coefficient array if necessary
			}
			return polynomial;
		}

		/// <summary>
		/// Finds the multiplicative inverse of the given polynomial by multiplying it with itself 2^n - 2 times
		/// </summary>
		/// <param name="a"></param>
		/// <param name="modulus"></param>
		/// <returns></returns>
		public static Polynomial MultiplicativeInverse(Polynomial a, Polynomial modulus)
		{
			var multiplied = new Polynomial(a.Coefficients);
			for (var i = 1; i < Math.Pow(2, modulus.Degree) - 2; i++)
			{
				multiplied *= a;
				multiplied %= modulus;
			}
			return multiplied;
		}

		/// <summary>
		/// Returns the result of multiplying the two given polynomials
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Polynomial Multiply(Polynomial a, Polynomial b)
		{
			/*
			 * (x + 1)(x + 1) = x^2 + x + x + 1 = x^2 + 1 = x mod x^2 + x + 1
			 */
			var coefficients = new bool[a.Degree + b.Degree + 2];
			for (var i = 0; i < a.Coefficients.Length; i++)
			{
				for (var j = 0; j < b.Coefficients.Length; j++)
				{
					coefficients[i + j] ^= a.Coefficients[i] & b.Coefficients[j];
				}
			}
			return new Polynomial(coefficients);
		}

		/// <summary>
		/// Returns true if both polynomials are non-null and have the same degree and have identical coefficients
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Polynomial a, Polynomial b)
		{
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;
			if (a.Degree != b.Degree)
				return false;
			for (var i = 0; i < a._coefficients.Length; i++)
			{
				if (a._coefficients[i] ^ b._coefficients[i])
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns false if both polynomials have the same degree and have identical coefficients, or if either is null
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Polynomial a, Polynomial b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Adds the two polynomials together
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Polynomial operator +(Polynomial a, Polynomial b)
		{
			return Add(a, b);
		}

		/// <summary>
		/// Multiplies the two polynomials together
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Polynomial operator *(Polynomial a, Polynomial b)
		{
			return Multiply(a, b);
		}

		/// <summary>
		/// Returns the remainder after division of the modulus
		/// </summary>
		/// <param name="a"></param>
		/// <param name="modulus"></param>
		/// <returns></returns>
		public static Polynomial operator %(Polynomial a, Polynomial modulus)
		{
			return Mod(a, modulus);
		}
	}
}
