using System;
using System.Text;

namespace Library.Input
{
	/// <summary>
	/// A set of functions related to getting input from the console
	/// </summary>
	public static class ConsoleUtil
	{
		/// <summary>
		/// Prompts the user to pick one option from a set. Returns the zero-based index of the chosen option
		/// </summary>
		/// <param name="title"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static int Choose(string title, params string[] options)
		{
			Console.WriteLine(title);
			for (var i = 0; i < options.Length; i++)
			{
				Console.Write("\t");
				Console.Write(i + 1);
				Console.Write(": ");
				Console.WriteLine(options[i]);
			}
			int choice;
		    // ReSharper disable once EmptyEmbeddedStatement
			while (!int.TryParse(Prompt("Choose an option: [1-" + options.Length + "]"), out choice) || choice < 1 || choice > options.Length) ; // Continue prompting the user for input until they get it right
			return choice - 1; // Remember that they're entering a one-based number while we want to return a zero-based number
		}

		/// <summary>
		/// Prints the given message to console and blocks until the user enters a line, then returns that string
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string Prompt(string message)
		{
			Console.WriteLine(message);
			return Console.ReadLine();
		}

		/// <summary>
		/// Reads a password from the console, printing the given passwordChar in place of plain text
		/// </summary>
		/// <param name="passwordChar">The character to print in place of input. Use '\0' to have no characters printed at all</param>
		/// <returns>The inputted password, or null if ESC was pressed</returns>
		public static string ReadPasswordLine(char passwordChar)
		{
			var builder = new StringBuilder();
			ConsoleKeyInfo key;
			var loop = true;
			while (loop)
			{
				// Read a key
				switch ((key = Console.ReadKey(true)).Key)
				{
					case ConsoleKey.Enter: // They're done typing their password
						loop = false;
						break;
					case ConsoleKey.Backspace: // Backspace one character
						if (builder.Length > 0)
						{
							builder.Remove(builder.Length - 1, 1);
							Console.Write('\b');
							Console.Write(' ');
							Console.Write('\b');
						}
						break;
					case ConsoleKey.Escape: // They want to cancel password input
					    Console.WriteLine('^');
						return null;
					default: // They're typing a password character
						builder.Append(key.KeyChar);
						if (passwordChar != '\0') // Don't print anything if passwordChar is '\0'
						{
							Console.Write(passwordChar);
						}
						break;
				}
			}
			Console.WriteLine();
			return builder.ToString();
		}

		/// <summary>
		/// Writes something out to the console using the specified foreground color
		/// </summary>
		/// <param name="o"></param>
		/// <param name="color"></param>
		public static void Write(object o, ConsoleColor color)
		{
			var startingColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(o.ToString());
			Console.ForegroundColor = startingColor;
		}

		/// <summary>
		/// Writes something out to the console using the specified foreground color, appending a newline at the end
		/// </summary>
		/// <param name="o"></param>
		/// <param name="color"></param>
		public static void WriteLine(object o, ConsoleColor color)
		{
			Write(o.ToString(), color);
			Console.WriteLine();
		}
	}
}
