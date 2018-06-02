using System;
using System.Globalization;
using System.Threading;

namespace InstrumentValues
{
	/// <summary>
	/// Show how to access individual values
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Start Tune and ramp up and down the scan range while HotLink is enabled.");

			new ValueTest().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
