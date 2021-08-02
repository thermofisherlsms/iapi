using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Workflow
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// An API license is required!
		/// </para>
		/// <para>
		/// It demonstrates how to start an acquisition or change a mode (On, Off, Standby).
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string arg1 = (args.Length >= 1) ? args[0] : "";
			if (arg1.Equals("ModeOnStandbyOn", StringComparison.OrdinalIgnoreCase) &&
				(args.Length == 1))
			{
				Console.WriteLine("Testing functionality ModeOnStandbyOn");
				Console.WriteLine();
				new ModeOnStandbyOn().DoJob();

				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
				return;
			}

			if (arg1.Equals("AcquisitionByTime", StringComparison.OrdinalIgnoreCase) &&
				(args.Length == 2))
			{
				double secs;
				NumberFormatInfo lang = NumberFormatInfo.CurrentInfo;
				if (args[1].Contains(NumberFormatInfo.InvariantInfo.NumberDecimalSeparator))
				{
					// don't get fooled by a wrongly understood NumberGroupSeparator
					lang = NumberFormatInfo.InvariantInfo;
				}
				if (double.TryParse(args[1], NumberStyles.Float, lang, out secs))
				{
					Console.WriteLine("Testing functionality AcquisitionByTime with {0} seconds", secs);
					Console.WriteLine();
					new AcquisitionByTime().DoJob(secs);

					Console.WriteLine("Press any key to continue...");
					Console.ReadKey();
					return;
				}
				Console.WriteLine("Specification of seconds not understood.");
			}

			if (arg1.Equals("AcquisitionByMethod", StringComparison.OrdinalIgnoreCase) &&
				(args.Length == 2))
			{
				if (File.Exists(args[1]))
				{
					Console.WriteLine("Testing functionality AcquisitionByMethod with method file {0}", args[1]);
					Console.WriteLine();
					new AcquisitionByMethod().DoJob(args[1]);

					Console.WriteLine("Press any key to continue...");
					Console.ReadKey();
					return;
				}
				Console.WriteLine("{0} is not a file", args[1]);
			}

			Console.WriteLine("usage: {0} [ModeOnStandbyOn | AcquisitionByTime < seconds> | AcquisitionByMethod <filename>]", Process.GetCurrentProcess().ProcessName);
			Console.WriteLine("Defaulting to ModeOnStandbyOn");
			Console.WriteLine();
			new ModeOnStandbyOn().DoJob();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
