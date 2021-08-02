using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Win32;

using Thermo.Interfaces.ExplorisAccess_V1;

namespace InitialConnect
{
	class Connect
	{
		const string DefaultBasePath = "Thermo\\Exploris";			// Default path of the instrument's config file.
		const string DefaultRegistry = "SOFTWARE\\Thermo Exploris";	// Name of the instrument's host in the registry.
		const string XmlRoot = "DataSystem";						// base element in the config file
		const string ApiFileNameDescriptor = "ApiFileName";			// Registry connect string of the assembly
		const string ApiClassNameDescriptor = "ApiClassName";       // Registry connect string of the implementing class


		/// <summary>
		/// Create an instance of the API object and return it. Exceptions will be thrown on errors.
        /// Please also check "2 KeepInstrumentConnection\Connection.cs" for a version of generic use.
		/// </summary>
		/// <returns>Returns a new instance of the API object representing an Exploris series instrument.</returns>
		static internal IExplorisInstrumentAccessContainer GetApiInstance()
		{
			string basePath = null;
			// Check the registry first. There should be an entry where programs (and also data) is located.
			using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				using (RegistryKey merkur = key.OpenSubKey(DefaultRegistry))
				{
					if (merkur != null)
					{
						basePath = (string) merkur.GetValue("data", null /* in case that only the key is present */);
					}
				}
			}

			if ((basePath == null) || !File.Exists(Path.Combine(basePath, XmlRoot + ".xml")))
			{
				// This condition can be treated already as an error. But is shall be demonstrated on how to find API otherwise.
				// Note that the preferred way is using the registry.
				basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), DefaultBasePath);
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(Path.Combine(basePath, XmlRoot + ".xml"));
			string filename = doc[XmlRoot][ApiFileNameDescriptor].InnerText.Trim();
			string classname = doc[XmlRoot][ApiClassNameDescriptor].InnerText.Trim();

			if (!File.Exists(filename))
			{
				// assume GAC specification, this is the last chance we have
				return (IExplorisInstrumentAccessContainer) Assembly.Load(filename).CreateInstance(classname);
			}
			return (IExplorisInstrumentAccessContainer) Assembly.LoadFrom(filename).CreateInstance(classname);
		}
	}
}
