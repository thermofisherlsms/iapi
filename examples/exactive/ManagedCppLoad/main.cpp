#include "stdafx.h"
#include "ApiUsageTest.h"

using namespace System;
using namespace Thermo::Interfaces::ExactiveAccess_V1::Control::InstrumentValues;
using namespace ManagedCppLoad;

int main(array<System::String ^> ^args)
{
	//ClientValidation^ noOptimize = gcnew  ClientValidation();
	ApiUsageTest^ test = gcnew ApiUsageTest();

	if (!test->Load())
	{
		return 1;
	}

	if (!test->WaitForInstrument())
	{
		return 1;
	}

	// This requires API privileges
	IScans^ scans = test->GetWorkingScansInterface();
	if (scans == nullptr)
	{
		return 1;
	}

	// And do something
	test->PlaceScans(scans);

	return 0;
}
