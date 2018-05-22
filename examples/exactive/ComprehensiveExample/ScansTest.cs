#region legal notice
//
// Copyright 2011 - 2014 Thermo Fisher Scientific Inc. or subsidiaries
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
using System.Threading;
using System.Globalization;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Scans;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class demonstrates the use of the <see cref="IScans"/> interface.
	/// </summary>
	internal class ScansTest : IDisposable
	{
		private IScans m_scans;
		private bool m_startCustomScan = true;
		private object m_lock = new object();
		private int m_disposed;
		private long m_runningNumber = 12345;	// start with an offset to make sure it's "us"
		private int m_polarity = 0;

		/// <summary>
		/// Create a new <see cref="ScansTest"/> and start the performance test immediately.
		/// </summary>
		/// <param name="instrument">the instrument instance</param>
		/// <param name="arguments">program arguments</param>
		internal ScansTest(IExactiveInstrumentAccess instrument, Arguments arguments)
		{
			Arguments = arguments;

			m_scans = instrument.Control.GetScans(false);
			m_scans.CanAcceptNextCustomScan += new EventHandler(Scans_CanAcceptNextCustomScan);
			m_scans.PossibleParametersChanged += new EventHandler(Scans_PossibleParametersChanged);

			DumpPossibleParameters();
			bool startNewScan = false;
			lock (m_lock)
			{
				if (m_scans.PossibleParameters.Length > 0)
				{
					startNewScan = m_startCustomScan;
					m_startCustomScan = false;
				}
			}

			if (startNewScan)
			{
				StartNewScan();
			}
		}

		/// <summary>
		/// The final destructor releases allocated system resources.
		/// </summary>
		~ScansTest()
		{
			// Let the GC dispose managed members itself.
			Dispose(false);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposeEvenManagedStuff">true to dispose managed and unmanaged resources; false to dispose unmanaged resources</param>
		protected void Dispose(bool disposeEvenManagedStuff)
		{
			// prevent double disposing
			if (Interlocked.Exchange(ref m_disposed, 1) != 0)
			{
				return;
			}

			if (disposeEvenManagedStuff)
			{
				if (m_scans != null)
				{
					m_scans.CanAcceptNextCustomScan -= new EventHandler(Scans_CanAcceptNextCustomScan);
					m_scans.PossibleParametersChanged -= new EventHandler(Scans_PossibleParametersChanged);
					m_scans.Dispose();
					m_scans = null;
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		virtual public void Dispose()
		{
			// Dispose managed and unmanaged resources and tell GC we don't need the destructor getting called.
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Get access to the flag whether this object is disposed.
		/// </summary>
		internal bool Disposed { get { return m_disposed != 0; } }

		/// <summary>
		/// Access to the program arguments.
		/// </summary>
		internal Arguments Arguments { get; private set; }

		/// <summary>
		/// Dump the list of possible commands.
		/// </summary>
		/// <param name="scans">where to take possible parameters from</param>
		/// <param name="verbose">shall each parameter be dumped?</param>
		static private bool DumpPossibleParameters(IScans scans, bool verbose)
		{
			IParameterDescription[] parameters = scans.PossibleParameters;
			if (parameters.Length == 0)
			{
				Console.WriteLine("No possible IScans parameters known.");
				return false;
			}
			if (verbose)
			{
				Console.WriteLine("IScans parameters:");
				foreach (IParameterDescription parameter in parameters)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("   '{0}' ", parameter.Name);
					if (parameter.Selection == "")
					{
						sb.AppendFormat("doesn't accept an argument, help: {0}", parameter.Help);
					}
					else
					{
						sb.AppendFormat("accepts '{0}', default='{1}', help: {2}", parameter.Selection, parameter.DefaultValue, parameter.Help);
					}
					Console.WriteLine(sb.ToString());
				}
			}
			else
			{
				Console.WriteLine("IScans parameters: {0}", string.Join(", ", parameters.Select(n => n.Name).ToArray()));
			}
			return true;
		}

		/// <summary>
		/// Dump the list of possible commands.
		/// </summary>
		private bool DumpPossibleParameters()
		{
			return DumpPossibleParameters(m_scans, Arguments.Verbose);
		}

		/// <summary>
		/// Start a new custom scan.
		/// </summary>
		private void StartNewScan()
		{
			ICustomScan cs = m_scans.CreateCustomScan();
			cs.RunningNumber = m_runningNumber++;
			
			// Allow an extra delay of 500 ms, we will answer as fast as possible, so this is a maximum value.
			cs.SingleProcessingDelay = 0.50D;

			// Toggle the polarity:
			m_polarity = (m_polarity == 0) ? 1 : 0;
			cs.Values["Polarity"] = m_polarity.ToString(NumberFormatInfo.InvariantInfo);

			try
			{
				DateTime now = Instrument.Now;
				if (!m_scans.SetCustomScan(cs))
				{
					Console.WriteLine("NEW CUSTOM SCAN HAS NOT BEEN PLACED, CONNECTION TO SERVICE BROKEN.");
				}
				Console.WriteLine(now.ToString(Program.TimeFormat) + ": Placed a new custom scan(" + cs.RunningNumber + ")");
			}
			catch (Exception e)
			{
				Console.WriteLine("PLACING A NEW SCAN: " + e.Message);
			}
		}

		/// <summary>
		/// Called when the current custom scan has been processed and the next custom scan can be accepted.
		/// We start a new scan.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Scans_CanAcceptNextCustomScan(object sender, EventArgs e)
		{
			Console.WriteLine(Instrument.Now.ToString(Program.TimeFormat) + ": CanAcceptNextCustomScan");
			if ((m_scans != null) && (m_scans.PossibleParameters.Length > 0))
			{
				// Assume we are able to place a new scan.
				StartNewScan();
			}
		}

		/// <summary>
		/// Called when the list of possible commands have changed we dump them.
		/// Additionally we start a new scan.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Scans_PossibleParametersChanged(object sender, EventArgs e)
		{
			if (!DumpPossibleParameters())
			{
				return;
			}

			bool startNewScan = false;
			lock (m_lock)
			{
				if (m_scans.PossibleParameters.Length > 0)
				{
					startNewScan = m_startCustomScan;
					m_startCustomScan = false;
				}
			}

			if (startNewScan)
			{
				StartNewScan();
			}
		}

		/// <summary>
		/// Set a default repeating scan with a FirstMass of 400.
		/// </summary>
		/// <param name="access">used to get access to the needed IScans interface.</param>
		/// <param name="verbose">shall the output be verbose?</param>
		static internal bool SetRepeatingScan(IExactiveInstrumentAccess access, bool verbose)
		{
			try
			{
				using (IScans scans = access.Control.GetScans(false))
				{
					bool parametersArrived = (scans.PossibleParameters.Length != 0);
					// Test if we have to wait for arrival of possible parameters:
					if (!parametersArrived)
					{
						EventHandler handler = (sender, args) => { parametersArrived = true; };
						scans.PossibleParametersChanged += handler;
						DateTime end = DateTime.Now.AddSeconds(3);
						// Not elegant, but it works
						while (!parametersArrived && (DateTime.Now < end))
						{
							// Sleep but perform COM communication in background.
							Thread.CurrentThread.Join(10);
						}
						scans.PossibleParametersChanged -= handler;
						// Fall into an error below if we didn't receive parameter descriptions.
					}

					DumpPossibleParameters(scans, verbose);

					IRepeatingScan scan = scans.CreateRepeatingScan();
					scan.RunningNumber = 9999;
					scan.Values["FirstMass"] = "400";

					if (scans.SetRepetitionScan(scan))
					{
						return true;
					}

					Console.WriteLine("SETTING A REPETITION SCAN HAS NOT BEEN DONE, CONNECTION TO SERVICE BROKEN.");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("SETTING A REPETITION SCAN: " + e.Message);
			}

			return false;
		}

		/// <summary>
		/// Cancel an outstanding repetition scan.
		/// </summary>
		/// <param name="access">used to get access to the needed IScans interface.</param>
		static internal void CancelRepeatingScan(IExactiveInstrumentAccess access)
		{
			try
			{
				using (IScans scans = access.Control.GetScans(false))
				{
					if (!scans.CancelRepetition())
					{
						Console.WriteLine("CANCELLATION OF THE REPETITION SCAN HAS NOT BEEN PERFORMED, CONNECTION TO SERVICE BROKEN.");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("CANCELING A REPETITION SCAN: " + e.Message);
			}
		}
	}
}
