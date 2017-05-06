using System;

namespace Library.DiscreteDistributions
{
	/// <summary>
	/// An implementation of the Robust Soliton Distribution's CDF
	/// </summary>
	public static class RobustSolitonDistribution
	{
		/// <summary>
		/// Generates a cumulative density function for the Robust Soliton Distribution
		/// </summary>
		/// <param name="k">The number of symbols</param>
		/// <param name="r">The expected ripple size</param>
		/// <param name="delta">The probability of a random walk deviating from its mean by more than a certain amount</param>
		/// <returns></returns>
		/// <remarks>This is a very straightforward implementation based on Michael Luby's "LT Codes" paper given in the Proceedings of the 43 rd Annual IEEE Symposium on Foundations of Computer Science</remarks>
		public static double[] GenerateCdf(int k, int r, double delta)
		{
			if (r < 1)
				throw new Exception("Apparently the parameter R must be greater than 1");

			// rho starts us with the Ideal Soliton Distribution
			var rho = new double[k + 1];
			rho[1] = 1 / (double)k;
			for (var i = 2; i <= k; i++)
			{
				rho[i] = 1 / (i * (double)(i - 1));
			}

			// tau is the correction to that Ideal Soliton Distribution
			var tau = new double[k + 1];
			for (var i = 1; i < k / r; i++)
			{
				tau[i] = r / (i * (double)k);
			}
			tau[k / r] = r * Math.Log(r / delta) / k;

			// beta is the sum of everything
			var beta = 0.0;
			for (var i = 0; i <= k; i++)
			{
				beta += rho[i] + tau[i];
			}

			// mu is the normalized PDF
			var mu = new double[k + 1];
			for (var i = 0; i <= k; i++)
			{
				mu[i] = (rho[i] + tau[i]) / beta;
			}

			// Turn that into a CDF
			var cdf = new double[k + 1];
			for (var i = 1; i <= k; i++)
			{
				cdf[i] = cdf[i - 1] + mu[i];
			}
			// Make sure the last element of the CDF is exactly 1
			cdf[k] = 1.0;

			return cdf;
		}
	}
}
