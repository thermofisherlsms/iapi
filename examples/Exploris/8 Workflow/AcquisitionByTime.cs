using System;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

using Common;

namespace Workflow
{
	/// <summary>
	/// This class demonstrates execution of an acquisition with current settings for a period of time.
	/// Observe in Tune or by other example programs what is executed.
	/// </summary>
	class AcquisitionByTime
	{
		internal AcquisitionByTime() { }

		/// <summary>
		/// Bring the system to mode "On" first.
		/// </summary>
		/// <param name="secs">Duration of the acquisition</param>
		internal void DoJob(double secs)
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				IExplorisAcquisition workflow = instrument.Control.Acquisition;
				// Make sure we have a connection (also) to the service, because the service maintains the locks.
				// Alternatively, wait until container has its property ServiceConnected set to true.
				// Lock breaks on service(!) disconnect and placing scans will then also not be possible.
				Console.WriteLine("Waiting 60 seconds for the instrument to enter On mode...");
				if (!workflow.WaitFor(TimeSpan.FromSeconds(60), n => n.SystemMode == SystemMode.On))
				{
					return;
				}

				// An API license is required for this. Wait until all licenses are transferred.
				if (!Connection.WaitForApiLicense(container, instrument, TimeSpan.FromSeconds(2)))
				{
					Console.Error.WriteLine("No connection to the core Service or missing an API license.");
					Environment.Exit(1);
				}

				Console.WriteLine("starting a {0} second acquisition...", secs);
				workflow.StartAcquisition(workflow.CreateAcquisitionLimitedByDuration(TimeSpan.FromSeconds(secs)));
			}
		}
	}
}
