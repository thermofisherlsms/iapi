#region legal notice
// Copyright(c) 2016 - 2018 Thermo Fisher Scientific - LSMS
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion legal notice
using System;
using System.Threading;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using IMsScan = Thermo.Interfaces.InstrumentAccess_V2.MsScanContainer.IMsScan;

namespace MetaDataListening
{
	/// <summary>
	/// This class demonstrates the way to access extra information coming with an acquisition start or
	/// ar scan arrival.
	/// </summary>
	class MetaDataReceiver
	{
		internal MetaDataReceiver() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + "...");

				orbitrap.AcquisitionStreamOpening += Orbitrap_AcquisitionStreamOpening;
				orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
				Thread.CurrentThread.Join(60000);
				orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
				orbitrap.AcquisitionStreamOpening -= Orbitrap_AcquisitionStreamOpening;
			}
		}

		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			using (IMsScan scan = (IMsScan) e.GetScan())	// caution! You must dispose this, or you block shared memory!
			{
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);

				// The common part is shared by all Thermo Fisher instruments, these settings mainly form the so called filter string
				// which also appears on top of each spectrum in many visualizers.
				Dump("Common", scan.CommonInformation);

				// The specific part is individual for each instrument type. Many values are shared by different Exactive Series models.
				Dump("Specific", scan.SpecificInformation);
			}
		}

		private void Orbitrap_AcquisitionStreamOpening(object sender, MsAcquisitionOpeningEventArgs e)
		{
			Console.WriteLine("\n{0:HH:mm:ss,fff} Acquisition stream opening", DateTime.Now);
			Dump("Specific", e.SpecificInformation);
		}

		private void Dump(string title, IInfoContainer container)
		{
			Console.WriteLine(title);
			foreach (string key in container.Names)
			{
				string value;
				// the source has to be "Unknown" to match all sources. Otherwise, the source has to match.
				// Sources can address values appearing in Tune files, general settings, but also values from
				// status log or additional values to a scan.
				MsScanInformationSource source = MsScanInformationSource.Unknown;   // show everything
				try
				{
					if (container.TryGetValue(key, out value, ref source))
					{
						string descr = source.ToString();
						descr = descr.Substring(0, Math.Min(11, descr.Length));
						Console.WriteLine("   {0,-11} {1,-35} = {2}", descr, key, value);
					}
				}
				catch { /* up to and including 2.8SP1 there is a bug displaying items which are null and if Foundation 3.1SP4 is used with CommonCore */ }
			}
		}
	}
}
