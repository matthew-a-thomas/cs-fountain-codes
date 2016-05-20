using com.backblaze.erasure;
using Library;
using Library.FountainCodeImplementations;
using Library.Randomness;
using net.fec.openrq;
using net.fec.openrq.decoder;
using net.fec.openrq.parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
	class Program
	{
		/// <summary>
		/// The list of fountain codes that this simulator will test
		/// </summary>
		enum Test : byte
		{
			/// <summary>
			/// RaptorQ is implemented by the OpenRQ Java library (https://github.com/openrq-team/OpenRQ) that has been compiled into a .DLL using IKVMC (http://www.ikvm.net/userguide/ikvmc.html)
			/// </summary>
			RaptorQ,

			/// <summary>
			/// LT Code is implemented in the Library project
			/// </summary>
			LTCode,

			/// <summary>
			/// Special LT Code is implemented in the Library project
			/// </summary>
			SpecialLTCode,

			/// <summary>
			/// Random Subset is implemented in the Library project
			/// </summary>
			RandomSubset,

			/// <summary>
			/// Carousel is implemented in the Library project
			/// </summary>
			Carousel,

			/// <summary>
			/// Sophisticated carousel is implemented in the Library project
			/// </summary>
			SophisticatedCarousel,

			/// <summary>
			/// Reed-Solomon comes from the JavaReedSolomon library (https://github.com/Backblaze/JavaReedSolomon) that has been compiled into a .DLL using IKVMC (http://www.ikvm.net/userguide/ikvmc.html)
			/// </summary>
			ReedSolomon
		}

		static void Main(string[] args)
		{
			Console.WriteLine("This program simulates the performance of a set of fountain codes. The results of the simulation are written to \"output.csv\" on your desktop.");
			Console.WriteLine();

			var dataSizeMin = 2;
			var dataSizeMax = 311; // The maximum precomputed primitive polynomial that I could find without missing smaller primitive polynomials is 311. Primitive polynomials are used in the SophisticatedCarousel implementation
			var delta = 1e-6; // The delta parameter of the Robust Soliton Distribution that's used in the LT Codes implementation
			var expectedRippleSize = 2; // The R parameter of the Robust Soliton Distribution that's used in the LT Codes implementation and in the Special LT Codes implementation
			var random = new ThreadSafeRandom(); // Provides thread-safe access to a quick RNG (since .Net's Random class isn't thread-safe)
			var symbolSize = 1; // The number of bytes that go into a single symbol. This shouldn't change the simulation results, but higher numbers will make everything slower
			using (var outputStream = File.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "output.csv"), FileMode.Append, FileAccess.Write, FileShare.Read)) // Append to the output file
			{
				using (var writer = new StreamWriter(outputStream)) // Wrap the output stream in a writer object
				{
					writer.AutoFlush = true; // Make sure everything is written out to the file as soon as possible

					// Make sure there's a file header
					if (outputStream.Position == 0)
					{
						// This file doesn't already have any contents. So it doesn't have a header. So let's add one
						writer.WriteLine(@"""k"",""p"",""test"",""n"",""generation-complexity"",""solution-complexity""");
					}

					// Set up the test method for some given parameters
					var test = new Func<byte[], double, Test, Tuple<byte[], int, int, int>>((data, erasureProbability, type) =>
						{
							switch (type)
							{
								case Test.RaptorQ:
									{
										// Set up an encoder and decoder
										var parameters = FECParameters.newParameters(
											// long dataLen, int symbSize, int numSrcBs
											data.Length,
											symbolSize,
											1 // Let's use just a single source block for encoding and decoding
										);
										var encoder = OpenRQ.newEncoder(
											data,
											parameters
											);
										var encoderSourceBlock = encoder.sourceBlock(0);
										var decoder = OpenRQ.newDecoderWithZeroOverhead(parameters);
										var decoderSourceBlock = decoder.sourceBlock(0);

										// Run the data through an erasure channel until it's decoded
										var numPacketsReceived = 0;
										for (var packetID = 0; packetID <= 16777216; packetID++) // 16777216 seems to be a magic number of the maximum number of encoding symbols that can be generated from RaptorQ
										{
											// Generate an encoding symbol packet
											var packet = encoderSourceBlock.encodingPacket(packetID);

											// Erase it if the probability is right
											if (random.NextDouble() <= erasureProbability)
											{ // This packet should get erased
												continue; // Skip to generating the next packet
											}
											else
											{ // This packet will be received
												numPacketsReceived++;
											}

											// Otherwise, try decoding it
											var result = decoderSourceBlock.putEncodingPacket(packet);
											if (result == SourceBlockState.DECODED)
											{
												// Decoding was successful. Return the results
												var decodedData = decoder.dataArray();
												return new Tuple<byte[], int, int, int>(decodedData, numPacketsReceived, 0, 0);
											}
										}
										return new Tuple<byte[], int, int, int>(data, -1, 0, 0); // Indicate failure because not enough encoding symbols were able to be generated
									}
								case Test.ReedSolomon:
									{
										// Set up the erasure flags for a RS encoder. Note that the overhead of this RS encoder is dynamically sized according to the number of symbols that will be erased
										var existingSymbols = new List<bool>();
										var numExisting = 0;
										while (numExisting < data.Length)
										{
											var exists = (random.NextDouble() > erasureProbability); // Adds "true" if a uniformly-random number is higher than the erasure probability
											existingSymbols.Add(exists);
											if (exists)
												numExisting++;
										}
										if (existingSymbols.Count > 256) // RS can only handle blocks of up to 256 symbols (erasure plus data)
											return new Tuple<byte[], int, int, int>(data, -1, 0, 0); // Return a null result (meaning RS can't handle this case)

										// Set up an RS encoder
										var encoder = ReedSolomon.create(data.Length, existingSymbols.Count - data.Length);
										var shards = new byte[existingSymbols.Count][];
										for (var i = 0; i < data.Length; i++)
										{
											shards[i] = new byte[] { data[i] }; // A byte from the original data
										}
										for (var i = data.Length; i < shards.Length; i++)
										{
											shards[i] = new byte[1]; // An empty byte. This shard will get populated with parity in a little bit
										}
										encoder.encodeParity(shards, 0, 1); // Populate parity blocks

										// Now erase the blocks that we marked as getting erased
										for (var i = 0; i < shards.Length; i++)
										{
											if (!existingSymbols[i])
												shards[i] = new byte[1]; // Erase this symbol
										}

										// Now decode the RS code after some symbols have been erased
										encoder.decodeMissing(shards, existingSymbols.ToArray(), 0, 1);

										// Pull out the decoded data and return the results
										var decodedData = new byte[data.Length];
										for (var i = 0; i < decodedData.Length; i++)
										{
											decodedData[i] = shards[i][0];
										}
										return new Tuple<byte[], int, int, int>(decodedData, numExisting, 0, 0);
									}
								case Test.Carousel:
								case Test.SophisticatedCarousel:
								case Test.LTCode:
								case Test.RandomSubset:
								case Test.SpecialLTCode:
									{
										// Set up the implementation for this type of fountain code
										IFountainCodeImplementation implementation;
										switch (type)
										{
											case Test.Carousel:
												implementation = new Carousel(data.Length);
												break;
											case Test.SophisticatedCarousel:
												implementation = new SophisticatedCarousel(data.Length);
												break;
											case Test.LTCode:
												implementation = new LubyTransform(random, data.Length, expectedRippleSize, delta);
												break;
											case Test.RandomSubset:
												implementation = new RandomSubset(random, data.Length);
												break;
											case Test.SpecialLTCode:
												implementation = new SpecialLubyTransform(random, data.Length, expectedRippleSize);
												break;
											default:
												throw new NotImplementedException();
										}

										// Set up the data as a symbol array
										var symbolArray = new Symbol<byte>[data.Length];
										for (var i = 0; i < symbolArray.Length; i++)
										{
											symbolArray[i] = new Symbol<byte>(new byte[] { data[i] });
										}

										// Set up the sender and receiver
										var sender = new Sender(symbolArray, implementation);
										var receiver = new Receiver(data.Length, 1, 0);

										// Send packets through an erasure channel until the message is decoded
										var numPacketsReceived = 0;
										var generationComplexity = 0;
										for (var packetID = 0; ; packetID++)
										{
											// Generate an encoding packet
											var packetComplexity = 0;
											var packet = sender.GenerateNext(ref packetComplexity);
											var coefficients = packet.Item1;
											var symbol = packet.Item2;

											// See if it should get erased
											if (random.NextDouble() <= erasureProbability)
											{ // This packet should get erasued
												continue;
											}
											else
											{ // This packet will get processed
												numPacketsReceived++;
												generationComplexity += packetComplexity;
												if (numPacketsReceived > data.Length * 10 + 1000) // Even the carousel method only needs ~4-5x as many symbols for very high erasure rates, so 10x + 1000 should really be too much
													throw new Exception("It seems that this implementation of fountain code is never going to be able to solve for the original data under these circumstances, because the number of received symbols is unreasonably high");
											}

											// Process the packet since it'll get received
											var solutionComplexity = 0;
											var result = receiver.Solve(coefficients, symbol, ref solutionComplexity);
											if (result != null)
											{ // Enough symbols have been received
												// Turn the decoded result into a byte array
												var decodedData = new byte[data.Length];
												for (var i = 0; i < decodedData.Length; i++)
												{
													decodedData[i] = result[i].Data[0];
												}

												// Return the results
												return new Tuple<byte[], int, int, int>(decodedData, numPacketsReceived, generationComplexity, solutionComplexity);
											}
										}
									}
								default:
									throw new NotImplementedException();
							}
						});

					// Continually run tests and record the results
					for (var processor = 0; processor < Environment.ProcessorCount; processor++)
					{
						Task.Run(() => // Start a test thread once for each processor
							{
								while (true)
								{
									// Set up test parameters
									var data = new byte[random.Next(dataSizeMin, dataSizeMax)]; // Pick some random data
									random.NextBytes(data);
									var erasureProbability = random.NextDouble(); // Pick a random erasure probability

									// Run the configured test for each of the test types using the same parameters
									foreach (Test testType in Enum.GetValues(typeof(Test)))
									{
										// Run the test for this test type and these parameters
										var results = test(data, erasureProbability, testType);
										var decodedData = results.Item1;
										var numReceivedPackets = results.Item2;
										var generationComplexity = results.Item3;
										var solutionComplexity = results.Item4;

										// Verify that the received results are identical to the original data
										if (decodedData.Length != data.Length)
											throw new Exception("The data coming out of the test is a different length than the original data");
										for (var i = 0; i < data.Length; i++)
										{
											if (data[i] != decodedData[i])
												throw new Exception("The data coming out of the test is different than the original data");
										}

										// Record this result to file (k,p,test,n)
										var outputLine = string.Join(",",
											data.Length,
											erasureProbability,
											testType.ToString(),
											numReceivedPackets,
											generationComplexity,
											solutionComplexity
											);
										lock (writer)
										{
											writer.WriteLine(outputLine);
											Console.WriteLine(outputLine);
										}
									}
								}
							});
					}

					// Eat input so that the main thread never quits
					while (true)
						Console.ReadKey(true);
				}
			}
		}
	}
}
