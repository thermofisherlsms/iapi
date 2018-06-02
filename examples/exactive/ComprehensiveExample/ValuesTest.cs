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
using System.Linq;
using System.Text;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.ExactiveAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;
using Thermo.Interfaces.InstrumentAccess_V1.Control;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class provides tests for the IInstrumentValues interface.
	/// </summary>
	internal class ValuesTest
	{
		private IExactiveValue m_instrumentAcquisition;
		private List<IExactiveValue> m_nodes = new List<IExactiveValue>();

		/// <summary>
		/// Create a new <see cref="ValuesTest"/> and start the performance immediately.
		/// </summary>
		/// <param name="instrument">the instrument instance</param>
		/// <param name="arguments">program arguments</param>
		internal ValuesTest(IExactiveInstrumentAccess instrument, Arguments arguments)
		{
			Arguments = arguments;

			m_instrumentAcquisition = (IExactiveValue) instrument.Control.InstrumentValues.Get(0x41);
			m_nodes.Add(m_instrumentAcquisition);
			m_instrumentAcquisition.ContentChanged += new EventHandler<ContentChangedEventArgs>(ContentChanged);
			m_instrumentAcquisition.CommandsChanged += new EventHandler(CommandsChanged);
			Console.WriteLine("instrument acquisition node: ID={0}, name={1}", m_instrumentAcquisition.Id, m_instrumentAcquisition.Name);

			if (Arguments.Chatty)
			{
				foreach (string s in instrument.Control.InstrumentValues.ValueNames)
				{
					try
					{
						IExactiveValue node = (IExactiveValue) instrument.Control.InstrumentValues.Get(s);
						if (node.Id != 0x41)
						{
							m_nodes.Add(node);

							node.ContentChanged += new EventHandler<ContentChangedEventArgs>(ContentChanged);
							node.CommandsChanged += new EventHandler(CommandsChanged);
						}
					}
					catch
					{
					}
				}
			}
		}

		/// <summary>
		/// Cleanup this instance.
		/// </summary>
		internal void CloseDown()
		{
			foreach (IExactiveValue node in m_nodes)
			{
				node.CommandsChanged -= new EventHandler(CommandsChanged);
				node.ContentChanged -= new EventHandler<ContentChangedEventArgs>(ContentChanged);
			}
			m_nodes.Clear();
		}

		/// <summary>
		/// Access to the program arguments.
		/// </summary>
		internal Arguments Arguments { get; private set; }

		/// <summary>
		/// Return the status text of a status value.
		/// </summary>
		/// <param name="contentStatus">value of the status enumeration of an instrument value</param>
		private string Status(int contentStatus)
		{
			switch (contentStatus)
			{
				case 0: return "OK";
				case 1: return "Info";
				case 2: return "Warning";
				case 3: return "Error";
				case 4: return "Fatal";
				default: break;
			}
			return "Unknown";
		}

		/// <summary>
		/// Called when the content change we print them.
		/// </summary>
		/// <param name="sender">the instrument value which value change</param>
		/// <param name="e">used to extract the value</param>
		private void ContentChanged(object sender, ContentChangedEventArgs e)
		{
			IValue value = sender as IValue;
			if (value != null)
			{
				if (e.Content == null)
				{
					Console.WriteLine("Value of ({0}) changed to null.", value.Name);
				}
				else if (e.Content == null)
				{
					Console.WriteLine("Value of ({0}) is unknown to the instrument or the connection is lost.", value.Name);
				}
				else
				{
					IContent content = e.Content;
					Console.WriteLine("Value of ({0}) changed to '{1}', unit='{2}', status={3}, help='{4}'", value.Name, content.Content, content.Unit, Status(content.Status), content.Help);
				}
			}
		}

		/// <summary>
		/// Called when the set of possible commands change we print them if we act verbosely.
		/// </summary>
		/// <param name="sender">the instrument value which commands change</param>
		/// <param name="e">doesn't matter</param>
		void CommandsChanged(object sender, EventArgs e)
		{
			IExactiveValue value = sender as IExactiveValue;
			if (value != null)
			{
				if (value.Commands.Length == 0)
				{
					Console.WriteLine("Commands for ({0}) are not registered", value.Name);
				}
				else
				{
					if (Arguments.Verbose)
					{
						Console.WriteLine("Commands for ({0}):", value.Name);
						foreach (IParameterDescription command in value.Commands)
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendFormat("   '{0}' ", command.Name);
							if (command.Selection == "")
							{
								sb.AppendFormat("doesn't accept an argument, help: {0}", command.Help);
							}
							else
							{
								sb.AppendFormat("accepts '{0}', default='{1}', help: {2}", command.Selection, command.DefaultValue, command.Help);
							}
							Console.WriteLine(sb.ToString());
						}
					}
					else
					{
						Console.WriteLine("Commands for ({0}): {1}", value.Name, string.Join(", ", value.Commands.Select(n => n.Name).ToArray()));
					}
				}
			}
		}
	}
}
