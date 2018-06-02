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
	/// Polling is the easiest way of checking a connection, but it may miss a fast disconnect/connect.
	/// </summary>
	class Polling
	{
		internal Polling() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				Console.WriteLine("Waiting 60 seconds for connection changes using polling...");
				DateTime end = DateTime.Now.AddSeconds(60);
				bool connected = instrument.Connected;
				Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + connected);

				// Check every 10 ms for a connection change, 10ms is resonsive, and we don't eat much time here.
				while (end > DateTime.Now)
				{
					Thread.CurrentThread.Join(10);
					bool c = instrument.Connected;
					if (c != connected)
					{
						connected = c;
						Console.WriteLine("\n" + DateTime.Now.ToString("HH:mm:ss,fff ") + instrument.InstrumentName + " connected:" + connected);
					}
				}
			}
		}
	}
}
