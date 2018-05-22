#include "StdAfx.h"

using namespace System;

namespace Thermo
{
	namespace Prometheus
	{
		// This class implements the link to the API's caller validation. Namespace, class name, property name and signature is fixed, don't change.
		// Single allowed difference: Return type can either be a plain string or an array of strings.
		ref class ClientValidation sealed
		{
		public:
#ifdef MultiLicenseSupport
			static property array<String^>^ License
			{
				array<String^>^ get()
				{
					return gcnew array<String^> { L"T2-VDAtMi1BbGwtOTNFREIwQjMwNURGODAwLVRoZXJtb8KgRmlzaGVywqBTY2llbnRpZmljwqAoQnJlbWVuKcKgR21iSMKgKEZHQyktRXhhY3RpdmUgTlJQQldBa3lLWWxNdG5CZi8xMFFMYjhKd1kxUVhXdXFLVi81djlrS2Z3ZE1DV2p2amgzT0RBPT0=-Api-8D3DF6FE1153800-Exactive:2-ManagedCppLoad,\u00A0Version=1.2.3.4,\u00A0Culture=neutral,\u00A0PublicKeyToken=e16708b1b21141e3 S/r+Ckzs9N3Oqf7xuSE0UN3YMZORWElVzoyKlbRXXdFzQ25WjmJajg==" };
				}
			}
#else
			static property String^ License
			{
				String^ get()
				{
					return L"T2-VDAtMi1BbGwtOTNFREIwQjMwNURGODAwLVRoZXJtb8KgRmlzaGVywqBTY2llbnRpZmljwqAoQnJlbWVuKcKgR21iSMKgKEZHQyktRXhhY3RpdmUgTlJQQldBa3lLWWxNdG5CZi8xMFFMYjhKd1kxUVhXdXFLVi81djlrS2Z3ZE1DV2p2amgzT0RBPT0=-Api-8D3DF6FE1153800-Exactive:2-ManagedCppLoad,\u00A0Version=1.2.3.4,\u00A0Culture=neutral,\u00A0PublicKeyToken=e16708b1b21141e3 S/r+Ckzs9N3Oqf7xuSE0UN3YMZORWElVzoyKlbRXXdFzQ25WjmJajg==";
				}
			}
#endif
		};
	}
}
