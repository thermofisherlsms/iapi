using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.ExplorisAccess_V1.MsScanContainer;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

using Common;

namespace DataListening
{
	/// <summary>
	/// This class gathers data for one minute and shows an overview of acquisition and scans.
	/// </summary>
	class DataReceiver
	{
		/// <summary>
		/// Bring the system to mode "On" to see scans coming in. Running a method shows
		/// even more.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				IExplorisMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + "...");

				instrument.Control.Acquisition.AcquisitionStreamOpening += Orbitrap_AcquisitionStreamOpening;
				instrument.Control.Acquisition.AcquisitionStreamClosing += Orbitrap_AcquisitionStreamClosing;
				orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
				Thread.CurrentThread.Join(60000);
				orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
				instrument.Control.Acquisition.AcquisitionStreamClosing -= Orbitrap_AcquisitionStreamClosing;
				instrument.Control.Acquisition.AcquisitionStreamOpening -= Orbitrap_AcquisitionStreamOpening;
			}
		}

		/// <summary>
		/// Callback when a scan was generated: We just notice the event and its contained centroids.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			using (IExplorisMsScan scan = (IExplorisMsScan) e.GetScan())	// caution! You must dispose this, or you block shared memory!
			{
				// We ignore all meta data here. We have tune data, typically coming once per acquisition, we
				// have a status log, providing information every 5 or 10 seconds, and we have trailer data coming
				// with each scan.
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);
			}
		}

		/// <summary>
		/// Callback when an acquisition stream was closed: We just notice the event.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Orbitrap_AcquisitionStreamClosing(object sender, EventArgs e)
		{
			Console.WriteLine("\n{0:HH:mm:ss,fff} {1}", DateTime.Now, "Acquisition stream closed (end of method)");
		}

		/// <summary>
		/// Callback when an acquisition stream was opened: We just notice the event.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter right now, but can be used to examine some initial parameters.</param>
		private void Orbitrap_AcquisitionStreamOpening(object sender, ExplorisAcquisitionOpeningEventArgs e)
		{
			Console.WriteLine("\n{0:HH:mm:ss,fff} {1}", DateTime.Now, "Acquisition stream opens (start of method)");
		}
	}
}
