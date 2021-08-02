using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;

using Common;

namespace KeepInstrumentConnection
{
	/// <summary>
	/// This class demonstrates on how to observe a connection to an attached instrument by using
	/// event handlers.
	/// </summary>
	class EventDriven
	{
		/// <summary>
		/// Use event handlers to check the connection. Unplug the instrument network cable, reboot it or shutdown the
		/// core Service for a short time to see an effect.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				Console.WriteLine("Waiting 60 seconds for connection changes using event listening...");

				// Check the connection initially (after setting up the handler for race conditions), but let the API announce connection changes
				instrument.ConnectionChanged += Instrument_ConnectionChanged;
				Thread.CurrentThread.Join(60000);
				instrument.ConnectionChanged -= Instrument_ConnectionChanged;
				Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + instrument.Connected);
			}
		}

		/// <summary>
		/// Callback when the instrument connection changes: We just do a printout, but we could start anything.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Instrument_ConnectionChanged(object sender, EventArgs e)
		{
			IExplorisInstrumentAccess instrument = (IExplorisInstrumentAccess) sender;
			Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + instrument.Connected);
		}
	}
}
