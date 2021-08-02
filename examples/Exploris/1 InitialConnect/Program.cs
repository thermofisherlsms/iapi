using System;

using Thermo.Interfaces.ExplorisAccess_V1;

namespace InitialConnect
{
	class Program
	{
		/// <summary>
		/// This program is a console mode program running under .NET 4.6 and maybe even older.
		/// <para>
		/// It demonstrates how to access an API instance for a Exploris style instrument.
		/// </para>
		/// </summary>
		/// <param name="args">ignored</param>
		static void Main(string[] args)
		{
			// In opposite to Exactive, the container is disposable, not the instrument
			using (IExplorisInstrumentAccessContainer container = Connect.GetApiInstance())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);
				// we just print the instrument name to proof success
				Console.WriteLine(instrument.InstrumentName);
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}
	}
}
