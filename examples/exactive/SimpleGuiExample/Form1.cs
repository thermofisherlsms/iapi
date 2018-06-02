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
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

using Thermo.Interfaces.InstrumentAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control.InstrumentValues;
using System.Reflection;


namespace SimpleGuiExample
{
	/// <summary>
	/// This trivial class demonstrates access to an Exactive Series instrument
	/// in a GUI.
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Name of the instrument in the registry.
		/// </summary>
		private const string InstrumentName = "Thermo Exactive";

		/// <summary>
		/// This string will be used as a registry value name to access the file name of the API assembly.
		/// </summary>
		private const string ApiFileNameDescriptor = "ApiFileName_Clr2_32_V1";

		/// <summary>
		/// This string will be used as a registry value name to access the class name of the API.
		/// </summary>
		private const string ApiClassNameDescriptor = "ApiClassName_Clr2_32_V1";

		public Form1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Place the content of the exception in the status box.
		/// </summary>
		/// <param name="e">Exception to dump</param>
		private void DumpExceptionToStatusBox(Exception e)
		{
			// collect messages first
			List<string> content = new List<string>();
			content.Add(e.Message);
			Exception inner = e.InnerException;
			while (inner != null)
			{
				content.Add("  containing " + inner.Message);
				inner = inner.InnerException;
			}

			// append the outmost stack trace
			content.Add("");
			content.AddRange(e.StackTrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
			TextBoxStatus.Lines = content.ToArray();
		}

		/// <summary>
		/// Create an instance of the API object and return it.
		/// Exceptions will be thrown on errors.
		/// </summary>
		private IInstrumentAccessContainer GetApiInstance()
		{
			string baseName = ((IntPtr.Size > 4) ? @"SOFTWARE\Wow6432Node\Finnigan\Xcalibur\Devices\" : @"SOFTWARE\Finnigan\Xcalibur\Devices\") + InstrumentName;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(baseName))
			{
				if (key != null)
				{
					string asmName = (string) key.GetValue(ApiFileNameDescriptor, null);
					string typeName = (string) key.GetValue(ApiClassNameDescriptor, null);
					if (!string.IsNullOrEmpty(asmName) && !string.IsNullOrEmpty(typeName))
					{
						Assembly asm = Assembly.LoadFrom(asmName);
						return (IInstrumentAccessContainer) asm.CreateInstance(typeName);
					}
				}
			}
			throw new Exception("Cannot find API information of instrument \"" + InstrumentName + "\" in the registry.");
		}

		/// <summary>
		/// The user requests to test the presence of the API.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void ButtonInterfaceTest_Click(object sender, EventArgs e)
		{
			try
			{
				IInstrumentAccessContainer container = GetApiInstance();
				using (IInstrumentAccess instrument = container.Get(1))
				{
					// get access to the first instrument, but release it immediately.
				}

				// Success!
				TextBoxStatus.Text = "The API of Exactive Series instruments is present and functional.";
			}
			catch (Exception ex)
			{
				DumpExceptionToStatusBox(ex);
			}
		}

		/// <summary>
		/// The user wants to actively change a value. This may or may not lead
		/// to an exception if privileges are missing or if the proper role is not assumed.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void ButtonSprayVoltage_Click(object sender, EventArgs e)
		{
			try
			{
				IInstrumentAccessContainer container = GetApiInstance();
				using (IInstrumentAccess instrument = container.Get(1))
				{
					TextBoxStatus.Text = "Waiting for spray voltage to become available...";
					IValue value = instrument.Control.InstrumentValues.Get("SourceSprayVoltage");

					DateTime timeout = DateTime.Now + TimeSpan.FromSeconds(5);
					while (value.Content.Content == null)
					{
						// Thread.Join continues to process the current message loop
						Thread.CurrentThread.Join(10);
						if (DateTime.Now > timeout)
						{
							break;
						}
					}

					if (value.Content.Content == null)
					{
						TextBoxStatus.Text = "Either no link to the instrument or the spray voltage is not a tuneable value of the source.";
					}
					else
					{
						TextBoxStatus.Text = "About to set the spray voltage...";
						Application.DoEvents();
						string previous = value.Content.Content;
						bool ok = value.Set("100");
						TextBoxStatus.Text = "Changing spray voltage from " + previous + " to 100: " + ((ok) ? "OK" : "impossible to set");
					}
				}
			}
			catch (Exception ex)
			{
				DumpExceptionToStatusBox(ex);
			}
		}
	}
}
