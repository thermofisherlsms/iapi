using System;
using System.Globalization;
using System.Threading;

namespace MetaDataListening
{
	/// <summary>
	/// Show how to access meta information at acquisition start and scan arrival.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Bring the system to On mode and/or start an acquisition to see results.");

			new MetaDataReceiver().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
