using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace PlacingScans
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// An API license is required!
		/// </para>
		/// <para>
		/// It demonstrates how to use Exploris API to place individual scans when the system is in On mode.
		/// It works also when a method is running. The system selects the next scan to place in this
		/// order:
		/// <list type="number">
		/// <item>custom scan</item>
		/// <item>repeating scan</item>
		/// <item>scan defined in method</item>
		/// <item>scan defined in tune program</item>
		/// </list>
		/// The system takes the first one defined in the order. A custom scan can be defined without having
		/// a repeating scan, etc.
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string arg1 = (args.Length == 1) ? args[0] : "";
			if (arg1.Equals("TandemByArrival", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality TandemByArrival");
				Console.WriteLine();
				new CustomScansTandemByArrival().DoJob();
			}
			else if (arg1.Equals("TandemByCallback", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality TandemByCallback");
				Console.WriteLine();
				new CustomScansTandemByCallback().DoJob();
			}
			else
			{
				{
					Console.WriteLine("usage: {0} [TandemByArrival | TandemByCallback]", Process.GetCurrentProcess().ProcessName);
					Console.WriteLine("Defaulting to TandemByCallback");
					Console.WriteLine();
					new CustomScansTandemByCallback().DoJob();
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
