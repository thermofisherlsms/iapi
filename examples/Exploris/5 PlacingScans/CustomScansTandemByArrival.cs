using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

using Common;

namespace PlacingScans
{
	/// <summary>
	/// This class places scans when the previously started scan arrived. Observe all scans started by the instrument from
	/// other scan sources, see <see cref="Program.Main"/>. This happens because at start, at end, and maybe in between,
	/// the queue of scans to be executed is not always filled.
	/// </summary>
	class CustomScansTandemByArrival
	{
		int m_scanId = 1;   // must be != 0
		IScans m_scans = null;

		/// <summary>
		/// Bring the system to mode "On" to place scans. This functionality also works when a method runs.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				// Make sure we have a connection (also) to the service, because the service maintains the locks.
				// Alternatively, wait until container has its property ServiceConnected set to true.
				// Lock breaks on service(!) disconnect and placing scans will then also not be possible.
				Console.WriteLine("Waiting 60 seconds for a connection to the instrument...");
				if (!instrument.Control.Acquisition.WaitFor(TimeSpan.FromSeconds(60), n=> (n.SystemMode != SystemMode.Disconnected) && (n.SystemMode != SystemMode.Maintenance)))
				{
					return;
				}

				// An API license is required for this. Wait until all licenses are transferred.
				if (!Connection.WaitForApiLicense(container, instrument, TimeSpan.FromSeconds(2)))
				{
					Console.Error.WriteLine("No connection to the core Service or missing an API license.");
					Environment.Exit(1);
				}

				// Allow concurrent access to this interface. Others may also place scans.
				using (m_scans = instrument.Control.GetScans(false))
				{
					IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
					Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + "...");

					orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
					Thread.CurrentThread.Join(60000);
					orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
				}
			}
		}

		/// <summary>
		/// Callback when a scan was generated: We show all arrived scans. We also place new scans.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			string accessId;
			using (IMsScan scan = (IMsScan) e.GetScan())	// caution! You must dispose this, or you block shared memory!
			{
				scan.Trailer.TryGetValue("Access Id:", out accessId);	// -1 indicates it is a system-generated scan
				Console.WriteLine("{0:HH:mm:ss,fff} scan {1} arrived", DateTime.Now, accessId);
				PlaceScan();
			}
		}

		/// <summary>
		/// Add a custom scan to the system to be executed.
		/// </summary>
		private void PlaceScan()
		{
			if ((m_scanId > 10) ||							// just place 10 scans
				(m_scans.PossibleParameters.Length == 0))	// don't start if it is unknown what properties of a scan can be set
			{
				return;
			}
			ICustomScan scan = m_scans.CreateCustomScan();
			scan.RunningNumber = m_scanId++;				// RunningNumber is reflected in "Access Id:", see above
			
			scan.Values["Polarity"] = "1";
			// Values not listed here are taken from default settings stored in the instrument when changing them in Tune.
			// Note that different value sets exists for positive and negative polarity.

			Console.WriteLine("{0:HH:mm:ss,fff} placing scan {1}", DateTime.Now, scan.RunningNumber);
			Console.WriteLine("sent=" + m_scans.SetCustomScan(scan));
		}
	}
}
