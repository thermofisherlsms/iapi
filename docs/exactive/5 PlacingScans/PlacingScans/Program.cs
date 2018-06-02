using System;
using System.Globalization;
using System.Threading;

namespace PlacingScans
{
	/// <summary>
	/// The program allows to place individual scans.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Bring the system to On mode and/or start an acquisition to place scans.");

			new CustomScansTandemByArrival().DoJob();
			new CustomScansTandemByCallback().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
