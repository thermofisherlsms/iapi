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

//#define ProfileAccessInsideEnumeration
//#define ProfileAccessOutsideEnumeration
using System;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using IMsScan = Thermo.Interfaces.InstrumentAccess_V2.MsScanContainer.IMsScan;

namespace DataListening
{
	/// <summary>
	/// This example shows the ways of accessing profile peaks inside a spectrum.
	/// For Orbitrap systems, profiles are of rare use in most cases. In doubt, prefer use of centroids only. 
	/// </summary>
	class ProfileDataReceiver
	{
		internal ProfileDataReceiver() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
				Console.WriteLine("Waiting 60 seconds for scans on detector " + orbitrap.DetectorClass + ", showing profiles...");

				orbitrap.MsScanArrived += Orbitrap_MsScanArrived;
				Thread.CurrentThread.Join(60000);
				orbitrap.MsScanArrived -= Orbitrap_MsScanArrived;
			}
		}

		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			// If examining code takes longer, in particular for some scans, it is wise
			// to use a processing queue in order to get the system as responsive as possible.

			using (IMsScan scan = (IMsScan) e.GetScan())    // caution! You must dispose this, or you block shared memory!
			{
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);
#if ProfileAccessInsideEnumeration
				int max = 5;
				foreach (ICentroid c in scan.Centroids)
				{
					if (max-- == 0)
						break;
					Console.Write("{0:F4},{1:0.0e0},z={2}  :  ", c.Mz, c.Intensity, c.Charge);

					// Access to profile information is done INSIDE the enumeration of centroids: profile is available:
					Console.WriteLine(string.Join("; ", c.Profile.Take(5).Select(n => string.Format("{0:F4},{1:0.0e0}", n.Mz, n.Intensity))));
				}
#elif ProfileAccessOutsideEnumeration
				ICentroid[] list = scan.Centroids.ToArray();
				for (int i = 0; i < Math.Min(5, list.Length); i++)
				{
					ICentroid c = list[i];
					Console.Write("{0:F4},{1:0.0e0},z={2}  :  ", c.Mz, c.Intensity, c.Charge);	// works

					// Access to profile information is done OUTSIDE the enumeration of centroids, which is done by ToArray(), no profile is available:
					Console.WriteLine(string.Join("; ", c.Profile.Take(5).Select(n => string.Format("{0:F4},{1:0.0e0}", n.Mz, n.Intensity)))); // crashes
				}
#else
				// Create an array where the profile is copied on enumeration if the above solution with ProfileAccessInsideEnumeration
				// does not fit. The way of processing shown here requies more memory and is slower.
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
