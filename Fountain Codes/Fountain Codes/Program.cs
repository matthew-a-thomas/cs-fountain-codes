using Library;
using Library.FountainCodeImplementations;
using Library.Input;
using Library.Randomness;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fountain_codes
{
    internal class Program
	{
	    private static void Main()
		{
			if (Console.WindowWidth > 80)
				Console.WindowWidth = 80;
			Console.BufferWidth = 80;
			Console.BufferHeight = 3000;

			// Print introduction
			Console.WriteLine(
@"This program demonstrates a few related types of fountain codes. A fountain
code encodes a message in a way that there's no limit to how many encoded
symbols can be created, and only a small portion of the generated symbols are
needed to reconstruct the original message, and any portion of the generated
symbols will do.

Additionally, this program visually demonstrates how the different fountain
codes work by sending them through a packet erasure channel. An erasure channel
just means there is a certain probability that information getting sent through
it gets completely lost.

LUBY TRANSFORM
===============================================================================
The Luby Transform code makes each encoding symbol by XOR'ing together a
subset of the message.

The number of elements in each subset is chosen from a particular kind of
random distribution called the Robust Soliton Distribution.

This distribution takes two parameters: the expected ripple size, and delta.

The expected ripple size is how many symbols one would expect to be released by
receiving an encoding packet. Basically this increases the probability that
(data length)/R symbols are XOR'd together. R=2 seems to work well.

Delta is the probability that a random walk of length k deviates from its mean
by more than ln(k/delta)*sqrt(k). In other words, a smaller delta makes the
underlying Robust Soliton Distribution more robust, so a smaller number is
better.

Luby Transform codes were introduced by Michael Luby.

SPECIAL LUBY TRANSFORM
===============================================================================
The ""special"" Luby Transform is the limit of the Luby Transform code as
delta approaches zero. In other words, there's a 100% chance that the number
of symbols XOR'd together is (data length)/R.

RANDOM SUBSET
===============================================================================
The random subset fountain code randomly XOR's together parts of the message.
Each part of the message has an equal probability of being included, and there
will always be at least one part included.

CAROUSEL
===============================================================================
The carousel fountain code just cycles through each part of the message. It
repeats as long as is necessary.

It isn't really technically a fountain code because not every subset of a
particular size will let you reconstruct the original message. For example, if
the message is two characters long, one could have an infinitely-large subset
of encoding symbols by capturing every other generated symbol, but would never
be able to get the original message. But even though this isn't a fountain code
it is still good for comparison.

");

			var random = new ThreadSafeRandom();
			while (true)
			{
				// Set up parameters for sending encoding packets
				var data = new List<Symbol<byte>>();
				foreach (var b in Encoding.ASCII.GetBytes(ConsoleUtil.Prompt("What message would you like to send?")))
				{
					data.Add(new Symbol<byte>(new[] { b })); // Make each original data symbol a single byte big
				}
				var erasureProbability = double.Parse(ConsoleUtil.Prompt("Packet erasure probability? [0-1)"));
				IFountainCodeImplementation implementation;
				switch (ConsoleUtil.Choose("Which fountain code implementation?", "Luby Transform", "Special Luby Transform", "Random Subset", "Sophisticated carousel", "Carousel"))
				{
					case 0: // Luby Transform
						implementation = new LubyTransform(random, data.Count, int.Parse(ConsoleUtil.Prompt("Expected ripple size? [1-?]")), double.Parse(ConsoleUtil.Prompt("Delta? (0-1]")));
						break;
					case 1: // Special Luby Transform
						implementation = new SpecialLubyTransform(random, data.Count, int.Parse(ConsoleUtil.Prompt("Expected ripple size? [2-?]")));
						break;
					case 2: // Random subset
						implementation = new RandomSubset(random, data.Count);
						break;
					case 3: // Sophisticated carousel
						implementation = new SophisticatedCarousel(data.Count);
						break;
					case 4: // Carousel
						implementation = new Carousel(data.Count);
						break;
					default:
						throw new NotImplementedException();
				}

				// Set up a sender
				var sender = new Sender(data.ToArray(), implementation);

				// Set up a receiver
				var receiver = new Receiver(data.Count, 0);

				// Send packets through the simulated erasure channel
			    var numReceived = 0;
				while (true)
				{
					// Generate a packet
					var complexity = 0;
					var packet = sender.GenerateNext(ref complexity);
					var coefficients = packet.Item1;
					var symbol = packet.Item2;
					var character = GetCharacter(symbol);

					// See if we should drop/erase it
					if (random.NextDouble() <= erasureProbability)
					{ // Erase this packet
						ConsoleUtil.Write(character, ConsoleColor.DarkRed, ConsoleColor.Black);
					}
					else
					{ // Don't erase the packet
						numReceived++;

						// Print out the symbol that's being sent to the receiver
					    ConsoleUtil.Write(character, ConsoleColor.Black, ConsoleColor.DarkGreen);

						// Give this packet to the receiver
						var result = receiver.Solve(coefficients, symbol, ref complexity);

						// See if the receiver has decoded everything yet
					    if (result == null)
                            continue; // The receiver has decoded everything
					    Console.WriteLine();
					    Console.WriteLine($"Decoded everything after receiving {numReceived} packets\n({(double) numReceived / (double) data.Count:.##} times as many as the message length; {numReceived - data.Count} additional):");
					    foreach (var part in result)
					    {
					        ConsoleUtil.Write((char)part.Data[0], ConsoleColor.DarkGreen, ConsoleColor.Black);
					    }
					    Console.WriteLine();

					    break; // Stop looping
					}
				}
				Console.WriteLine();
			}
		}

	    private static char GetCharacter(Symbol<byte> symbol)
	    {
	        char character;
	        if (symbol.Data[0] < 32 || symbol.Data[0] > 127)
	            character = '?'; // It's a non-printable character
	        else
	            character = (char) symbol.Data[0];
	        return character;
	    }
	}
}
