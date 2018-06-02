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
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace KeepInstrumentConnection
{
	/// <summary>
	/// Best way of connection testing is an event-driven model which also checks the overall state of the
	/// instrument. There is a textual summary available, but there are also indexed states (enums).
	/// </summary>
	class PreferredWay
	{
		internal PreferredWay() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				Console.WriteLine("Waiting 60 seconds for connection changes using event listening on Acquisition.StateChanged ...");

				// Best way is to use instrument.Control.Acquisition.WaitFor or a pure event handler of StateChange,
				// but for demonstration, we display the changes in ConnectionChanged and StateChanged.
				// For demonstration, start this program, reboot instrument and monitor also with Tune view.
				instrument.Control.Acquisition.StateChanged += Acquisition_StateChanged;
				instrument.ConnectionChanged += Instrument_ConnectionChanged;
				Thread.CurrentThread.Join(60000);
				instrument.ConnectionChanged -= Instrument_ConnectionChanged;
				instrument.Control.Acquisition.StateChanged -= Acquisition_StateChanged;
			}
			// See also instrument.Control.Acquisition.WaitFor
		}

		private void Instrument_ConnectionChanged(object sender, EventArgs e)
		{
			IExactiveInstrumentAccess instrument = (IExactiveInstrumentAccess) sender;
			Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + instrument.Connected);
		}

		private void Acquisition_StateChanged(object sender, StateChangedEventArgs e)
		{
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + "Overall description:     " + e.State.Description);
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + "Instrument mode:         " + e.State.SystemMode);
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + "Instrument system state: " + e.State.SystemState);
		}
	}
}
