using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Thermo.Interfaces.ExplorisAccess_V1;
using Thermo.Interfaces.ExplorisAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control;

using Common;

namespace InstrumentValues
{
	/// <summary>
	/// This class provides means to observe all changes on four different, well-known nodes.
	/// </summary>
	class ValueTest
	{
		/// <summary>
		/// Display all properties of four nodes and when they change. Start/stop running a method to observe changes.
		/// </summary>
		internal void DoJob()
		{
			using (IExplorisInstrumentAccessContainer container = Connection.Get())
			{
				IExplorisInstrumentAccess instrument = container.Get(1);

				// Show available well-known nodes
				IExplorisInstrumentValues values = instrument.Control.InstrumentValues;
				int count = 0;
				foreach (string s in values.ValueNames.Select(n => n.PadRight(38)))
				{
					Console.Write(s);
					if ((++count % 2) == 0)
					{
						Console.WriteLine();
					}
				}
				Console.WriteLine();

				string[] nodeNames = { "Root", "VirtualInstrument", "InstrumentAcquisition", "Procedures" };
				List<IExplorisValue> nodes = new List<IExplorisValue>();
				foreach (string name in nodeNames)
				{
					IExplorisValue node = values.Get(name);
					nodes.Add(node);
					node.CommandsChanged += Node_CommandsChanged;
					node.ContentChanged += Node_ContentChanged;
				}

				foreach (IExplorisValue node in nodes)
				{
					Node_ContentChanged(node.Name, node.Content);
				}
				Console.WriteLine("Waiting 60 seconds for an instrument action like a restart, a method start, etc...");
				Thread.CurrentThread.Join(60000);
				foreach (IExplorisValue node in nodes)
				{
					node.ContentChanged -= Node_ContentChanged;
					node.CommandsChanged -= Node_CommandsChanged;
				}
			}
		}

		/// <summary>
		/// Callback when the commands of a node changed: we dump those commands.
		/// </summary>
		/// <param name="sender">The sender is the node, we dump that</param>
		/// <param name="e">doesn't matter</param>
		private void Node_CommandsChanged(object sender, EventArgs e)
		{
			Node_CommandsChanged((IExplorisValue) sender);
		}

		/// <summary>
		/// Callback when the content of a node changed: we dump that.
		/// </summary>
		/// <param name="sender">The sender is the node, we dump that</param>
		/// <param name="e">This content the content of the node</param>
		private void Node_ContentChanged(object sender, ContentChangedEventArgs e)
		{
			Node_ContentChanged(((IExplorisValue) sender).Name, e.Content);
		}

		/// <summary>
		/// Dump all commands of a node. This changes, since they are queried when that node is registered by "values.Get"
		/// and they also may change when the user role changes.
		/// </summary>
		/// <param name="node">The node for which to dump available commands</param>
		private void Node_CommandsChanged(IExplorisValue node)
		{
			Console.WriteLine("Commands for {0}: {1}", node.Name, string.Join("", node.Commands.Select(n => "\n   name=" + n.Name + ", selection=" + n.Selection + ", help=" + n.Help)));
		}

		/// <summary>
		/// Dump the textual content of a node.
		/// </summary>
		/// <param name="nodeName">The name of the node</param>
		/// <param name="content">Content of the node</param>
		private void Node_ContentChanged(string nodeName, IContent content)
		{
			string text = (content == null) ? "((null))" : (content.Content == null) ? "(null)" : content.Content;
			// Status, Help and Unit are ignored.
			Console.WriteLine("New content of {0}: {1}", nodeName, text);
		}
	}
}
