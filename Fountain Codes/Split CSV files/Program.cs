using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split_CSV_files
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(
@"This program splits an output CSV file up into different files, applies
filters, and additionally appends some computed columns");

			var files = new Dictionary<string, StreamWriter>();
			var parts = new Dictionary<string, int>();
			// Assuming the header is "k","p","test","n","generation-complexity","solution-complexity"
			parts["k"] = 0;
			parts["p"] = 1;
			parts["test"] = 2;
			parts["n"] = 3;
			parts["generation-complexity"] = 4;
			parts["solution-complexity"] = 5;

			using (var reader = new StreamReader(@"C:\Users\MAT7317\Dropbox\Personal\Master's Project\output.csv"))
			{
				var line = reader.ReadLine();
				// This first line should be the header
				var header = line;

				Console.WriteLine("Processing...");
				while ((line = reader.ReadLine()) != null) // Continue reading lines to figure out where to put each line
				{
					var split = line.Split(',');

					// Pull out the parts
					var k = int.Parse(split[parts["k"]]);
					var p = double.Parse(split[parts["p"]]);
					var test = split[parts["test"]];
					var n = int.Parse(split[parts["n"]]);
					var generationComplexity = int.Parse(split[parts["generation-complexity"]]);
					var solutionComplexity = int.Parse(split[parts["solution-complexity"]]);

					// Figure out computed columns
					var kInv = Math.Pow(k, -1);
					var logK = Math.Log(k);
					long k2 = k * k;
					long k3 = k2 * k;
					var expK = Math.Exp(k);
					var nInv = Math.Pow(n, -1);
					var logN = Math.Log(n);
					long n2 = n * n;
					long n3 = n2 * n;
					var expN = Math.Exp(n);
					var logP = Math.Log(p);
					var expP = Math.Exp(p);
					var overheadFraction = (double)n / (double)k;
					var overheadRaw = n - k;

					// Make sure n isn't -1, which indicates a failure
					if (n < 0)
						continue;

					// See if we've got a writer set up for this
					StreamWriter writer;
					if (!files.TryGetValue(test, out writer))
					{
						writer = new StreamWriter(@"C:\Users\MAT7317\Dropbox\Personal\Master's Project\output-" + test + ".csv");
						files[test] = writer;

						writer.Write(header); // Write the original header
						writer.WriteLine(@",""log-p"",""exp-p"",""k-inv"",""log-k"",""k-squared"",""k-cubed"",""exp-k"",""n-inv"",""log-n"",""n-squared"",""n-cubed"",""exp-n"",""overhead-fraction"",""overhead-raw"""); // Append to that header the computed columns
					}

					// Write this line out to this writer
					writer.Write(line); // The original line
					writer.Write(',');
					writer.WriteLine(string.Join(",", // The additional computed data points. Make sure these are in the same order as the above line which prints the header
						logP,
						expP,
						kInv,
						logK,
						k2,
						k3,
						expK,
						nInv,
						logN,
						n2,
						n3,
						expN,
						overheadFraction,
						overheadRaw
						));
				}
			}

			// Dispose of all the opened files
			foreach (var file in files.Values)
			{
				file.Dispose();
			}

			Console.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
		}
	}
}
