using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;

using Common;

namespace KeepInstrumentConnection
{
	/// <summary>
	/// This class demonstrates on how to observe a connection to an attached instrument by polling.
	/// It consumes some CPU power, but is easy to understand.
	/// </summary>
	class Polling
	{
		/// <summary>
		/// Use polling to check the connection. Unplug the instrument network cable, reboot it or shutdown the
		/// core Service for a short time to see an effect.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				Console.WriteLine("Waiting 60 seconds for connection changes using polling...");
				DateTime end = DateTime.Now.AddSeconds(60);
				bool connected = instrument.Connected;
				Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + connected);

				// Check every 10 ms for a connection change, 10ms is responsive, and we don't eat much time here.
				while (end > DateTime.Now)
				{
					Thread.CurrentThread.Join(10);
					bool c = instrument.Connected;
					if (c != connected)
					{
						connected = c;
						Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + connected);
					}
				}
			}
		}
	}
}
