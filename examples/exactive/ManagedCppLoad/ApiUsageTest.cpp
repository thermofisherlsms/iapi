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
#include "StdAfx.h"
#include "ApiUsageTest.h"
#include "HandlerToResetEvent.h"

using namespace System::Reflection;
using namespace Microsoft::Win32;

using namespace Thermo::Interfaces::InstrumentAccess_V1;
using namespace Thermo::Interfaces::InstrumentAccess_V1::Control::Acquisition;
using namespace Thermo::Interfaces::ExactiveAccess_V1::Control::Acquisition;

namespace ManagedCppLoad
{
	ApiUsageTest::ApiUsageTest()
	{
	}
	ApiUsageTest::!ApiUsageTest()
	{
		m_instrument = nullptr;	// no active destruction here
	}
	ApiUsageTest::~ApiUsageTest()
	{
		this->!ApiUsageTest();
	}

	// Return true if loading the API has been successful
	bool ApiUsageTest::Load()
	{
		Console::Write(L"Loading Exactive Series API...");
		try
		{
			Object^ o = LoadDirect();
			IInstrumentAccessContainer^ iac = safe_cast<IInstrumentAccessContainer^>(o);
			m_instrument = safe_cast<IExactiveInstrumentAccess^>(iac->Get(1));
		}
		catch (Exception^ e)
		{
			Console::WriteLine(L" failed to load API");
			Console::WriteLine();
			Console::WriteLine(e->ToString());
			return false;
		}

		Console::WriteLine(L" API Loaded");
		return true;
	}

	// Return true if the instrument is accessible within 10 seconds
	bool ApiUsageTest::WaitForInstrument()
	{
		Console::Write(L"Waiting for instrument to become connected within 10 seconds...");
		try
		{
			IExactiveAcquisition^ ea = m_instrument->Control->Acquisition;
			if (!ea->WaitFor(TimeSpan::FromSeconds(10), SystemMode::Off, SystemMode::On, SystemMode::Standby))
			{
				Console::WriteLine(L" Sorry, system mode remained at {0}", ea->State->SystemMode);
				return false;
			}
			// I cheated, we wait a maximum of 20 secs.
			if (!ea->WaitFor(TimeSpan::FromSeconds(10), InstrumentState::Off, InstrumentState::StandBy, InstrumentState::ReadyToDownload))
			{
				Console::WriteLine(L" Sorry, instrument state remained at {0}", ea->State->SystemState);
				return false;
			}
		}
		catch (Exception^ e)
		{
			Console::WriteLine(L" failed to use the API");
			Console::WriteLine();
			Console::WriteLine(e->ToString());
			return false;
		}
		Console::WriteLine(L" Instrument connected");
		return true;
	}

	// Return the scans interface in a way that it is suitable to receive commands
	IScans^ ApiUsageTest::GetWorkingScansInterface()
	{
		Console::Write(L"Placing custom scans and waiting for it to accept commands...");

		IScans^ s = nullptr;
		try
		{
			s = m_instrument->Control->GetScans(false);

			// The interface is not usable until parameters are available, create a waiting mutex and wait on it.
			AutoResetEvent^ waiter = gcnew AutoResetEvent((s->PossibleParameters != nullptr) && (s->PossibleParameters->Length > 0));	// auto-dispose
			HandlerToResetEvent^ handler = gcnew HandlerToResetEvent(waiter);	// auto-dispose
			s->PossibleParametersChanged += gcnew System::EventHandler(handler, &HandlerToResetEvent::Handler);
			waiter->WaitOne(1000);	// 1 second should really be sufficient because we are already connected. Expect few ms to get this happen
			s->PossibleParametersChanged -= gcnew System::EventHandler(handler, &HandlerToResetEvent::Handler);
			if ((s->PossibleParameters == nullptr) || (s->PossibleParameters->Length == 0))
			{
				Console::WriteLine(L" Sorry, IScans interface node doesn't supply parameters for unknown reason");
				return nullptr;
			}
		}
		catch (System::Security::AccessControl::PrivilegeNotHeldException^ priv)
		{
			Console::WriteLine(L" cannot get IScans interface, missing privilege '{0}'", priv->PrivilegeName);
			return nullptr;
		}
		catch (Exception^ e)
		{
			Console::WriteLine(L" cannot get IScans interface");
			Console::WriteLine();
			Console::WriteLine(e->ToString());
			return nullptr;
		}

		// assume "Set" is part of the commands
		Console::WriteLine(L"Available");
		return s;
	}

	// Place two consecutive scans, one on positive, one in negative mode
	void ApiUsageTest::PlaceScans(IScans^ scans)
	{
		Console::Write(L"Placing custom scans with different polarity...");

		try
		{
			ICustomScan^ cs;
			for (int i = 0; i < 5; i++)
			{
				cs = scans->CreateCustomScan();
				cs->RunningNumber = 2001;
				// assume we have one possible values "0" and "1" for polarity
				cs->Values["Polarity"] = "0";
				if (!scans->SetCustomScan(cs))
				{
					Console::WriteLine(L" cannot place custom scan for unknown reason");
					return;
				}
				cs = scans->CreateCustomScan();
				cs->RunningNumber = 2002;
				// assume we have one possible values "0" and "1" for polarity
				cs->Values["Polarity"] = "1";
				if (!scans->SetCustomScan(cs))
				{
					Console::WriteLine(L" cannot place custom scan for unknown reason");
					return;
				}
			}
		}
		catch (Exception^ e)
		{
			Console::WriteLine(L" cannot place custom scans");
			Console::WriteLine();
			Console::WriteLine(e->ToString());
		}
		Console::WriteLine(L" OK");
	}

	// New way of linking the API requires knowledge about class/namespace and the implementing API.
	// Exactive's standard values can be found in the standard Xcalibur registry at
	// HKLM\Software[\Wow6432Node]\Finnigan\Xcalibur\Devices\Thermo Exactive
	// under the string values ApiFileName_Clr2_32_V1 and ApiClassName_Clr2_32_V1
	Object^ ApiUsageTest::LoadDirect()
	{
		String^ asmName = nullptr;
		String^ typeName = nullptr;

		String^ baseName = String::Format(L"SOFTWARE{0}\\Finnigan\\Xcalibur\\Devices\\Thermo Exactive", (IntPtr::Size > 4) ? L"" : L"\\Wow6432Node");
		RegistryKey^ key = Registry::LocalMachine->OpenSubKey(baseName);
		if (key != nullptr)
		{
			asmName = safe_cast<String^>(key->GetValue(L"ApiFileName_Clr2_32_V1", nullptr));
			typeName = safe_cast<String^>(key->GetValue(L"ApiClassName_Clr2_32_V1", nullptr));
			key->Close();
		}
		if (String::IsNullOrEmpty(asmName) || String::IsNullOrEmpty(typeName))
		{
			Console::Error->WriteLine("Exactive is not registered or service hasn't been started one time.");
			Environment::Exit(3);
		}

		Assembly^ assm = Assembly::LoadFrom(asmName);
		Object^ o = assm->CreateInstance(typeName);
		return o;
	}

	// Old way of linking to the API using COM. Disadvantages are strange error messages, late detection
	// of bitness or CLR errors and too much .NET-magic
	Object^ ApiUsageTest::LoadPerCom()
	{
		Type ^t = Type::GetTypeFromProgID("Thermo Exactive.API_Clr2_32_V1", true);
		Object ^o = Activator::CreateInstance(t);
		return o;
	}
}
