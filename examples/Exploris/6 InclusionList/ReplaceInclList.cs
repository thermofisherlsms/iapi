using System;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Methods;

using Common;

namespace InclusionList
{
	/// <summary>
	/// This class replaces an inclusion list during an execution of a method. It is a demonstration, not an example
	/// with a purpose different to showing the steps of execution.
	/// </summary>
	class ReplaceInclList
	{
		ITable m_replacementTable;
		IMethods m_methods;

		/// <summary>
		/// Bring the system to mode "On" and start execution of a method that uses an inclusion list.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				// An API license is required for this. Wait until all licenses are transferred.
				if (!Connection.WaitForApiLicense(container, instrument, TimeSpan.FromSeconds(2)))
				{
					Console.Error.WriteLine("No connection to the core Service or missing an API license.");
					Environment.Exit(1);
				}

				m_methods = instrument.Control.Methods;

				// don't know the time for powering up the table ==> do it (once) in advance of its use
				m_replacementTable = CreateReplacementTable();

				Console.WriteLine("Waiting 60 seconds for a starting acquisition...");
				instrument.Control.Acquisition.AcquisitionStreamOpening += Orbitrap_AcquisitionStreamOpening;
				Thread.CurrentThread.Join(60000);
				instrument.Control.Acquisition.AcquisitionStreamOpening -= Orbitrap_AcquisitionStreamOpening;
			}
		}

		/// <summary>
		/// Create an inclusion table.
		/// </summary>
		/// <returns>Returns the created list with two elements</returns>
		private ITable CreateReplacementTable()
		{
			ITable replacementTable = m_methods.CreateTable(typeof(IInclusionTable));
			foreach (ITableColumnDescription col in replacementTable.ColumnInfo)
			{
				Console.WriteLine("{0,-12} {1,-25} {2}", col.Name, col.Selection, (col.Help ?? "").Trim().Replace("\r", "").Replace("\n", "; "));
				if (col.AcceptedHeaderNames.Length > 0)
				{
					Console.WriteLine("{0,38} alternative names: {1}", "", string.Join(", ", col.AcceptedHeaderNames));
				}
			}
			ITableRow row = replacementTable.CreateRow();
			row.ColumnValues["Polarity"] = "0";		// must match, PRM has a polarity setting
			row.ColumnValues["Value"] = "700";		// m/z
			replacementTable.Rows.Add(row);

			row = replacementTable.CreateRow();
			row.ColumnValues["Polarity"] = "1";
			row.ColumnValues["Value"] = "1600";
			replacementTable.Rows.Add(row);

			return replacementTable;
		}

		/// <summary>
		/// Callback when the system start a new acquisition: Assuming that a method is started, we replace the inclusion table, assuming
		/// that it has ID 1. The program is stopped after the operation.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void Orbitrap_AcquisitionStreamOpening(object sender, ExplorisAcquisitionOpeningEventArgs e)
		{
			// action can happen at any time, but at start of an acquisition, we may cover most scenarios. It may have a delay, though.
			m_methods.ReplaceTable(1, 13 /* != 0 */, m_replacementTable);
			Console.WriteLine("Replaced the table");
			Environment.Exit(0);
		}
	}
}
