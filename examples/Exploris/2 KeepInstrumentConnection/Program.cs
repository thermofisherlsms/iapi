using System;
using System.Diagnostics;

namespace KeepInstrumentConnection
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// It demonstrates how to use Exploris API to watch out for connection changes.
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			string arg1 = (args.Length == 1) ? args[0] : "";
			if (arg1.Equals("Polling", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality Polling");
				Console.WriteLine();
				new Polling().DoJob();
			}
			else if (arg1.Equals("EventDriven", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality EventDriven");
				Console.WriteLine();
				new EventDriven().DoJob();
			}
			else if (arg1.Equals("Properly", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality Properly");
				Console.WriteLine();
				new Properly().DoJob();
			}
			else
			{
				{
					Console.WriteLine("usage: {0} [Polling | EventDriven | Properly]", Process.GetCurrentProcess().ProcessName);
					Console.WriteLine("Defaulting to Properly");
					Console.WriteLine();
					new Properly().DoJob();
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
