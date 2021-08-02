using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

using Common;

namespace KeepInstrumentConnection
{
	/// <summary>
	/// This class demonstrates on how to observe a connection to an attached instrument by using
	/// event handlers for both the connection and the current instrument state.
	/// </summary>
	class Properly
	{
		/// <summary>
		/// Use event handlers to check the connection. Unplug the instrument network cable, reboot it or shutdown the
		/// core Service for a short time to see an effect. Also change the run mode or perform an acquisition.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				Console.WriteLine("Waiting 60 seconds for connection changes using event listening on Acquisition.StateChanged ...");

				// Best way is to use instrument.Control.Acquisition.WaitFor or a pure event handler of StateChange,
				// but for demonstration, we display the changes in ConnectionChanged and StateChanged.
				instrument.Control.Acquisition.StateChanged += Acquisition_StateChanged;
				instrument.ConnectionChanged += Instrument_ConnectionChanged;
				Thread.CurrentThread.Join(60000);
				instrument.ConnectionChanged -= Instrument_ConnectionChanged;
				instrument.Control.Acquisition.StateChanged -= Acquisition_StateChanged;
			}
			// See also instrument.Control.Acquisition.WaitFor
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

		/// <summary>
		/// Callback when the instrument state changes: We just do a printout, but we could start anything.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">used to extract the current instrument state</param>
		private void Acquisition_StateChanged(object sender, StateChangedEventArgs e)
		{
			// Much better than acting on the connection state is to act on the instrument state.
			// There should not be a start of something during a calibration or an acquisition.
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + "Instrument mode:         " + e.State.SystemMode);
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + "Instrument system state: " + e.State.SystemState);
		}
	}
}
