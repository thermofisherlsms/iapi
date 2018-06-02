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
using System.Reflection;
using System.IO;
using System.Globalization;

namespace ComprehensiveExample
{
	/// <summary>
	/// This class parses arguments and provides an easy access.
	/// </summary>
	internal class Arguments
	{
		public const string InstrumentId = "Thermo Exactive";
		public const string InstrumentIdCom = InstrumentId + ".API_Clr2_32_V1";

		/// <summary>
		/// Create a new <see cref="Arguments"/> and parse the passed values.
		/// </summary>
		/// <param name="arguments">list of program arguments</param>
		internal Arguments(string[] arguments)
		{
			OperationTime = 100 * 1000;
			if (arguments.Length == 0)
			{
				Console.WriteLine("(request help for command line options by specifying \"-?\")");
			}
			for (int i = 0; i < arguments.Length; i++)
			{
				string arg = arguments[i];
				if (arg.StartsWith("#"))
				{
					// comment
					continue;
				}
				if (arg == "-verbose")
				{
					Verbose = true;
					continue;
				}
				if (arg == "-chatty")
				{
					Verbose = true;
					Chatty = true;
					continue;
				}
				if (arg == "-analogOutput")
				{
					ShowAnalogOutput = true;
					continue;
				}
				if (arg == "-scanOutput")
				{
					ShowScanOutput = true;
					continue;
				}
				if (arg == "-status")
				{
					ShowStatus = true;
					continue;
				}
				if (arg.StartsWith("-COM="))
				{
					ComAccess = arg.Substring(5);
					InstrumentAccess = null;
					ClassAccess = new KeyValuePair<string,string>();
					continue;
				}
				if (arg.StartsWith("-Instrument="))
				{
					InstrumentAccess = arg.Substring(12);
					ComAccess = null;
					ClassAccess = new KeyValuePair<string,string>();
					continue;
				}
				if (arg.StartsWith("-Class="))
				{
					string[] token = arg.Substring(7).Split(',');
					if (token.Length != 2)
					{
						Usage();
					}
					ClassAccess = new KeyValuePair<string, string>(token[0].Trim(), token[1].Trim());
					ComAccess = null;
					InstrumentAccess = null;
					continue;
				}
				if (arg.StartsWith("-operationTime="))
				{
					string time = arg.Substring(15);
					int seconds;
					if (!int.TryParse(time, out seconds))
					{
						Usage();
					}
					if ((seconds <= 0) || (seconds >= int.MaxValue / 1000))
					{
						Usage();
					}
					OperationTime = seconds * 1000;
					continue;
				}
				if (arg == "-modeTest")
				{
					ModeTest = true;
					continue;
				}
				if (arg == "-repeatingScanTest")
				{
					RepeatingScanTest = true;
					continue;
				}
				if (arg == "-scansTest")
				{
					ScansTest = true;
					continue;
				}
				if (arg.StartsWith("-methodsTest="))
				{
					MethodsTest = arg.Substring(13);
					continue;
				}
				if (arg == "-valuesTest")
				{
					ValuesTest = true;
					continue;
				}
				if (arg.StartsWith("-run="))
				{
					string runarg = arg.Substring(5);
					int count;
					if (int.TryParse(runarg, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out count))
					{
						RunCount = count;
						RunTime = null;
						RunMethod = null;
						continue;
					}
					double min;
					if (runarg.EndsWith("min") && double.TryParse(runarg.Substring(0, runarg.Length - 3), NumberStyles.Float, NumberFormatInfo.CurrentInfo, out min))
					{
						RunCount = null;
						RunTime = TimeSpan.FromMinutes(min);
						RunMethod = null;
						continue;
					}
					runarg = runarg.Trim();
					if ((runarg != "") && File.Exists(runarg))
					{
						RunCount = null;
						RunTime = null;
						RunMethod = runarg;
						continue;
					}
				}

				Usage();
			}
		}

		/// <summary>
		/// Access to the non-option arguments.
		/// </summary>
		internal string[] FreeArguments { get; private set; }

		/// <summary>
		/// Shall output be verbose?
		/// </summary>
		internal bool Verbose { get; private set; }

