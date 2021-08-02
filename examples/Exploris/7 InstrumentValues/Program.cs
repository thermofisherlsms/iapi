using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace InstrumentValues
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// An API license is required!
		/// </para>
		/// <para>
		/// It demonstrates how to use Exploris API to set node values or start a (mass) calibration.
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string arg1 = (args.Length == 1) ? args[0] : "";
			if (arg1.Equals("Value", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality Value");
				Console.WriteLine();
				new ValueTest().DoJob();
			}
			else if (arg1.Equals("MassCalibration", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Testing functionality MassCalibration");
				Console.WriteLine();
				new MassCalibration().DoJob();
			}
			else
			{
				{
					Console.WriteLine("usage: {0} [Value | MassCalibration]", Process.GetCurrentProcess().ProcessName);
					Console.WriteLine("Defaulting to MassCalibration");
					Console.WriteLine();
					new MassCalibration().DoJob();
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
