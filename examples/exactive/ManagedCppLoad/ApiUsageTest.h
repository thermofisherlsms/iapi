#pragma once
using namespace System;

using namespace Thermo::Interfaces::ExactiveAccess_V1;
using namespace Thermo::Interfaces::InstrumentAccess_V1::Control::Scans;

namespace ManagedCppLoad
{
	public ref class ApiUsageTest : IDisposable
	{
	private:
		IExactiveInstrumentAccess^ m_instrument;

	private:
		Object^ LoadPerCom();
		Object^ LoadDirect();

	protected:
		!ApiUsageTest();

	public:
		ApiUsageTest();
		~ApiUsageTest();
		bool Load();
		bool WaitForInstrument();
		IScans^ GetWorkingScansInterface();
		void PlaceScans(IScans^ scans);
	};
}

