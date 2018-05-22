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
using Thermo.Interfaces.InstrumentAccess_V1.AnalogTraceContainer;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class displays the output of the analog channels when they arrive.
	/// </summary>
	internal class AnalogOutput
	{
		/// <summary>
		/// Create a new <see cref="AnalogOutput"/> and make sure two analog channels are observed.
		/// </summary>
		/// <param name="instrument">the instrument instance</param>
		internal AnalogOutput(IInstrumentAccess instrument)
		{
			Analog1 = instrument.GetAnalogTraceContainer(0);
			Analog1.AnalogTracePointArrived += new EventHandler<AnalogTracePointEventArgs>(Instrument_AnalogTracePointArrived);
			Analog2 = instrument.GetAnalogTraceContainer(1);
			Analog2.AnalogTracePointArrived += new EventHandler<AnalogTracePointEventArgs>(Instrument_AnalogTracePointArrived);
		}

		/// <summary>
		/// Cleanup this instance.
		/// </summary>
		internal void CloseDown()
		{
			// Be tolerant to thread-switches
			IAnalogTraceContainer analogContainer;

			analogContainer = Analog1;
			Analog1 = null;
			if (analogContainer != null)
			{
				analogContainer.AnalogTracePointArrived -= new EventHandler<AnalogTracePointEventArgs>(Instrument_AnalogTracePointArrived);
			}

			analogContainer = Analog2;
			Analog2 = null;
			if (analogContainer != null)
			{
				analogContainer.AnalogTracePointArrived -= new EventHandler<AnalogTracePointEventArgs>(Instrument_AnalogTracePointArrived);
			}
		}

		/// <summary>
		/// Access to the first analog container.
		/// </summary>
		private IAnalogTraceContainer Analog1 { get; set; }

		/// <summary>
		/// Access to the second analog container.
		/// </summary>
		private IAnalogTraceContainer Analog2 { get; set; }

		/// <summary>
		/// When an analog event arrives we dump the content.
		/// </summary>
		/// <param name="sender">used to identify the channel</param>
		/// <param name="e">content will be dumped</param>
		private void Instrument_AnalogTracePointArrived(object sender, AnalogTracePointEventArgs e)
		{
			IAnalogTraceContainer analogTrace = sender as IAnalogTraceContainer;
			if (analogTrace != null)
			{
				Console.WriteLine("{0}: [{1} - {2}]: {3} at {4} s", analogTrace.DetectorClass, analogTrace.Minimum, analogTrace.Maximum, e.TracePoint.Value, e.TracePoint.Occurrence.TotalSeconds);
			}
		}
	}
}
