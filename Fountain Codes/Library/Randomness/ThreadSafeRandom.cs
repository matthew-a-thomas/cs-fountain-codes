using System;

namespace Library.Randomness
{
	/// <summary>
	/// A thread-safe implementation of Random, since Random isn't thread-safe by itself (https://msdn.microsoft.com/en-us/library/system.random#ThreadSafety)
	/// </summary>
	public class ThreadSafeRandom : Random
	{
		object lockObject = new object();

		/// <summary>
		/// Initializes a new instance of the System.Random class, using a time-dependent default seed value.
		/// </summary>
		public ThreadSafeRandom() : base() { }
		/// <summary>
		/// Initializes a new instance of the System.Random class, using the specified seed value.
		/// </summary>
		public ThreadSafeRandom(int seed) : base(seed) { }

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns></returns>
		public override int Next()
		{
			lock (this.lockObject)
			{
				return base.Next();
			}
		}

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public override int Next(int maxValue)
		{
			lock (this.lockObject)
			{
				return base.Next(maxValue);
			}
		}

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public override int Next(int minValue, int maxValue)
		{
			lock (this.lockObject)
			{
				return base.Next(minValue, maxValue);
			}
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer"></param>
		public override void NextBytes(byte[] buffer)
		{
			lock (this.lockObject)
			{
				base.NextBytes(buffer);
			}
		}

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		/// <returns></returns>
		public override double NextDouble()
		{
			lock (this.lockObject)
			{
				return base.NextDouble();
			}
		}
	}
}
