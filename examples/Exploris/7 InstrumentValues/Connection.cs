using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Microsoft.Win32;

using Thermo.Interfaces.ExplorisAccess_V1;

namespace Common
{
	/// <summary>
	/// This class offers the method <see cref="Get"/>, that allows to get access to the API of Exploris.
	/// Other methods are used only if changing states in the instrument, which require an API license.
	/// </summary>
	static class Connection
	{
		const string DefaultBasePath = "Thermo\\Exploris";          // Default path of the instrument's config file.
		const string DefaultRegistry = "SOFTWARE\\Thermo Exploris"; // Name of the instrument's host in the registry.
		const string XmlRoot = "DataSystem";                        // base element in the config file
		const string ApiFileNameDescriptor = "ApiFileName";         // Registry connect string of the assembly
		const string ApiClassNameDescriptor = "ApiClassName";       // Registry connect string of the implementing class
		const string ApiLicenseName = "Api";						// This is a fixed term

		/// <summary>
		/// Create an instance of the API object and return it. Online access is being demanded. Exceptions will be thrown on errors.
		/// </summary>
		/// <returns>Returns a new instance of the API object representing an Exploris series instrument.</returns>
		static internal IExplorisInstrumentAccessContainer Get()
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

			IExplorisInstrumentAccessContainer retval;
			if (!File.Exists(filename))
			{
				// assume GAC specification
				retval = (IExplorisInstrumentAccessContainer) Assembly.Load(filename).CreateInstance(classname);
			}
			else
			{
				retval = (IExplorisInstrumentAccessContainer) Assembly.LoadFrom(filename).CreateInstance(classname);
			}

            // Instruct API to get connected to the instrument it represents. Otherwise only offline functionality would
            // be available. Starting with Exploris 3.1, this call would block for a short period of time (500ms) to have the
            // connection to the core Service (acting as a relay) being established. That would allow direct use of commands.
            // There is still no guarantee that the connection is there, check states!
			retval.StartOnlineAccess();
			return retval;
		}

#region Only needed for functionality changing the state of the instrument or getting node access
		/// <summary>
		/// Wait until the core Service got connected to the API.
		/// </summary>
		/// <param name="container">The instrument container to use</param>
		/// <param name="end">ending time until we for for being connected</param>
		/// <returns>Returns true if the service became connected in time, false otherwise</returns>
		static private bool WaitServiceConnected(IExplorisInstrumentAccessContainer container, DateTime utcEnd)
		{
			// Before Exploris 3.1, several ServiceConnectionChanged events might have been fired before the
			// connection really got established. Handle that by a repeating loop. Assume that waiting isn't
			// necessary at all.
			while (!container.ServiceConnected)
			{
				// Wait until the service got connected.
				using (AutoResetEvent connectionChanged = new AutoResetEvent(false))
				{
					// We use an event semaphore for waiting. Just checking the connection from time to time is also possible.
					EventHandler serviceConnectionChanges = (o, e) => { connectionChanged.Set(); };
					container.ServiceConnectionChanged += serviceConnectionChanges;
					try
					{
						// The connection may became available between top of the method and establishing the event handler.
						// Check for that first.
						if (!container.ServiceConnected)
						{
							TimeSpan maxWait = utcEnd - DateTime.UtcNow;
							if ((maxWait < TimeSpan.Zero) || !connectionChanged.WaitOne(maxWait))
							{
								return false;
							}
						}
					}
					finally
					{
						container.ServiceConnectionChanged -= serviceConnectionChanges;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Wait until licensing information is available. Licenses are requested and returned with some microseconds delay
		/// when a connection to the underlying core Service gets available. The instrument may become available before
		/// any license becomes available. Although this has changed in Exploris 3.1, for which the licenses are requested
		/// before <paramref name="container"/>.ServiceConnected is set to true, this code is necessary for older
		/// versions and not wrong for newer versions because API layer would throw a PrivilegeNotHeldException when
		/// accessing write functionality instead of having the return value of this method.
		/// <para>
		/// This routine waits for licenses becoming available and checks for an API license. There is no need to call this
		/// method if data or states are only read.
		/// </para>
		/// <para>
		/// The call will not work on an Exploris 120, which doesn't support API.
		/// </para>
		/// <para>
		/// The call will also not work if the API license has some kind of specification regarding software version, etc.
		/// But in that case, access to API fails later. In either case, the call will return not too early for proper
		/// continuation if that is possible at all.
		/// </para>
		/// </summary>
		/// <param name="container">The container used to check for service connections</param>
		/// <param name="instrument">The instrument to check licenses for</param>
		/// <param name="duration">Maximum time to wait for a result</param>
		/// <returns>Returns true if an API license is present and access to restricted functionality is granted in principle, false otherwise.</returns>
		static internal bool WaitForApiLicense(IExplorisInstrumentAccessContainer container, IExplorisInstrumentAccess instrument, TimeSpan duration)
		{
			if (instrument.Licenses.Where(n => n.Features.Contains(ApiLicenseName, StringComparer.OrdinalIgnoreCase)).Any())
			{
				// bail out fast for an available API license.
				return true;
			}

			DateTime finalExpiration = DateTime.UtcNow + duration;

			// We have two options to get results. We wait until the connection to the service gets available AND we wait for licenses.
			// The later thing can be done in two ways. The maybe delayed but easy-to-code way is just to wait some time.
			// The faster way with a lot more code is recommended if less time consumption is crucial.

			if (!WaitServiceConnected(container, finalExpiration))
			{
				return false;
			}

			// Wait from now on for a maximum of 200 ms. That is absolutely sufficient. Using Thread.Sleep must be avoided.
			// The easy way if to just perform a "Thread.CurrentThread.Join(200);", but we try not to waste so much time.
			DateTime plus200ms = DateTime.UtcNow + TimeSpan.FromMilliseconds(200);
			finalExpiration = (plus200ms < finalExpiration) ? plus200ms : finalExpiration;

			// We need some event semaphores on which we can wait. Putting them into usings will let them disposed as soon as possible.
			using (AutoResetEvent licensesChanged = new AutoResetEvent(false))
			{
				EventHandler licensesAvailabilityChanges = (o, e) => { licensesChanged.Set(); };
				// Licenses may trickle in from time to time. We must wait in a loop.
				try
				{
					instrument.LicensesChanged += licensesAvailabilityChanges;
					while (!instrument.Licenses.Where(n => n.Features.Contains(ApiLicenseName, StringComparer.OrdinalIgnoreCase)).Any())
					{
						TimeSpan maxWait = finalExpiration - DateTime.UtcNow;
						if ((maxWait < TimeSpan.Zero) || !licensesChanged.WaitOne(maxWait))
						{
							return false;
						}
					}
					return true;
				}
				finally
				{
					instrument.LicensesChanged -= licensesAvailabilityChanges;
				}			
			}
		}
#endregion Only needed for functionality changing the state of the instrument or getting node access
	}
}