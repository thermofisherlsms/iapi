#region legal notice
//
// Copyright 2011 - 2013 Thermo Fisher Scientific Inc. or subsidiaries
//
// This is part of an example on how to use the API of an Exactive Series instrument
// developed by Thermo Fisher Scientific (Bremen) GmbH.
//
// This file and the whole program are delivered “as is“ and without any warranty.
//
// Permission is granted to use this file or parts of the file for your own development, as long as there is a statement in the file header that the file has been modified, if applicable.
//
#endregion legal notice

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

using IMsScan = Thermo.Interfaces.InstrumentAccess_V2.MsScanContainer.IMsScan;
using ICentroid = Thermo.Interfaces.InstrumentAccess_V2.MsScanContainer.ICentroid;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class presents the output of the scans being acquired by the instrument.
	/// </summary>
	internal class ScansOutput
	{
		private bool m_firstScan = true;
		private double m_integral = 0;
		private long m_profiles = 0;
		private long m_centroids = 0;
		private long m_scans = 0;

		/// <summary>
		/// Crate a new <see cref="ScansOutput"/>
		/// </summary>
		/// <param name="instrument">the instrument instance</param>
		/// <param name="arguments">program arguments</param>
		internal ScansOutput(IExactiveInstrumentAccess instrument, Arguments arguments)
		{
			Arguments = arguments;

			ScanContainer = instrument.GetMsScanContainer(0);
			if (Arguments.Verbose)
			{
				Console.WriteLine("Detector class: " + ScanContainer.DetectorClass);
			}
			ScanContainer.AcquisitionStreamOpening += new EventHandler<MsAcquisitionOpeningEventArgs>(ScanContainer_AcquisitionStarted);
			ScanContainer.AcquisitionStreamClosing += new EventHandler(ScanContainer_AcquisitionEnded);
			ScanContainer.MsScanArrived += new EventHandler<MsScanEventArgs>(ScanContainer_ScanArrived);
		}

		/// <summary>
		/// Show the last acquired scan if that exists and cleanup.
		/// </summary>
		internal void CloseDown()
		{
			// Be tolerant to thread-switches
			IMsScanContainer scanContainer = ScanContainer;
			ScanContainer = null;

			if (!Arguments.Verbose)
			{
				// only acquired in non-verbose mode:
				Console.WriteLine("scans={0}, centroids={1}, profiles={2}", m_scans, m_centroids, m_profiles);
			}

			if (scanContainer != null)
			{
				scanContainer.MsScanArrived -= new EventHandler<MsScanEventArgs>(ScanContainer_ScanArrived);
				scanContainer.AcquisitionStreamClosing -= new EventHandler(ScanContainer_AcquisitionEnded);
				scanContainer.AcquisitionStreamOpening -= new EventHandler<MsAcquisitionOpeningEventArgs>(ScanContainer_AcquisitionStarted);
				if (Arguments.Verbose)
				{
					using (IMsScan scan = (/* V2 */ IMsScan) scanContainer.GetLastMsScan())
					{
						DumpScan("GetLastScan()", scan);
					}
				}
			}
		}

		/// <summary>
		/// Access to the program arguments.
		/// </summary>
		internal Arguments Arguments { get; private set; }

		/// <summary>
		/// Access to the scan container hosted by this instance.
		/// </summary>
		private IMsScanContainer ScanContainer { get; set; }

		/// <summary>
		/// When a new acquisition starts we dump that information in verbose mode.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">contains the informations to be printed</param>
		private void ScanContainer_AcquisitionStarted(object sender, MsAcquisitionOpeningEventArgs e)
		{
			if (Arguments.Verbose)
			{
				Console.WriteLine("START OF ACQUISITION");
				if (Arguments.Chatty)
				{
					DumpVars(e.SpecificInformation);
				}
			}
		}

		/// <summary>
		/// When an acquisitions ends we dump that information in verbose mode.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void ScanContainer_AcquisitionEnded(object sender, EventArgs e)
		{
			if (Arguments.Verbose)
			{
				Console.WriteLine("END OF ACQUISITION");
			}
		}

		/// <summary>
		/// When a new scan arrives we dump that information in verbose mode.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">used to access the scan information</param>
		private void ScanContainer_ScanArrived(object sender, MsScanEventArgs e)
		{
			if (!Arguments.Verbose)
			{
				Console.WriteLine("Scan arrived");
				// As an example we access all centroids
				using (IMsScan scan = (/* V2 */ IMsScan) e.GetScan())
				{
					m_integral += AllProfInts(scan);
				}
				return;
			}

			// Dump the scan content.
			using (IMsScan scan = (/* V2 */ IMsScan) e.GetScan())
			{
				DumpScan("Scan arrived", scan);
			}
		}

		/// <summary>
		/// Dump a scan and prepend it with an intro string.
		/// </summary>
		/// <param name="intro">string to prepend</param>
		/// <param name="scan">thing to dump</param>
		private void DumpScan(string intro, IMsScan scan)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(Instrument.Now.ToString(Program.TimeFormat));
			sb.Append(": ");
			sb.Append(intro);
			sb.Append(", ");
			if (scan == null)
			{
				sb.Append("(empty scan)");
				Console.WriteLine(sb.ToString());
				return;
			}
			else
			{
				sb.Append("detector=");
				sb.Append(scan.DetectorName);
				string id;
				if (scan.SpecificInformation.TryGetValue("Access Id:", out id))
				{
					sb.Append(", id=");
					sb.Append(id);
				}
				Console.WriteLine(sb.ToString());
			}

			if (m_firstScan || Arguments.Chatty)
			{
				DumpVars(scan);
			}

			if (Arguments.Chatty)
			{
				Console.Write("  Noise: ");
				foreach (INoiseNode noise in scan.NoiseBand)
				{
					Console.Write("[{0}, {1}], ", noise.Mz, noise.Intensity);
				}
				Console.WriteLine();
			}

			Console.WriteLine("{0} centroids, {1} profile peaks", scan.CentroidCount ?? 0, scan.ProfileCount ?? 0);
			foreach (ICentroid centroid in scan.Centroids)
			{
				if (m_firstScan || Arguments.Chatty)
				{
					Console.WriteLine(" {0,10:F5}, " + /*"I={1:E5}, C={2}, " +*/ "E={3,-5} F={4,-5} M={5,-5} R={6,-5} Res={7}",
									  centroid.Mz, centroid.Intensity, centroid.Charge, centroid.IsExceptional, centroid.IsFragmented, centroid.IsMerged, centroid.IsReferenced, centroid.Resolution);
					if (scan.HasProfileInformation)
					{
						Console.Write("    Profile:");
						try
						{
							foreach (IMassIntensity profilePeak in centroid.Profile)
							{
								Console.Write(" [{0,10:F5},{1:E5}] ", profilePeak.Mz, profilePeak.Intensity);
							}
						}
						catch
						{
						}
						Console.WriteLine();
					}
				}
				else
				{
					if ((centroid.Charge ?? 0) > 1)
						Console.WriteLine(" {0,10:F5}, " + /*"I={1:E5}, C={2}, " +*/ "E={3,-5} F={4,-5} M={5,-5} R={6,-5} Res={7}",
										  centroid.Mz, centroid.Intensity, centroid.Charge, centroid.IsExceptional, centroid.IsFragmented, centroid.IsMerged, centroid.IsReferenced, centroid.Resolution);
				}
			}
			m_firstScan = false;
		}

		/// <summary>
		/// Dump all variables belonging to a scan
		/// </summary>
		/// <param name="scan"></param>
		private void DumpVars(IMsScan scan)
		{
			Console.WriteLine("  COMMON");
			DumpVars(scan.CommonInformation);
			Console.WriteLine("  SPECIFIC");
			DumpVars(scan.SpecificInformation);
		}

		/// <summary>
		/// Dump all scan variables belonging to a specific container in a scan.
		/// </summary>
		/// <param name="container">container to dump all contained variables for</param>
		private void DumpVars(IInfoContainer container)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string s in container.Names)
			{
				DumpVar(container, s, sb);
			}
		}

		/// <summary>
		/// Dump the content of a single variable to the console after testing the consistency.
		/// </summary>
		/// <param name="container">container that variable belongs to</param>
		/// <param name="name">name of the variable</param>
		/// <param name="sb">buffer to be reused for speed</param>
		private void DumpVar(IInfoContainer container, string name, StringBuilder sb)
		{
			sb.Length = 0;

			// NOTE: The whole try block performs the consistency check except that
			// part that is marked. You can ignore it savely.

			try
			{
				object o0 = null, o1, o2, o3;
				string s0 = null, s1, s2, s3;
				MsScanInformationSource i0 = MsScanInformationSource.Unknown, i1, i2;
				i1 = MsScanInformationSource.Unknown;
				if (!container.TryGetValue(name, out s1, ref i1))
				{
					sb.Append("NO TEXT(Unk) ");
				}
				else
				{
					i0 = i1;
					s0 = s1;
					if (!container.TryGetValue(name, out s2, ref i1))
					{
						sb.Append("NO TEXT(" + i0 + ") ");
					}
					else if (s1 != s2)
					{
						sb.Append("TEXT DIFF1 ");
					}
				}

				if (!container.TryGetValue(name, out s3))
				{
					sb.Append("NO PLAIN TEXT ");
				}
				else if (s0 != s3)
				{
					sb.Append("TEXT DIFF2 ");
					s0 = s3;
				}
				if (i0 != MsScanInformationSource.Unknown)
				{
					for (MsScanInformationSource ss = MsScanInformationSource.Common; ss <= MsScanInformationSource.AcquisitionFixed; ss++)
					{
						if (ss == i0)
						{
							continue;
						}
						i2 = ss;
						if (container.TryGetValue(name, out s2, ref i2))
						{
							sb.Append("TEXT DOUBLETTE ");
						}
					}
				}

				i2 = MsScanInformationSource.Unknown;
				if (!container.TryGetRawValue(name, out o1, ref i2))
				{
					sb.Append("NO DATA(Unk) ");
					i0 = MsScanInformationSource.Unknown;
				}
				else
				{
					if (i2 != i0)
					{
						sb.Append("SRC DIFF ");
						i0 = i2;
					}
					i1 = i0;
					o0 = o1;
					if (!container.TryGetRawValue(name, out o2, ref i1))
					{
						sb.Append("NO TEXT(" + i0 + ") ");
					}
					else if (!o1.Equals(o2))
					{
						sb.Append("DATA DIFF1 ");
					}
				}

				if (!container.TryGetRawValue(name, out o3))
				{
					sb.Append("NO PLAIN Data ");
				}
				else if (!o0.Equals(o3))
				{
					sb.Append("DATA DIFF2 ");
					o0 = o3;
				}
				if (i0 != MsScanInformationSource.Unknown)
				{
					for (MsScanInformationSource ss = MsScanInformationSource.Common; ss <= MsScanInformationSource.AcquisitionFixed; ss++)
					{
						if (ss == i0)
						{
							continue;
						}
						i2 = ss;
						if (container.TryGetRawValue(name, out o2, ref i2))
						{
							sb.Append("DATA DOUBLETTE ");
						}
					}
				}

				// ADD THE REAL INFORMATION
				sb.Append("type=" + i0);
				sb.Append(", text='" + s0);
				sb.Append("', raw='" + o0);
				sb.Append("'");
				if (sb.ToString().Equals("NO TEXT(Unk) NO PLAIN TEXT NO DATA(Unk) NO PLAIN Data type=Unknown, text='', raw=''"))
				{
					sb.Length = 0;
					sb.Append("NO ACCESS");
				}
			}
			finally
			{
				sb.Insert(0, "  " + name + ": ");
				Console.WriteLine(sb.ToString());
			}
		}

		/// <summary>
		/// This routine accesses all centroids and profiles in the scan to show the usage.
		/// </summary>
		/// <param name="scan">the scan to deal with</param>
		private double AllProfInts(IMsScan scan)
		{
			// Access all profile information directly. Either use this way or that shown below.
			int pc = scan.ProfileCount ?? 0;
			if (pc > 0)
			{
				double[] mzs = new double[pc];
				double[] ints = new double[pc];
				scan.GetProfileData(ref mzs, ref ints);
			}
			double retval = 0;
			m_scans++;

			// Access the centroids and, as part of each centroid, the list of profile elements.
			// Those may be omitted if the instrument only acquires centroids and no profile information.
			foreach (ICentroid centroid in scan.Centroids)
			{
				m_centroids++;
				foreach (IMassIntensity p in centroid.Profile)
				{
					m_profiles++;
					retval += p.Intensity;
				}
			}

			return retval;
		}
	}
}
