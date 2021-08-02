using System;
using System.Threading;
using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;
using Thermo.Interfaces.SpectrumFormat_V1;

using Common;

namespace Apd
{
	/// <summary>
	/// This class demonstrates access to APD data.
	/// </summary>
	class ApdDataReceiver
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
		/// Callback when a scan was generated: We just notice the event and print the charge envelope information.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">This contains already the scan. Don't overuse the power of the garbage collector.</param>
		private void Orbitrap_MsScanArrived(object sender, MsScanEventArgs e)
		{
			using (IMsScan scan = (IMsScan) e.GetScan())    // caution! You must dispose this, or you block shared memory!
			{
				Console.WriteLine("\n{0:HH:mm:ss,fff} scan with {1} centroids arrived", DateTime.Now, scan.CentroidCount);
				int i = 0;

				foreach (ICentroid c in scan.Centroids)
				{
					Console.Write("{0,-2}:  m/z={1,-9:F4} I={2:0.0e00} z={3,-2} R={4,-4}", i, c.Mz, c.Intensity, c.Charge, c.Resolution);
					if ((scan.ChargeEnvelopes != null) && c.ChargeEnvelopeIndex.HasValue)
					{
						IChargeEnvelope env = scan.ChargeEnvelopes[c.ChargeEnvelopeIndex.Value];
						Console.Write(" env={0,-2}    M={1,-9:F4} topId={2,-3} Xcorr={3:F4}", c.ChargeEnvelopeIndex, env.MonoisotopicMass, env.TopPeakCentroidId, env.CrossCorrelation);
					}
					Console.WriteLine();
					if (i++ >= 20)
					{
						break;
					}
				}
			}
		}
	}
}
