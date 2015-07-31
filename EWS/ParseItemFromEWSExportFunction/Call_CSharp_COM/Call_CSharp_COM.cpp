// Call_CSharp_COM.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#import "E:\amt\Personal\TechnicalDocs\CodeProject\Code\MyInterop\obj\Debug\com.MyInterop.tlb" named_guids raw_interfaces_only

int _tmain(int argc, _TCHAR* argv[])
{
	CoInitialize(NULL);
	MyInterop::IMyDotNetInterfacePtr pDotNetCOMPtr;

	HRESULT hRes = pDotNetCOMPtr.CreateInstance(MyInterop::CLSID_MyDotNetClass);
	if (hRes == S_OK)
	{
		pDotNetCOMPtr->ShowDialog ();
	}
	
	CoUninitialize ();
	return 0;
}

