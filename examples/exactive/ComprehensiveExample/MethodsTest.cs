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
