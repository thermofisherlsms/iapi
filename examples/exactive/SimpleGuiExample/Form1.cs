#region legal notice
//
// Copyright 2011 - 2014 Thermo Fisher Scientific Inc. or subsidiaries
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
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
