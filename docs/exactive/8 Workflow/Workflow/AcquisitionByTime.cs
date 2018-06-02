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

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace Workflow
{
	/// <summary>
	/// This class starts an acquisition for 20 seconds
	/// </summary>
	class AcquisitionByTime
	{
		internal AcquisitionByTime() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				IExactiveAcquisition workflow = instrument.Control.Acquisition;
				Console.WriteLine("Waiting 60 seconds for the instrument to enter On mode...");
				if (!workflow.WaitFor(TimeSpan.FromSeconds(60), SystemMode.On))
				{
					return;
				}

				Console.WriteLine("starting a 20 second acquisition...");
				workflow.StartAcquisition(workflow.CreateAcquisitionLimitedByDuration(TimeSpan.FromSeconds(20)));
			}
		}
	}
}
