using System;
using System.Globalization;
using System.Threading;

namespace InclusionList
{
	/// <summary>
	/// Demonstrate the exchange of an inclusion list.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Console.WriteLine("Bring the system to On mode and start an acquisition with a PRM experiment to replace the inclusion list.");
			Console.WriteLine("Check the scans later in the generated rawfile.");

			new ReplaceInclList().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
