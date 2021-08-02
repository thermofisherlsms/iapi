using System;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

using Common;

namespace DataListening
{
	/// <summary>
	/// This class gathers data for one minute and shows some scan data, but still no meta information.
	/// </summary>
	class EasyDataReceiver
	{
		/// <summary>
		/// Bring the system to mode "On" to see scans coming in.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + ", showing centroids...");

				orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
				Thread.CurrentThread.Join(60000);
				orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
			}
		}

		/// <summary>
		/// Callback when a scan was generated: We just notice the event and its contained centroids and the noise polygon.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			using (IMsScan scan = (IMsScan) e.GetScan())    // caution! You must dispose this, or you block shared memory!
			{
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);
				Console.WriteLine("Noise: " + string.Join("; ", scan.NoiseBand.Take(5).Select(n => string.Format("{0:F2},{1:0.0e0}", n.Mz, n.Intensity))) + " ...");
				Console.WriteLine("Centroids: " + string.Join("; ", scan.Centroids.Take(5).Select(n => string.Format("{0:F2},{1:0.0e0},z={2},R={3}", n.Mz, n.Intensity, n.Charge, n.Resolution))) + " ...");
			}
		}
	}
}
