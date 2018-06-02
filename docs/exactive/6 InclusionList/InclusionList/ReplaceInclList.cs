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
using Thermo.Interfaces.InstrumentAccess_V1.Control.Methods;
using Thermo.Interfaces.InstrumentAccess_V1.MsScanContainer;

namespace InclusionList
{
	/// <summary>
	/// Replace the include list during a method run. It is expected that the method uses an
	/// inclusion list as in an PRM experiment.
	/// </summary>
	class ReplaceInclList
	{
		ITable m_replacementTable;
		IMethods m_methods;

		internal ReplaceInclList() { }

		internal void DoJob()
		{
			using (IExactiveInstrumentAccess instrument = Connection.GetFirstInstrument())
			{
				m_methods = instrument.Control.Methods;

				// don't know the time for powering up the table ==> do it (once) in advance of its use
				m_replacementTable = CreateReplacementTable();

				IMsScanContainer orbitrap = instrument.GetMsScanContainer(0);
				Console.WriteLine("Waiting 60 seconds for a starting acquisition...");
				orbitrap.AcquisitionStreamOpening += Orbitrap_AcquisitionStreamOpening;
				Thread.CurrentThread.Join(60000);
				orbitrap.AcquisitionStreamOpening -= Orbitrap_AcquisitionStreamOpening;
			}
		}

		private ITable CreateReplacementTable()
		{
			// Show possible columns, their names, help and value options
			ITable replacementTable = m_methods.CreateTable(typeof(IInclusionTable));
			foreach (ITableColumnDescription col in replacementTable.ColumnInfo)
			{
				Console.WriteLine("{0,-12} {1,-25} {2}", col.Name, col.Selection, (col.Help ?? "").Trim().Replace("\r", "").Replace("\n", "; "));
				if (col.AcceptedHeaderNames.Length > 0)
				{
					Console.WriteLine("{0,38} alternative names: {1}", "", string.Join(", ", col.AcceptedHeaderNames));
				}
			}

			// Create a new table with two rows.
			ITableRow row = replacementTable.CreateRow();
			row.ColumnValues["Polarity"] = "Positive";  // must match, PRM has a polarity setting
			row.ColumnValues["Mass [m/z]"] = "700";
			replacementTable.Rows.Add(row);
			row = replacementTable.CreateRow();
			row.ColumnValues["Polarity"] = "Negative";
			row.ColumnValues["Mass [m/z]"] = "1600";
			replacementTable.Rows.Add(row);

			return replacementTable;
		}

		private void Orbitrap_AcquisitionStreamOpening(object sender, MsAcquisitionOpeningEventArgs e)
		{
			// action can happen at any time, but at start of an acquisition, we may cover most scenarios. It may have a delay, though.
			m_methods.ReplaceTable(1, 13 /* must be != 0 */, m_replacementTable);
			Console.WriteLine("Replaced the table");
			Environment.Exit(0);
		}
	}
}
