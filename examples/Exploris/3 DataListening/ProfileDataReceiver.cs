using System;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;

using Common;

namespace DataListening
{
	/// <summary>
	/// This class gathers data for one minute and shows some scan data, but still no meta information.
	/// Specifically, it shows inline the problems with accessing profile peaks.
	/// </summary>
	class ProfileDataReceiver
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
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + ", showing profiles...");

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

				// For the sake of maximum speed profile data is only available when having access to the corresponding centroid.
				//
				// In general, profiles are accessed without real need and without knowledge about its use. It heavily depends
				// on the detector and the way on how centroids are calculated and later corrected.

#if ProfileAccessInsideEnumeration
				int max = 5;
				foreach (ICentroid c in scan.Centroids)
				{
					if (max-- == 0)
						break;
					// This works, but is unhandy in most cases.
					Console.Write("{0:F4},{1:0.0e0},z={2}  :  ", c.Mz, c.Intensity, c.Charge);
					Console.WriteLine(string.Join("; ", c.Profile.Take(5).Select(n => string.Format("{0:F4},{1:0.0e0}", n.Mz, n.Intensity))));
				}
#elif ProfileAccessOutsideEnumeration
				ICentroid[] list = scan.Centroids.ToArray();
				for (int i = 0; i < Math.Min(5, list.Length); i++)
				{
					ICentroid c = list[i];
					Console.Write("{0:F4},{1:0.0e0},z={2}  :  ", c.Mz, c.Intensity, c.Charge);	// works
					Console.WriteLine(string.Join("; ", c.Profile.Take(5).Select(n => string.Format("{0:F4},{1:0.0e0}", n.Mz, n.Intensity)))); // crashes
				}
#else
				// Create an array where the profile is copied on enumeration
				Tuple<ICentroid,IMassIntensity[]>[] list = scan.Centroids.Select(n => new Tuple<ICentroid, IMassIntensity[]>(n, (IMassIntensity[]) n.Profile.Clone())).ToArray();
				for (int i = 0; i < Math.Min(5, list.Length); i++)
				{
					Tuple<ICentroid, IMassIntensity[]> tuple = list[i];
					Console.Write("{0:F4},{1:0.0e0},z={2}  :  ", tuple.Item1.Mz, tuple.Item1.Intensity, tuple.Item1.Charge);  // works
					Console.WriteLine(string.Join("; ", tuple.Item2.Take(5).Select(n => string.Format("{0:F4},{1:0.0e0}", n.Mz, n.Intensity)))); // works
				}
#endif
			}
		}
	}
}
