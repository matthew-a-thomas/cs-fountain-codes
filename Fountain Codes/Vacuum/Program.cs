using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vacuum
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("This program is designed to suck up all the data results being generated in realtime by other machines");
			var run = true;
			using (var outputStream = new FileStream(@"C:\Users\MAT7317\Desktop\compiled-output.csv", FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				using (var writer = new StreamWriter(outputStream))
				{
					writer.AutoFlush = true;
					while (run)
					{
						Console.WriteLine("What machine would you like to add?");
						var machineName = Console.ReadLine();
						if (machineName == "exit")
							run = false;
						else
							Task.Run(() =>
								{
									using (var stream = new FileStream(@"\\" + machineName + @"\c$\Users\MAT7317\Desktop\output.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
									{
										using (var reader = new StreamReader(stream))
										{
											var first = true;
											var count = 0;
											while (run)
											{
												var line = reader.ReadLine();
												if (line == null)
												{
													Thread.Sleep(1000);
												}
												else if (first)
												{
													first = false;
												}
												else
												{
													lock (writer)
													{
														writer.WriteLine(line);
													}
													if (count++ % 1000 == 0)
														Console.Write(".");
												}
											}
											Console.WriteLine("Exiting");
										}
									}
								});
					}
				}
			}
		}
	}
}