		/// <summary>
		/// Shall output be extremely verbose?
		/// </summary>
		internal bool Chatty { get; private set; }

		/// <summary>
		/// Shall analog output be presented to the user?
		/// </summary>
		internal bool ShowAnalogOutput { get; private set; }

		/// <summary>
		/// Shall scan output be presented to the user?
		/// </summary>
		internal bool ShowScanOutput { get; private set; }

		/// <summary>
		/// Shall the status be shown?
		/// </summary>
		internal bool ShowStatus { get; private set; }

		/// <summary>
		/// Access to the COM string to get access to the instrument at all.
		/// </summary>
		internal string ComAccess { get; private set; }

		/// <summary>
		/// Access to the Instrument string to get access to the instrument at all.
		/// </summary>
		internal string InstrumentAccess { get; private set; }

		/// <summary>
		/// Access to the Class strings to get access to the instrument at all.
		/// </summary>
		internal KeyValuePair<string, string> ClassAccess { get; private set; }

		/// <summary>
		/// OperationTime of the runtime of this program in milliseconds.
		/// </summary>
		internal int OperationTime { get; private set; }

		/// <summary>
		/// Shall the test for switching between run modes be performed?
		/// </summary>
		internal bool ModeTest { get; private set; }

		/// <summary>
		/// Shall the test for setting a repeating scan on IScans be performed?
		/// We just set the first mass to 400.
		/// </summary>
		internal bool RepeatingScanTest { get; private set; }

		/// <summary>
		/// Shall the test on IScans be performed?
		/// </summary>
		internal bool ScansTest { get; private set; }

		/// <summary>
		/// Shall the test on IMethods be performed? null if no, otherwise the name of the method to deal with.
		/// </summary>
		internal string MethodsTest { get; private set; }

		/// <summary>
		/// Shall the test on IInstrumentValues be performed?
		/// </summary>
		internal bool ValuesTest { get; private set; }

		/// <summary>
		/// Shall the program start an acquisition with RunCount scans?
		/// </summary>
		internal int? RunCount { get; private set; }

		/// <summary>
		/// Shall the program start an acquisition with the passed duration?
		/// </summary>
		internal TimeSpan? RunTime { get; private set; }

		/// <summary>
		/// Shall the program start an acquisition with the passed method?
		/// </summary>
		internal string RunMethod { get; private set; }

		/// <summary>
		/// Dump the usage to the user and die.
		/// </summary>
		static void Usage()
		{
			string name = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			Console.WriteLine("usage: " + name + " [options]");
			Console.WriteLine("possible options:");
			Console.WriteLine("-verbose               provide extra output");
			Console.WriteLine("-chatty                be very verbose (implies -verbose)");
			Console.WriteLine("-analogOutput          present analog data");
			Console.WriteLine("-scanOutput            present scan data (checks -verbose and -chatty)");
			Console.WriteLine("-status                dump the current status each time it changes");
			Console.WriteLine("-COM=<id>              use id instead of using -Class for connecting.");
			Console.WriteLine("                       \"" + InstrumentIdCom + "\" may be a good choice");
			Console.WriteLine("-Instrument=<id>       use id instead of \"" + InstrumentId + "\"");
			Console.WriteLine("                       to get access to the instrument");
			Console.WriteLine("-Class=<asm,class>     use the pair of assembly name and class name");
			Console.WriteLine("                       to get access to the instrument");
			Console.WriteLine("-operationTime=<s>     works for s seconds (default = 100)");
			Console.WriteLine("-modeTest              switch between different run modes");
			Console.WriteLine("-repeatingScanTest     set a repeating scan to test the IScans interface during execution");
			Console.WriteLine("-scansTest             perform a test of the IScans interface");
			Console.WriteLine("-methodsTest=<name>    perform a test of the IMethods interface with the passed");
			Console.WriteLine("                       methods file name");
			Console.WriteLine("-valuesTest            perform a test of the IInstrumentValues interface");
			Console.WriteLine("-run=<count>           let an acquisition start for count scans");
			Console.WriteLine("-run=<time>min         let an acquisition start for time minutes");
			Console.WriteLine("-run=<methodfilename>  let an acquisition start with the given method");

			Environment.Exit(1);
		}
	}
}
