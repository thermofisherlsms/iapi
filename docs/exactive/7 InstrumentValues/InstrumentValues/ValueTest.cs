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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.ExactiveAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control;

namespace InstrumentValues
{
	/// <summary>
	/// Demonstrate the access to individual readback and set values.
	/// </summary>
	class ValueTest
	{
		internal ValueTest() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				// Show available well-known nodes
				IExactiveInstrumentValues values = instrument.Control.InstrumentValues;
				int count = 0;
				foreach (string s in values.ValueNames.Select(n => n.PadRight(38)))
				{
					Console.Write(s);
					if ((++count % 2) == 0)
						Console.WriteLine();
				}
				Console.WriteLine();

				// "Follow" the readback of some known values. A set access is also possible.
				// Start Tune, select HotLink and ramp up and down the scan range to get an impression.
				string[] nodeNames = { "Root", "VirtualInstrument", "InstrumentAcquisition", "DefScanRangeLow", "DefScanRangeHigh" };
				List<IExactiveValue> nodes = new List<IExactiveValue>();
				foreach (string name in nodeNames)
				{
					IExactiveValue node = values.Get(name);
					nodes.Add(node);
					node.CommandsChanged += Node_CommandsChanged;
					node.ContentChanged += Node_ContentChanged;
				}

				foreach (IExactiveValue node in nodes)
				{
					Node_ContentChanged(node);
				}
				Console.WriteLine("Waiting 60 seconds for an instrument restart...");
				Thread.CurrentThread.Join(60000);
				foreach (IExactiveValue node in nodes)
				{
					node.ContentChanged -= Node_ContentChanged;
					node.CommandsChanged -= Node_CommandsChanged;
				}
			}
		}

		private void Node_CommandsChanged(object sender, EventArgs e)
		{
			Node_CommandsChanged((IExactiveValue) sender);
		}

		private void Node_ContentChanged(object sender, ContentChangedEventArgs e)
		{
			Node_ContentChanged((IExactiveValue) sender);
		}

		private void Node_CommandsChanged(IExactiveValue node)
		{
			Console.WriteLine("Commands for {0}: {1}", node.Name, string.Join("", node.Commands.Select(n => "\n   name=" + n.Name + ", selection=" + n.Selection + ", help=" + n.Help)));
		}

		private void Node_ContentChanged(IExactiveValue node)
		{
			string content = (node.Content == null) ? "((null))" : (node.Content.Content == null) ? "(null)" : node.Content.Content;
			Console.WriteLine("New content of {0}: {1}", node.Name, content);
		}

	}
}
