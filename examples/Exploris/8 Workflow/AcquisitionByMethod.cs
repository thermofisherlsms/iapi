using System;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition.Workflow;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

using Common;

namespace Workflow
{
	/// <summary>
	/// This class demonstrates execution of methods.
	/// Observe in Tune or by other example programs what is executed.
	/// </summary>
	class AcquisitionByMethod
	{
		/// <summary>
		/// Bring the system to mode "On" first.
		/// </summary>
		/// <param name="filename">Name of the method file</param>
		internal void DoJob(string filename)
		{
			Console.WriteLine("Output file will be C:\\Xcalibur\\data\\API-Training.raw");

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

				Console.WriteLine("starting method " + filename);
				IAcquisitionMethodRun acquisition = workflow.CreateMethodAcquisition(filename);
				acquisition.RawFileName = "C:\\Xcalibur\\data\\API-Training.raw";
				workflow.StartAcquisition(acquisition);
			}
		}
	}
}
