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
	/// <para>
	/// This class is easier to be understood if <see cref="CustomScansTandemByArrival"/> has been read first.
	/// </para>
	/// </summary>
	class CustomScansTandemByCallback
	{
		int m_scanId = 1;   // must be != 0
		bool m_initialCondition = true;
		IScans m_scans = null;

		/// <summary>
		/// Bring the system to mode "On" to place scans. This functionality also works when a method runs.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				// Make sure we have a connection to the service, because the service maintains the locks.
				// Alternatively, wait until container has its property ServiceConnected set to true.
				// Lock breaks on service disconnect and placing scans will then also not be possible.
				Console.WriteLine("Waiting 60 seconds for a connection to the instrument...");
				if (!instrument.Control.Acquisition.WaitFor(TimeSpan.FromSeconds(60), n => (n.SystemMode != SystemMode.Disconnected) && (n.SystemMode != SystemMode.Maintenance)))
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
					WaitFor(m_scans);
					IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
					Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + "...");

					orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
					m_scans.CanAcceptNextCustomScan += Scans_CanAcceptNextCustomScan;
					Thread.CurrentThread.Join(60000);
					orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
					m_scans.CanAcceptNextCustomScan -= Scans_CanAcceptNextCustomScan;
				}
			}
		}

		/// <summary>
		/// Wait up to 10 seconds for the parameter set becomes available before we can use it. The parameter set
		/// offers all settings for a single scan. Parameters are quite constant, but may change on a significant change
		/// in hardware (e.g. a different ion source) or similar settings.
		/// <para>
		/// It is typically safe to assume that parameters stay constant when an acquisition has started until the end of it.
		/// </para>
		/// </summary>
		/// <param name="scans">The interface providing access to those parameters.</param>
		private void WaitFor(IScans scans)
		{
			Console.WriteLine("{0:HH:mm:ss,fff} waiting for IScans interface to become ready", DateTime.Now);
			ManualResetEvent wait = new ManualResetEvent(false);

			EventHandler handler = (sender,e) => { wait.Set(); };
			scans.PossibleParametersChanged += handler;
			if ((scans.PossibleParameters != null) && (scans.PossibleParameters.Length > 0))
			{
				wait.Set();
			}
			wait.WaitOne(10000);
			scans.PossibleParametersChanged -= handler;

			if ((scans.PossibleParameters == null) || (scans.PossibleParameters.Length == 0))
			{
				throw new TimeoutException("Not connected to the instrument or something else happened.");
			}
			Console.WriteLine("{0:HH:mm:ss,fff} end of wait", DateTime.Now);
		}

		/// <summary>
		/// Callback when the system is capable to place a new scan: This is one of the most tricky parts in API and also one
		/// that is prone to wrong timings and wrong execution.
		/// <para>
		/// In principle, it is possible to preload the queue of scans to execute with as many scans as wanted, maybe even thousands.
		/// Under real conditions, this rarely makes sense due to changes in execution latency and LC conditions.
		/// </para>
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Scans_CanAcceptNextCustomScan(object sender, EventArgs e)
		{
			// This event will be thrown when a CUSTOM scan just has been selected as next scan to execute. Other scans may still
			// be executed that are not custom scans. See the explanations below within Orbitrap_MsScanArrived.
			//
			// In principle, it should be possible to have an uninterrupted set of custon scans using CanAcceptNextCustomScan,
			// but for having results or a particular scan interpreted, one can use SingleProcessingDelay explained further
			// down.
			//
			// Typically, for Orbitrap scans, there is a short period of time (~10ms) during which a further custom scan can
			// be placed without having an interruption of custom scans. But due to latency it is possible that other scans
			// are executed from time to time. This can be addressed by a short value for SingleProcessingDelay
			// explained in PlaceScan below.
			Console.WriteLine("{0:HH:mm:ss,fff} CanAcceptNextCustomScan", DateTime.Now);
			PlaceScan();
		}

		/// <summary>
		/// Callback when a scan was generated: We show all arrived scans. We also start placing new scans.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			string accessId;
			using (IMsScan scan = (IMsScan) e.GetScan())    // caution! You must dispose this, or you block shared memory!
			{
				scan.Trailer.TryGetValue("Access Id:", out accessId);   // -1 indicates it is a system-generated scan
				Console.WriteLine("{0:HH:mm:ss,fff} scan {1} arrived", DateTime.Now, accessId);
				if (m_initialCondition)
				{
					m_initialCondition = false;
					PlaceScan();

					// Placing a new scan doesn't mean that the result of this will arrive next. There may still be a scan
					// being processed by FT, one may be in the Orbitrap detector, one may be gathering ions.
					//
					// Own-placed scans will come in row if placed in one bunch or when CanAcceptNextCustomScan is
					// utilized (see above), maybe combined with SingleProcessingDelay (see below)
				}
			}
		}

		/// <summary>
		/// Add a custom scan to the system to be executed.
		/// </summary>
		private void PlaceScan()
		{
			if (m_scanId > 10)							// just place 10 scans
			{
				return;
			}
			ICustomScan scan = m_scans.CreateCustomScan();
			scan.RunningNumber = m_scanId++;            // RunningNumber is reflected in "Access Id:", see above

			scan.Values["Polarity"] = "1";
			// Values not listed here are taken from default settings stored in the instrument when changing them in Tune.
			// Note that different value sets exists for positive and negative polarity.

			if (m_scanId == 5)
			{
				// let the instrument wait for a maximum of 10 seconds, which will not happen under normal circumstances.
				scan.SingleProcessingDelay = 10;
			}
			Console.WriteLine("{0:HH:mm:ss,fff} placing scan {1}", DateTime.Now, scan.RunningNumber);
			m_scans.SetCustomScan(scan);
		}
	}
}
