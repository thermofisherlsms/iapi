#region legal notice
// Copyright(c) 2016 - 2018 Thermo Fisher Scientific - LSMS
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
using System.Reflection;
using Microsoft.Win32;

using Thermo.Interfaces.ExactiveAccess_V1;
using Thermo.Interfaces.InstrumentAccess_V1;

namespace Workflow
{
	/// <summary>
	/// This class hosts the single method <see cref="GetFirstInstrument"/> to get access to the first Exactive API controlled instrument.
	/// </summary>
	static class Connection
	{
		const string InstrumentName = "Thermo Exactive"; // Name of the instrument in the registry.
		const string ApiFileNameDescriptor = "ApiFileName_Clr2_32_V1"; // Registry connect string of the assembly
		const string ApiClassNameDescriptor = "ApiClassName_Clr2_32_V1"; // Registry connect string of the implementing class

		static private IInstrumentAccessContainer m_container = null;

		/// <summary>
		/// Create an instance of the API object and return it. Exceptions will be thrown on errors.
		/// </summary>
		static private IInstrumentAccessContainer GetContainer()
		{
			string baseName = @"SOFTWARE\Finnigan\Xcalibur\Devices\" + InstrumentName;
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
		/// Open a connection to the API and return the first instrument. Expect exceptions on errors accessing the API.
		/// </summary>
		/// <returns>Returns an implementation of the first instrument of the API that REQUIRES a call to Dispose finally.</returns>
		static internal IExactiveInstrumentAccess GetFirstInstrument()
		{
			if (m_container == null)
			{
				m_container = GetContainer();
			}
			return (IExactiveInstrumentAccess) m_container.Get(1);
		}
	}
}
