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
using System.IO;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

using Thermo.Interfaces.InstrumentAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition;
using Thermo.Interfaces.InstrumentAccess_V1.Control.Acquisition.Modes;
using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.ExactiveAccess_V1.Control;
using Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition;
using Thermo.Interfaces.ExactiveAccess_V1.Control.Acquisition.Workflow;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class manages the instrument access and general functionality. The program will stop after
	/// some minutes.
	/// </summary>
	internal class Instrument : IDisposable
	{
		private int m_disposed = 0;
		private ScansOutput m_scansOutput = null;
		private AnalogOutput m_analogOutput = null;
		private MethodsTest m_methodsTest = null;
		private ScansTest m_scansTest = null;
		private ValuesTest m_valuesTest = null;
		private ManualResetEvent m_wait = null;
		private bool repeatingScanStarted = false;
		static private double m_frequency = 0;
		static private long m_startCounter = 0;
		static private DateTime m_startTime = DateTime.MinValue;

		/// <summary>
		/// Create a new <see cref="Instrument"/>
		/// </summary>
		/// <param name="arguments">arguments that drive the behavious of this instance</param>
		internal Instrument(Arguments arguments)
		{
			Arguments = arguments;

			Type type = null;
			object o = null;

			string idSource = null;

			if (arguments.ComAccess != null)
			{
				try
				{
					type = Type.GetTypeFromProgID(arguments.ComAccess, true);
				}
				catch (Exception e1)
				{
					Console.WriteLine("Cannot determine the type of the API via COM.");
					Console.WriteLine(e1.Message);
					if (e1.InnerException != null)
					{
						Console.WriteLine("(" + e1.InnerException.Message + ")");
					}
					Console.WriteLine();
					Console.WriteLine("Is the desired Exactive library registered?");
					Environment.Exit(1);
				}

				try
				{
					o = Activator.CreateInstance(type);
					if (o == null)
					{
						throw new Exception("Cannot create an instance of " + type.FullName);
					}
				}
				catch (Exception e2)
				{
					Console.WriteLine("Cannot create an object of the API via COM.");
					Console.WriteLine(e2.Message);
					if (e2.InnerException != null)
					{
						Console.WriteLine("(" + e2.InnerException.Message + ")");
					}
					Console.WriteLine();
					Console.WriteLine("Are the desired Exactive library and its dependencies registered?");
					Console.WriteLine("Is the machine platform well set?");
					Environment.Exit(1);
				}
				// Select the second-last word in the COM
				string[] comNames = arguments.ComAccess.Split('.');
				idSource = comNames[comNames.Length - 2];
			}
			else
			{
				string asmName;
				string typeName;
				asmName = arguments.ClassAccess.Key;
				typeName = arguments.ClassAccess.Value;
				idSource = arguments.InstrumentAccess ?? Arguments.InstrumentId;
				if (string.IsNullOrWhiteSpace(asmName))
				{
					string baseName = ((IntPtr.Size > 4) ? @"SOFTWARE\Wow6432Node\Finnigan\Xcalibur\Devices\" : @"SOFTWARE\Finnigan\Xcalibur\Devices\") + idSource;
					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(baseName))
					{
						if (key != null)
						{
							asmName = (string) key.GetValue("ApiFileName_Clr2_32_V1", null);
							typeName = (string) key.GetValue("ApiClassName_Clr2_32_V1", null);
						}
					}
				}
				if (string.IsNullOrWhiteSpace(asmName) || string.IsNullOrWhiteSpace(typeName))
				{
					Console.WriteLine("No API information has been gathered for instrument \"" + idSource + "\".");
					Console.WriteLine("Please, use \"-Class\" on the command line or select a different instrument using \"-Instrument\".");
					Environment.Exit(1);
				}
				try
				{
					string origPath = Directory.GetCurrentDirectory();
					try
					{
						Directory.SetCurrentDirectory(Path.GetDirectoryName(asmName));
						Assembly asm = Assembly.LoadFrom(asmName);
						o = asm.CreateInstance(typeName);
					}
					finally
					{
						try
						{
							Directory.SetCurrentDirectory(origPath);
						}
						catch
						{
						}
					}
				}
				catch (Exception e3)
				{
					Console.WriteLine("Cannot create an object of the API.");
					Console.WriteLine(e3.Message);
					if (e3.InnerException != null)
					{
						Console.WriteLine("(" + e3.InnerException.Message + ")");
					}
					Console.WriteLine();
					Console.WriteLine("Are the desired Exactive library and its dependencies registered?");
					Console.WriteLine("Is the machine platform well set?");
					Environment.Exit(1);
				}
			}

			Container = o as IInstrumentAccessContainer;
			if (Container == null)
			{
				Console.WriteLine("Cannot cast the gotten API to the desired type.");
				Console.WriteLine("Desired type: " + typeof(IInstrumentAccessContainer).FullName);
				Console.WriteLine("Supported types:");
				Console.WriteLine(o.GetType().FullName);
				foreach (Type supported in o.GetType().GetInterfaces())
				{
					Console.WriteLine(supported.FullName);
				}
				Console.WriteLine("Did you update the locally used interface library?");
				Environment.Exit(1);
			}

			// determine the instrument id from the source specification
			int firstDigit = idSource.Length;
			for (; (firstDigit > 0) && Char.IsDigit(idSource[firstDigit - 1]); firstDigit--)
				;

			int id;
			int.TryParse(idSource.Substring(firstDigit), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out id);	// Sets id to 0 on error

			IInstrumentAccess instrument = null;
			try
			{
				instrument = Container.Get((id == 0) ? 1 : id);
			}
			catch (FileNotFoundException e4)
			{
				Console.WriteLine(e4.Message);
				Console.WriteLine("Are all files registered properly?");
			}
			if (instrument == null)
			{
				Console.WriteLine("Cannot access the first instrument");
				Environment.Exit(1);
			}

			InstrumentInstance = instrument as IExactiveInstrumentAccess;
			if (InstrumentInstance == null)
			{
				Console.WriteLine("The first instrument is not of the Exactive family");
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// The final destructor releases allocated system resources.
		/// </summary>
		~Instrument()
		{
			// Let the GC dispose managed members itself.
			Dispose(false);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposeEvenManagedStuff">true to dispose managed and unmanaged resources; false to dispose unmanaged resources</param>
		protected void Dispose(bool disposeEvenManagedStuff)
		{
			// prevent double disposing
			if (Interlocked.Exchange(ref m_disposed, 1) != 0)
			{
				return;
			}

			if ((Arguments != null) && Arguments.RepeatingScanTest && repeatingScanStarted && (InstrumentInstance != null))
			{
				ScansTest.CancelRepeatingScan(InstrumentInstance);
			}

			if (disposeEvenManagedStuff)
			{
				if (m_scansOutput != null)
				{
					m_scansOutput.CloseDown();
					m_scansOutput = null;
				}
				if (m_analogOutput != null)
				{
					m_analogOutput.CloseDown();
					m_analogOutput = null;
				}
				if (m_valuesTest != null)
				{
					m_valuesTest.CloseDown();
					m_valuesTest = null;
				}
				if (m_scansTest != null)
				{
					m_scansTest.Dispose();
					m_scansTest = null;
				}
				if (m_methodsTest != null)
				{
					m_methodsTest.CloseDown();
					m_methodsTest = null;
				}
				if (InstrumentInstance != null)
				{
					InstrumentInstance.Dispose();
					InstrumentInstance = null;
				}
			}

			
			IInstrumentAccessContainer container = Container;
			Container = null;
			if (container != null)
			{
				IDisposable disp = container as IDisposable;
				if (disp != null)
				{
					disp.Dispose();
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		virtual public void Dispose()
		{
			// Dispose managed and unmanaged resources and tell GC we don't need the destructor getting called.
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Get access to the flag whether this object is disposed.
		/// </summary>
		internal bool Disposed { get { return m_disposed != 0; } }

		/// <summary>
		/// Access to the particular instrument instance.
		/// </summary>
		private IExactiveInstrumentAccess InstrumentInstance { get; set; }

		/// <summary>
		/// Access to the control interface of the instrument.
		/// </summary>
		private IExactiveControl Control { get { return InstrumentInstance.Control; } }

		/// <summary>
		/// Access to the acquisition interface of the instrument.
		/// </summary>
		private IExactiveAcquisition Acquisition { get { return Control.Acquisition; } }

		/// <summary>
		/// Access to the instrument container which is nice for time access.
		/// </summary>
		private IInstrumentAccessContainer Container { get; set; }

		[DllImport("kernel32.dll")]
		extern static private short QueryPerformanceCounter(ref long x);

		[DllImport("kernel32.dll")]
		extern static private short QueryPerformanceFrequency(ref long x);

		/// <summary>
		/// Return the current time in GMT with top-most accuracy.
		/// </summary>
		static internal DateTime Now
		{
			get
			{
				if (m_frequency == 0D)
				{
					long frequency = 0;
					if (QueryPerformanceFrequency(ref frequency) != 0)
					{
						// Let the function be compiled and ready:
						QueryPerformanceCounter(ref m_startCounter);
						m_startTime = DateTime.UtcNow;
						// Try to avoid a preemption by forcing a preemption
						Thread.Sleep(0);
						if (QueryPerformanceCounter(ref m_startCounter) != 0)
						{
							m_startTime = DateTime.Now;
							m_frequency = frequency;
						}
						else
						{
							return DateTime.Now;
						}
					}
					else
					{
						return DateTime.Now;
					}
				}

				long counter = 0;
				QueryPerformanceCounter(ref counter);
				double seconds = (counter - m_startCounter) / m_frequency;
				return m_startTime.AddSeconds(seconds);
			}
		}

		/// <summary>
		/// Access to the program arguments.
		/// </summary>
		internal Arguments Arguments { get; private set; }

		/// <summary>
		/// Cycle through the modes On/Off/Standby if we are already in one of the modes.
		/// We leave in On mode if we are already in one of the three modes.
		/// </summary>
		/// <param name="end">timeout</param>
		/// <returns>true if we are not in On/Off/Standby initially or if we reached On mode after the cycle.</returns>
		private bool CycleModes(DateTime end)
		{
			if (!Acquisition.WaitFor(TimeSpan.Zero, SystemMode.Off, SystemMode.Standby, SystemMode.On))
			{
				return true;
			}

			// Test every possible switch!
			KeyValuePair<IMode, SystemMode>[] switches = {
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateStandbyMode(), SystemMode.Standby),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateOnMode(), SystemMode.On),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateStandbyMode(), SystemMode.Standby),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateOffMode(), SystemMode.Off),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateOnMode(), SystemMode.On),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateOffMode(), SystemMode.Off),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateStandbyMode(), SystemMode.Standby),
															new KeyValuePair<IMode, SystemMode>(Acquisition.CreateOnMode(), SystemMode.On),
														 };
			if (Arguments.Verbose)
			{
				Console.WriteLine("Switching modes...");
			}

			DateTime start = DateTime.MaxValue;
			for (int i = 0; i < switches.Length; i++)
			{
				if (i == 1)
				{
					// After the first "change" we take the time. The "change" may not
					// had any influence if we had been in this state already. And don't
					// be too smart in detecting the correct state.
					start = Now;
				}
				if (Arguments.Chatty)
				{
					Console.Write("   to {0} ", switches[i].Value.ToString());
				}
				if (Acquisition.SetMode(switches[i].Key) != ChangeResult.Submitted)
				{
					if (Arguments.Chatty)
					{
						Console.WriteLine();
					}
					Console.WriteLine("Cannot set the instrument to {0}...", switches[i].Value.ToString());
					return false;
				}
				if (Arguments.Chatty)
				{
					Console.Write(" ...");
				}
				if (!Acquisition.WaitFor(end - Now, switches[i].Value))
				{
					if (Arguments.Chatty)
					{
						Console.WriteLine();
					}
					Console.WriteLine("Mode change to {0} didn't happen as expected...", switches[i].Value.ToString());
					return false;
				}
				if (Arguments.Chatty)
				{
					Console.WriteLine();
				}
			}
			TimeSpan switchingTime6 = Now - start;
			if (Arguments.Verbose)
			{
				Console.WriteLine("Average mode switching time: {0:F2} ms", switchingTime6.TotalMilliseconds / 6);
			}

			return true;
		}

		/// <summary>
		/// This method ensures the system runs, either by a user-provided method/acquisition or by an
		/// already running acquisition.
		/// </summary>
		/// <param name="started">time stamp when the machine has started</param>
		/// <returns>true if the system is running, false if the instrument is disconnected</returns>
		private bool EnsureRunningSystem(DateTime started)
		{
			DateTime end = started + TimeSpan.FromMilliseconds(Arguments.OperationTime);

			// wait until the instrument is connected
			if (Acquisition.WaitFor(TimeSpan.Zero, SystemMode.Disconnected, SystemMode.Maintenance, SystemMode.Malconfigured))
			{
				Console.WriteLine("Waiting for a connection...");

				if (!Acquisition.WaitForOtherThan(end - Now, SystemMode.Disconnected, SystemMode.Maintenance, SystemMode.Malconfigured))
				{
					return false;
				}

				// let the instrument settle for a maximum of 45 seconds after a reconnect.
				int settleTime = Math.Max(45, (int) (end - Now).TotalSeconds);
				Acquisition.WaitFor(TimeSpan.FromSeconds(settleTime), SystemMode.On, SystemMode.Off, SystemMode.Standby);
			}

			if (Arguments.ModeTest)
			{
				if (!CycleModes(end))
				{
					return false;
				}
			}

			Console.WriteLine("Mode=" + Acquisition.State.SystemMode);
			Console.WriteLine("State=" + Acquisition.State.SystemState);
			if (Acquisition.WaitForOtherThan(TimeSpan.Zero, SystemMode.Disconnected, SystemMode.Maintenance, SystemMode.Malconfigured, SystemMode.Off, SystemMode.Standby))
			{
				return true;
			}
			Console.WriteLine("Waiting for a system mode where data gets processed...");
			return Acquisition.WaitForOtherThan(end - Now, SystemMode.Disconnected, SystemMode.Maintenance, SystemMode.Malconfigured, SystemMode.Off, SystemMode.Standby);
		}

		/// <summary>
		/// Start a method if one if given in the arguments to the program or perform another method-like acquisition.
		/// For this, the instrument needs to be On.
		/// </summary>
		/// <param name="started">time stamp when the machine has started</param>
		/// <returns>true if the system is running somehow, false if the instrument is disconnected</returns>
		private bool StartMethodIfSystemIsIdle(DateTime started)
		{
			DateTime end = started + TimeSpan.FromMilliseconds(Arguments.OperationTime);

			if (Acquisition.WaitForOtherThan(TimeSpan.Zero, SystemMode.On))
			{
				return true;
			}

			// the instrument is on, test if we have to start an acquisition.
			IAcquisitionWorkflow methodWorkflow = null;

			if (Arguments.RunCount.HasValue)
			{
				try
				{
					// this may throw an exception if the arguments are invalid.
					methodWorkflow = Acquisition.CreateAcquisitionLimitedByCount(Arguments.RunCount.Value);
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception creating an acquisition limited by a count: " + e.Message);
					return false;
				}
			}
			if (Arguments.RunTime.HasValue)
			{
				try
				{
					// this may throw an exception if the arguments are invalid.
					methodWorkflow = Acquisition.CreateAcquisitionLimitedByDuration(Arguments.RunTime.Value);
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception creating an acquisition limited by time: " + e.Message);
					return false;
				}
			}
			if (Arguments.RunMethod != null)
			{
				try
				{
					// this may throw an exception if the arguments are invalid.
					methodWorkflow = Acquisition.CreateMethodAcquisition(Arguments.RunMethod);
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception creating an acquisition defined by a method: " + e.Message);
					return false;
				}
			}

			// start the method
			if (methodWorkflow != null)
			{
				if (Arguments.ModeTest)
				{
					// stop in standby mode to illustrate common value settings like starttrigger, RAW file, etc:
					methodWorkflow.Continuation = AcquisitionContinuation.Standby;
				}
				ChangeResult result = Control.Acquisition.StartAcquisition(methodWorkflow);
				switch (result)
				{
					case ChangeResult.ForeignControl:
						// taken under foreign control in between which is OK

					case ChangeResult.IllegalOperationState:
						// taken under foreign control in between which is OK

					case ChangeResult.InstrumentDisconnected:
						// The instrument disconnected in between, which is OK

					default:
						// Assume a harmless, unknown problem
						return true;

					case ChangeResult.UnknownRequestType:
						Console.WriteLine("The interface rejected the request to start an acquisition because of an unknown request type.");
						// let this stop to show the error prominently.
						return false;
						
					case ChangeResult.IllegalValues:
						Console.WriteLine("The interface rejected the request to start an acquisition because of invalid values.");
						// let this stop to show the error prominently.
						return false;
						
					case ChangeResult.Submitted:
						return true;
				}
			}

			// no acquisition by user selected
			return true;
		}

		/// <summary>
		/// Let the selected operations happen.
		/// </summary>
		internal void Go()
		{
			if (Arguments.Verbose)
			{
				Console.WriteLine("Instrument ID:   " + InstrumentInstance.InstrumentId);
				Console.WriteLine("Instrument Name: " + InstrumentInstance.InstrumentName);
			}
			Console.WriteLine("Connected:       " + InstrumentInstance.Connected);
			if (Arguments.Verbose)
			{
				DumpRoles();
			}

			try
			{
				DateTime started = Now;
				m_wait = new ManualResetEvent(InstrumentInstance.Connected);

				InstrumentInstance.ConnectionChanged += new EventHandler(ConnectionChanged);
				InstrumentInstance.UserRolesChanged += new EventHandler(UserRolesChanged);
				InstrumentInstance.ErrorsArrived += new EventHandler<ErrorsArrivedEventArgs>(Instrument_ErrorsArrived);

				if (Arguments.ShowStatus)
				{
					Acquisition.StateChanged += new EventHandler<StateChangedEventArgs>(Acquisition_StateChanged);
					m_scansOutput = new ScansOutput(InstrumentInstance, Arguments);
				}
				if (Arguments.ShowScanOutput)
				{
					m_scansOutput = new ScansOutput(InstrumentInstance, Arguments);
				}
				if (Arguments.ShowAnalogOutput)
				{
					m_analogOutput = new AnalogOutput(InstrumentInstance);
				}

				if (EnsureRunningSystem(started))
				{
					if (StartMethodIfSystemIsIdle(started))
					{
						if (Arguments.ValuesTest)
						{
							m_valuesTest = new ValuesTest(InstrumentInstance, Arguments);
						}
						if (Arguments.MethodsTest != null)
						{
							m_methodsTest = new MethodsTest(InstrumentInstance, Arguments);
						}
						if (Arguments.RepeatingScanTest)
						{
							repeatingScanStarted = ScansTest.SetRepeatingScan(InstrumentInstance, Arguments.Verbose);
						}
						if (Arguments.ScansTest)
						{
							m_scansTest = new ScansTest(InstrumentInstance, Arguments);
						}

						int span = Arguments.OperationTime - (int) (Now - started).TotalMilliseconds;
						if (span > 0)
						{
							Console.WriteLine("Sleeping for " + (span / 1000) + "s");
							Thread.Sleep(span);
						}
					}
				}
			}
			finally
			{
				InstrumentInstance.ErrorsArrived -= new EventHandler<ErrorsArrivedEventArgs>(Instrument_ErrorsArrived);
				InstrumentInstance.UserRolesChanged -= new EventHandler(UserRolesChanged);
				InstrumentInstance.ConnectionChanged -= new EventHandler(ConnectionChanged);
			}
		}

		/// <summary>
		/// When the instrument changes its state, the new state is dumped here.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">contains the state that gets dumped</param>
		private void Acquisition_StateChanged(object sender, StateChangedEventArgs e)
		{
			IState state = e.State;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("STATE: " + state.Description);
			sb.AppendLine("       " + state.SystemState);
			sb.AppendLine("Mode:  " + state.SystemMode);
			if (state.ElapsedRuntime.HasValue)
			{
				sb.AppendLine("Done:  " + state.ElapsedRuntime.Value.ToString("F2") + "%");
			}
			Console.Write(sb.ToString());
		}

		/// <summary>
		/// When the connection state changes we emit a message about that.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void ConnectionChanged(object sender, EventArgs e)
		{
			ManualResetEvent wait = m_wait;
			if (InstrumentInstance.Connected && (wait != null))
			{
				wait.Set();
			}
			Console.WriteLine("The connection to the instrument " + ((InstrumentInstance.Connected) ? "has been established" : "dropped"));
		}

		/// <summary>
		/// Dump the users role and the permissions and privileges.
		/// </summary>
		private void DumpRoles()
		{
			Console.WriteLine("Current user role: " + InstrumentInstance.CurrentUserRole);
			Console.WriteLine("Possible user roles: " + string.Join(", ", InstrumentInstance.UserRoles));
			Console.WriteLine("Granted licenses: " + string.Join(", ", InstrumentInstance.Licenses));
		}

		/// <summary>
		/// When the user role or the permissions/privileges change we emit a message about that.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">doesn't matter</param>
		private void UserRolesChanged(object sender, EventArgs e)
		{
			DumpRoles();
		}

		/// <summary>
		/// When errors from the instrument arrived we publish them.
		/// </summary>
		/// <param name="sender">doesn't matter</param>
		/// <param name="e">contains the messages to be dumped</param>
		private void Instrument_ErrorsArrived(object sender, ErrorsArrivedEventArgs e)
		{
			foreach (IError error in e.Errors)
			{
				Console.WriteLine("ERROR>>> " + error.Content.ToUpper());
			}
		}
	}
}
