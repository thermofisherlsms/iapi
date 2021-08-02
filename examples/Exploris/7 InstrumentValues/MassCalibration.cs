using System;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

using Common;

namespace InstrumentValues
{
	/// <summary>
	/// This class provides means to perform a procedure. If possible, it will be a mass calibration on positive
	/// masses. Up until Exploris 3.1, this results in executing a regular calibration.
	/// </summary>
	class MassCalibration
	{
		ManualResetEvent m_jobDone = new ManualResetEvent(false);
		IExplorisInstrumentAccess m_instrument;
		IExplorisValue m_procedures = null;

		/// <summary>
		/// Perform the calibration as described in <see cref="MassCalibration"/>
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				m_instrument = container.Get(1);

				// An API license is required for this. Wait until all licenses are transferred.
				if (!Connection.WaitForApiLicense(container, m_instrument, TimeSpan.FromSeconds(2)))
				{
					Console.Error.WriteLine("No connection to the core Service or missing an API license.");
					Environment.Exit(1);
				}

				m_instrument.Control.Acquisition.StateChanged += Acquisition_StateChanged;
				m_procedures = m_instrument.Control.InstrumentValues.Get("Procedures");
				m_procedures.CommandsChanged += Procedures_CommandsChanged;
				m_procedures.ContentChanged += Procedures_ContentChanged;

				Console.WriteLine("Waiting 60 seconds for the instrument GETTING READY TO RUN a calibration...");
				if (m_jobDone.WaitOne(60000))
				{
					Console.WriteLine("Waiting 10 further seconds to receive feedback about the calibration...");
					Thread.CurrentThread.Join(10000);
				}
				// Inhibit any further start:
				m_jobDone.Set();
				m_procedures.ContentChanged -= Procedures_ContentChanged;
				m_procedures.CommandsChanged -= Procedures_CommandsChanged;
				m_instrument.Control.Acquisition.StateChanged -= Acquisition_StateChanged;
			}
		}

		/// <summary>
		/// Callback when the state changes: we present the state and check if we could start our job.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">Will be used to show the system mode</param>
		private void Acquisition_StateChanged(object sender, StateChangedEventArgs e)
		{
			Console.WriteLine("New state {0}", e.State.SystemMode);
			CheckForStart();
		}

		/// <summary>
		/// Callback when commands to the procedures node become available or changed: we show these commands.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Procedures_CommandsChanged(object sender, EventArgs e)
		{
			Console.WriteLine("Commands for Procedures: {0}", string.Join("", m_procedures.Commands.Select(n => "\n   name=" + n.Name + ", selection=" + n.Selection + ", help=" + n.Help)));
			CheckForStart();
		}

		/// <summary>
		/// Callback when the content of the procedures node changes: we emit the current state on that node.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">Used to extract the state of the procedures node</param>
		private void Procedures_ContentChanged(object sender, ContentChangedEventArgs e)
		{
			string content = (m_procedures.Content == null) ? "((null))" : (m_procedures.Content.Content == null) ? "(null)" : m_procedures.Content.Content;
			Console.WriteLine("New content of Procedures: {0}", content);
		}

		/// <summary>
		/// Start the calibration if that hasn't been done yet.
		/// </summary>
		private void CheckForStart()
		{
			// calibration has already been executed, some state has changed to indicate calibration start, but state may be acceptable for starting
			if (m_jobDone.WaitOne(0))
			{
				return;
			}

			// state must be On!
			if (!m_instrument.Control.Acquisition.WaitFor(TimeSpan.Zero, n => n.SystemMode == SystemMode.On))
			{
				return;
			}

			// the node and its commands must be available and it must contain "Calibrate" with possible selection ""
			// or "PositiveMass".
			if ((m_procedures == null) | (m_procedures.Commands == null))
			{
				return;
			}
			IParameterDescription param = m_procedures.Commands.Where(n => n.Name == "Calibrate").FirstOrDefault();
			if (param != null)
			{
				if (param.Selection == null)
				{
					Console.WriteLine("Starting calibration...");
					m_procedures.Execute("Calibrate", null);
					m_jobDone.Set();
				}
				else if (param.Selection.Split(',').Contains("PositiveMass"))
				{
					Console.WriteLine("Starting calibration...");
					m_procedures.Execute("Calibrate", "PositiveMass");
					m_jobDone.Set();
				}
			}
		}
	}
}
