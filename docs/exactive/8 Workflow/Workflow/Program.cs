using System;
using System.Globalization;
using System.Threading;

namespace Workflow
{
	/// <summary>
	/// This example shows how to bring the system into a specific mode (e.g. "On"), to start an acquisition by
	/// time or by method. An LC link is not supported.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Start Tune to get feedback, bring instrument into Standby mode, and follow instructions here.");

			new OnStandbyOn().DoJob();
			new AcquisitionByTime().DoJob();
			new RunningMethod().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
