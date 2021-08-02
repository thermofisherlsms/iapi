using System;
using System.Globalization;
using System.Threading;

namespace Apd
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// It demonstrates how to observe APD (Advanced Peak Detection) data. It is wise to read the example
		/// about DataListening first.
		/// </para>
		/// </summary>
		/// <param name="args">ignored</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			new ApdDataReceiver().DoJob();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
