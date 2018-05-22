#include "StdAfx.h"
#include "HandlerToResetEvent.h"

namespace ManagedCppLoad
{
	HandlerToResetEvent::!HandlerToResetEvent()
	{
		// not really needed
		semaphore = nullptr;
	}

	HandlerToResetEvent::HandlerToResetEvent(AutoResetEvent^ semaphore)
	{
		this->semaphore = semaphore;
	}

	HandlerToResetEvent::~HandlerToResetEvent()
	{
		this->!HandlerToResetEvent();
	}

	// Set the event semaphore if parameters become available
	void HandlerToResetEvent::Handler(Object^ sender, EventArgs^ e)
	{
		IScans^ value = safe_cast<IScans^>(sender);
		if ((value != nullptr) && (value->PossibleParameters != nullptr) && (value->PossibleParameters->Length > 0))
		{
			semaphore->Set();
		}
	}
}