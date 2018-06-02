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

namespace KeepInstrumentConnection
{
	/// <summary>
	/// Event driven connection check even see short disruptions. But is just checks the pure connection.
	/// That is insufficient in many cases.
	/// </summary>
	class EventDriven
	{
		internal EventDriven() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				Console.WriteLine("Waiting 60 seconds for connection changes using event listening...");

				// Check the connection initially (after setting up the handler for race conditions), but let the API announce connection changes
				instrument.ConnectionChanged += Instrument_ConnectionChanged;
				Thread.CurrentThread.Join(60000);
				instrument.ConnectionChanged -= Instrument_ConnectionChanged;
				Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + instrument.Connected);
			}
		}

		private void Instrument_ConnectionChanged(object sender, EventArgs e)
		{
			IExactiveInstrumentAccess instrument = (IExactiveInstrumentAccess) sender;
			Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + instrument.Connected);
		}
	}
}
