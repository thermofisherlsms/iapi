#region legal notice
// Copyright(c) 2011 - 2018 Thermo Fisher Scientific - LSMS
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
