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
#include "stdafx.h"
#include "ApiUsageTest.h"

using namespace System;
using namespace Thermo::Interfaces::ExactiveAccess_V1::Control::InstrumentValues;
using namespace ManagedCppLoad;

int main(array<System::String ^> ^args)
{
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
