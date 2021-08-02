using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace DataListening
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// It demonstrates how to use Exploris API to get access to generated data, the so called scans.
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string arg1 = (args.Length == 1) ? args[0] : "";
			if (arg1.Equals("Data", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality Data");
				Console.WriteLine();
				new DataReceiver().DoJob();
			}
			else if (arg1.Equals("EasyData", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality EasyData");
				Console.WriteLine();
				new EasyDataReceiver().DoJob();
			}
			else if (arg1.Equals("ProfileData", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality ProfileData");
				Console.WriteLine();
				new ProfileDataReceiver().DoJob();
			}
			else
			{
				{
					Console.WriteLine("usage: {0} [Data | EasyData | ProfileData]", Process.GetCurrentProcess().ProcessName);
					Console.WriteLine("Defaulting to EasyData");
					Console.WriteLine();
					new EasyDataReceiver().DoJob();
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
