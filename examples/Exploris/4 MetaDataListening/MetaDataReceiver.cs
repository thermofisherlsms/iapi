using System;
using System.Collections.Generic;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;

using Common;

namespace MetaDataListening
{
	/// <summary>
	/// This class gathers data for one minute and shows some meta data of scans.
	/// </summary>
	class MetaDataReceiver
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
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + "...");

				instrument.Control.Acquisition.AcquisitionStreamOpening += Orbitrap_AcquisitionStreamOpening;
				orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
				Thread.CurrentThread.Join(60000);
				orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
				instrument.Control.Acquisition.AcquisitionStreamOpening -= Orbitrap_AcquisitionStreamOpening;
			}
		}

		/// <summary>
		/// Callback when a scan was generated: We just notice the event and its contained centroids and the noise polygon.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			using (IMsScan scan = (IMsScan) e.GetScan())	// caution! You must dispose this, or you block shared memory!
			{
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);
				
				// Data of the header is a text dictionary, always being in english formatting, especially for numbers
				Dump("Header", scan.Header);

				// Data of scans is a fixed set of strings for each category, if the configuration of the instrument hasn't changed.

				// Tune data is an ancient term about settings that appear once at acquisition start for the hardware setup and
				// typically include values for source settings and constant values describing the system.
				Dump("TuneData", scan.TuneData);

				// Trailer data are emitted for each scan and include additional values not covered by scan filters.
				Dump("Trailer", scan.Trailer);

				// Status log entries describe the system health and other parameters.
				// They come at fixed time intervalls, e.g. every 10 s.
				Dump("StatusLog", scan.StatusLog);
			}
		}

		/// <summary>
		/// Callback when an acquisition stream was opened: The information passed along with the acquisition start,
		/// which typically is related to a method run, is shown.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter right now, but can be used to examine some initial parameters.</param>
		private void Orbitrap_AcquisitionStreamOpening(object sender, ExplorisAcquisitionOpeningEventArgs e)
		{
			Console.WriteLine("\n{0:HH:mm:ss,fff} Acquisition stream opening", DateTime.Now);
			// Data is a text dictionary, always being in english formatting, especially for numbers
			Dump("Specific", e.StartingInformation);
		}

		/// <summary>
		/// Show the content of a string dictionary on the console.
		/// </summary>
		/// <param name="title">A header line</param>
		/// <param name="container">items to show with some indention</param>
		private void Dump(string title, IDictionary<string, string> container)
		{
			Console.WriteLine(title);
			foreach (KeyValuePair<string, string> info in container)
			{
				Console.WriteLine("   {0,-35} = {1}", info.Key, info.Value);
			}
		}
		/// <summary>
		/// Show the content of an information source on the console. They are related to TuneData, Trailer and StatusLog.
		/// <para>
		/// These are preformatted, the format is transferred invisible at the acquisition start or MS start and will
		/// change during an acquisition.
		/// </para>
		/// <para>
		/// Values are transferred internally as binary data, as example as a double. It is possible to pull data
		/// out of a container both as text (english formatting) or as binary data.
		/// </para>
		/// </summary>
		/// <param name="title">A header line</param>
		/// <param name="container">items to show with some indention</param>
		private void Dump(string title, IInformationSourceAccess container)
		{
			if (!container.Available)
			{
				return;
			}
			Console.WriteLine(title);
			if (!container.Valid)
			{
				Console.WriteLine("   INVALID DATA");
				return;
			}
			
			foreach (string key in container.ItemNames)
			{
				// It is possible to get data as text as shown here or by TryGetRawValue to retrieve the
				// original binary value. This may make sense only for speed or high accuracy reasons and
				// should be used only when that particular value and its type is known.
				string value;
				if (container.TryGetValue(key, out value))
				{
					Console.WriteLine("   {0,-35} = {1}", key, value);
				}
			}
		}
	}
}
