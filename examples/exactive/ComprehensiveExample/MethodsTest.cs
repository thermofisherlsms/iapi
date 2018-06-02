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
using System.Collections.Generic;
using System.Text;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Methods;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class provides a test for the IMethods interface. An inclusion list
	/// will be loaded, dumped and applied to the currently running method.
	/// </summary>
	internal class MethodsTest
	{
		internal interface XX : IInclusionTable
		{
			bool b { get; set; }
		}

		class XXI : XX
		{
			public bool b
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public IList<ITableColumnDescription> ColumnInfo
			{
				get { throw new NotImplementedException(); }
			}

			public ITableRow CreateRow()
			{
				throw new NotImplementedException();
			}

			public IList<ITableRow> Rows
			{
				get { throw new NotImplementedException(); }
			}
		}

		/// <summary>
		/// Create a new <see cref="MethodsTest"/> and start the performance immediately.
		/// </summary>
		/// <param name="instrument">the instrument instance</param>
		/// <param name="arguments">program arguments</param>
		internal MethodsTest(IExactiveInstrumentAccess instrument, Arguments arguments)
		{
			ITable table;
			ITableRow row;
			table = instrument.Control.Methods.LoadTable(arguments.MethodsTest, 1, typeof(IInclusionTable));
			table = instrument.Control.Methods.LoadTable(arguments.MethodsTest, 1, table.GetType());
			Console.WriteLine("Columns in the inclusion table:");
			foreach (ITableColumnDescription descr in table.ColumnInfo)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("   {1}'{0}' ", descr.Name, (descr.Optional) ? "optional " : "mandantory ");
				if (descr.Selection == "")
				{
					sb.AppendFormat("doesn't accept an argument, help: {0}", descr.Help);
				}
				else
				{
					sb.AppendFormat("accepts '{0}', default='{1}', help: {2}", descr.Selection, descr.DefaultValue, descr.Help);
				}
				Console.WriteLine(sb.ToString());
			}
			foreach (ITableRow r in table.Rows)
			{
				Console.WriteLine("row = " + r);
			}
			Console.WriteLine("ReplaceTable=" + instrument.Control.Methods.ReplaceTable(1, 2, table));
			row = table.CreateRow();
			row.ColumnValues["Mass [m/z]"] = "120";
			row.ColumnValues["Polarity"] = "Positive";
			table.Rows.Add(row);
			instrument.Control.Methods.ReplaceTable(1, 43, table);
		}

		/// <summary>
		/// Cleanup this instance.
		/// </summary>
		internal void CloseDown()
		{
		}
	}
}
