using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

using Common;

namespace Workflow
{
	/// <summary>
	/// This class demonstrates mode switches to On, then to Standby, then to On again.
	/// Observe in Tune or by other example programs what is executed.
	/// </summary>
	class ModeOnStandbyOn
	{
		/// <summary>
		/// Bring the system to mode "On", "Off" or "Standby" to see effects.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				IExplorisAcquisition workflow = instrument.Control.Acquisition;
				// Make sure we have a connection (also) to the service, because the service maintains the locks.
				// Alternatively, wait until container has its property ServiceConnected set to true.
				// Lock breaks on service(!) disconnect and placing scans will then also not be possible.
				Console.WriteLine("Waiting 60 seconds for a connection to the instrument...");
				if (!workflow.WaitFor(TimeSpan.FromSeconds(60), n => (n.SystemMode != SystemMode.Disconnected) && (n.SystemMode != SystemMode.Maintenance)))
				{
					return;
				}

				// An API license is required for this. Wait until all licenses are transferred.
				if (!Connection.WaitForApiLicense(container, instrument, TimeSpan.FromSeconds(2)))
				{
					Console.Error.WriteLine("No connection to the core Service or missing an API license.");
					Environment.Exit(1);
				}

				Console.WriteLine("switching to on...");
				workflow.SetMode(workflow.CreateOnMode());

				/* not necessary but helpful for viewing */
				Thread.CurrentThread.Join(500);

				Console.WriteLine("switching to standby...");
				workflow.SetMode(workflow.CreateStandbyMode());

				/* not necessary but helpful for viewing */
				Thread.CurrentThread.Join(500);

				Console.WriteLine("switching to on...");
				workflow.SetMode(workflow.CreateOnMode());
			}
		}
	}
}
