using System;
using System.Globalization;
using System.Threading;

namespace InclusionList
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// An API license is required!
		/// </para>
		/// <para>
		/// It demonstrates how to use Exploris API to replace an inclusion list by another one.
		/// Inclusion lists are used by methods. Hence, for this program to run properly, a method
		/// has to run, and knowledge about the method is necessary.
		/// </para>
		/// </summary>
		/// <param name="args">Arguments from the command line</param>
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			new ReplaceInclList().DoJob();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
