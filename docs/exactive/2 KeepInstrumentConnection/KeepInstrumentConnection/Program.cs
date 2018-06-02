using System;

namespace KeepInstrumentConnection
{
	/// <summary>
	/// Show the ways of checking a connection to the instrument.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			new Polling().DoJob();
			new EventDriven().DoJob();
			new PreferredWay().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
