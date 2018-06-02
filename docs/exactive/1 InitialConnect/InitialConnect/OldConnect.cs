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

using Thermo.Interfaces.InstrumentAccess_V1;

namespace InitialConnect
{
	/// <summary>
	/// This is a deprecated way of connecting to an Exactive Series instrument.
	/// Using COM works only in some cases, and error messages don't explain reasons.
	/// Instantiation is done by a Type lookup and final instantiation.
	/// </summary>
	class OldConnect
	{
		const string InstrumentName = "Thermo Exactive"; // Name of the instrument in the registry.
		const string ApiFileNameDescriptor = "ApiFileName_Clr2_32_V1"; // Registry connect string of the assembly
		const string ApiClassNameDescriptor = "ApiClassName_Clr2_32_V1"; // Registry connect string of the implementing class

		// Create an instance of the API object and return it. Exceptions will be thrown on errors.
		private IInstrumentAccessContainer GetApiInstance()
		{
			Type type = Type.GetTypeFromProgID("Thermo Exactive.API_Clr2_32_V1", true);
			object o = Activator.CreateInstance(type);
			return (IInstrumentAccessContainer) o;
		}

		// perform the work:
		internal void DoJob()
		{
			IInstrumentAccessContainer container = GetApiInstance();
			using (IInstrumentAccess instrument = container.Get(1))
			{
				// we just print the instrument name.
				Console.WriteLine(instrument.InstrumentName);
			}
		}
	}
}
