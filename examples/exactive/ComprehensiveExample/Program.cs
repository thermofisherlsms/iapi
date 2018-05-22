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
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ComprehensiveExample
{
	public class Program
	{
		internal const string TimeFormat = "HH:mm:ss.fff";

		/// <summary>
		/// Test the Access library of the instrument.
		/// </summary>
		[MTAThread]
		static public void Main(string[] args)
		{
			Arguments arguments = new Arguments(args);

			using (Instrument instrument = new Instrument(arguments))
			{
				instrument.Go();
			}
			Console.WriteLine("Instrument disposed.");
			GC.Collect();

			Process p = Process.GetCurrentProcess();
			Console.WriteLine("Process timings: user={0:F3}, priv={1:F3}, tot={2:F3} seconds", p.UserProcessorTime.TotalSeconds, p.PrivilegedProcessorTime.TotalSeconds, p.TotalProcessorTime.TotalSeconds);
			Console.WriteLine("press any key...");
			Console.ReadKey();

			Environment.Exit(0);
		}

	}
}