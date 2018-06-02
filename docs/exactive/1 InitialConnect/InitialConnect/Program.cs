using System;

namespace InitialConnect
{
	/// <summary>
	/// Just connect to an Exactive Series instrument. Standard uses a Registry lookup and Assembly.LoadFrom.
	/// A deprecated way of using COM also exists.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			new Connect().DoJob();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
