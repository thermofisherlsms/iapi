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
using System.IO;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition;
using Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition.Workflow;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;

namespace Workflow
{
	/// <summary>
	/// This class starts an acquisition by running a method.
	/// </summary>
	class RunningMethod
	{
		internal RunningMethod() { }

		internal void DoJob()
		{
			string methodName;
			for (;;)
			{
				Console.WriteLine("(Please await end of the previous acquisition first)");
				Console.WriteLine("Name of the method to execute? (default folder is C:\\Xcalibur\\methods...");
				methodName = Path.Combine("C:\\Xcalibur\\methods", Console.ReadLine().Trim());
				if (File.Exists(methodName))
				{
					break;
				}
				Console.WriteLine("File doesn't exist: " + methodName);
				Console.WriteLine();
			}
			Console.WriteLine("Output file will be C:\\Xcalibur\\data\\API-Training.raw");

			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				IExactiveAcquisition workflow = instrument.Control.Acquisition;
				Console.WriteLine("Waiting 60 seconds for the instrument to enter On mode...");
				if (!workflow.WaitFor(TimeSpan.FromSeconds(60), SystemMode.On))
				{
					return;
				}

				Console.WriteLine("starting method " + methodName);
				IAcquisitionMethodRun acquisition = workflow.CreateMethodAcquisition(methodName);
				acquisition.RawFileName = "C:\\Xcalibur\\data\\API-Training.raw";
				workflow.StartAcquisition(acquisition);
			}
		}
	}
}
