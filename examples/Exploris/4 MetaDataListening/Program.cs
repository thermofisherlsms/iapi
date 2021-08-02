using System;
using System.Globalization;
using System.Threading;

namespace MetaDataListening
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// It demonstrates how to use Exploris API to access scan and acquisition meta information.
		/// </para>
		/// </summary>
		/// <param name="args">ignored</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			new MetaDataReceiver().DoJob();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
