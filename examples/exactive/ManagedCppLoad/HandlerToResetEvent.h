#pragma once
using namespace System;
using namespace System::Threading;

using namespace Thermo::Interfaces::InstrumentAccess_V1::Control::Scans;

namespace ManagedCppLoad
{
	// Event handler for setting a mutex on receiving an event of an IScans->PossibleParametersChanged.
	// We set the event if and only if at least one command is present.
	public ref class HandlerToResetEvent
	{
	private:
		AutoResetEvent^ semaphore;

	protected:
		!HandlerToResetEvent();

	public:
		HandlerToResetEvent(AutoResetEvent^ semaphore);
		~HandlerToResetEvent();
		void Handler(Object^ sender, EventArgs^ e);
	};
}