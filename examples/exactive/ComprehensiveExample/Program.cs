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