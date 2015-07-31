// DotNet_COM_Call.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <objbase.h>
#import "D:\11Study\04EWS-ExchangeWebService\10Com\ManagedCOM\ManagedCOM\MyInterop\bin\Debug\MyInterop.tlb" named_guids raw_interfaces_only

typedef struct _MAPINAMEID
{
	LPGUID lpguid;
	ULONG ulKind;
	union {
		LONG IID;
		LPWSTR lpwstrName;
	} Kind;

} MAPINAMEID, FAR * LPMAPINAMEID;

typedef struct _E15PropertyItem
{
	UINT nTag;
	UINT nUsID;
	enum PropertyType
	{
		Unkown,
		BOOL,
		String,
		Int16,
		Int32,
		Int64,
		ByteArray,
		Double,
		DateTime,
		MVString,
	} Type;
	MAPINAMEID namedID;
	int nLength;
	byte* pValue;

}E15PropertyItem, *PE15PropertyItem;

const void GetGuidString(GUID guid, char* buf)
{
	_snprintf_s(buf, 64, 64
		, "{%08X-%04X-%04x-%02X%02X-%02X%02X%02X%02X%02X%02X}"
		, guid.Data1
		, guid.Data2
		, guid.Data3
		, guid.Data4[0], guid.Data4[1]
		, guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5]
		, guid.Data4[6], guid.Data4[7]
		);
}

int totalData = 0;

E15PropertyItem::PropertyType GetPropertyType()
{
	switch (totalData++ % 7)
	{
	case 0:
		return E15PropertyItem::BOOL;
	case 1:
		return E15PropertyItem::ByteArray;
	case 2:
		return E15PropertyItem::DateTime;
	case 3:
		return E15PropertyItem::Double;
	case 4:
		return E15PropertyItem::Int16;
	case 5:
		return E15PropertyItem::Int32;
	case 6:
		return E15PropertyItem::Int64;
	case 7:
		return E15PropertyItem::PropertyType::String;
	default:
		return E15PropertyItem::Int32;
		break;
	}
}

int GetPropertyLength(E15PropertyItem::PropertyType propertyType)
{
	switch (propertyType)
	{
	case E15PropertyItem::BOOL:
		return 1;
	case E15PropertyItem::ByteArray:
		return rand()%64;
	case E15PropertyItem::DateTime:
		return 8;
	case E15PropertyItem::Double:
		return 8;
	case E15PropertyItem::Int16:
		return 2;
	case E15PropertyItem::Int32:
		return 4;
	case E15PropertyItem::Int64:
		return 8;
	case E15PropertyItem::PropertyType::String:
		return rand() % 64;
	default:
		return 0;
	}
}
//
//void SetValue(byte* value, int length, E15PropertyItem::PropertyType propertyType){
//	switch (propertyType)
//	{
//	case E15PropertyItem::BOOL:
//		if (totalData++ % 2 == 0)
//			value[0] = 0x00;
//		else
//			value[0] = 0x01;
//		break;
//	case E15PropertyItem::ByteArray:
//		for (int index = 0; index < length; index++)
//		{
//			value[index] = (byte)(totalData++);
//		}
//		break;
//	case E15PropertyItem::DateTime:
//		
//		return 8;
//	case E15PropertyItem::Double:
//		return 8;
//	case E15PropertyItem::Int16:
//		return 2;
//	case E15PropertyItem::Int32:
//		return 4;
//	case E15PropertyItem::Int64:
//		return 8;
//	case E15PropertyItem::PropertyType::String:
//		return rand() % 64;
//	default:
//		return 0;
//	}
//}
//
//E15PropertyItem GetFixProperty()
//{
//	E15PropertyItem item;
//	item.nTag = (UINT);
//	item.nUsID = 0x0038;
//	item.Type = E15PropertyItem::BOOL;
//	item.nLength = LEN;
//
//	GUID guid;
//	HRESULT hr = ::CoCreateGuid(&guid);
//	char buf[64] = { 0 };
//	GetGuidString(guid, buf);
//	printf(buf);
//
//	MAPINAMEID nameId;
//	nameId.Kind.IID = 0x0039;
//	nameId.ulKind = 0;
//	nameId.lpguid = &guid;
//	item.namedID = nameId;
//	byte valueBuffer[LEN];
//
//	for (int index = 0; index < LEN; index++)
//	{
//		valueBuffer[index] = index;
//	}
//	item.pValue = valueBuffer;
//	items[0] = item;
//}
//
//E15PropertyItem GetVarProperty()
//{
//	E15PropertyItem ite;
//	return ite;
//}

void TestSuperChild()
{
	CoInitialize(NULL);
	MyInterop::IChildrenInterfacePtr ptrChildren = NULL;
	HRESULT hRes = ptrChildren.CreateInstance(MyInterop::CLSID_TestCom);

	int result = 0;
	hRes = ptrChildren->Multi(2, 3, (long*)&result);
	hRes = ((MyInterop::ISuperInterfacePtr)ptrChildren)->Add(3, 4, (long*)&result);

	if (ptrChildren != NULL)
	{
		ptrChildren.Release();
		ptrChildren = NULL;
	}
	CoUninitialize();
}

void TestCom()
{
	CoInitialize(NULL);
	MyInterop::IMyDotNetInterfacePtr pDotNetCOMPtr = NULL;
	MyInterop::IChildInterfacePtr pchildInterface = NULL;
	const int LEN = 10;
	HRESULT hRes = pDotNetCOMPtr.CreateInstance(MyInterop::CLSID_MyDotNetClass);
	GUID guid;
	if (hRes == S_OK)
	{
		//pDotNetCOMPtr->ShowDialog();
		byte arrayByte[] = { 0x01, 0x02, 0x03 };
		int length = 3;
		LPWSTR name = L"haha,hehe";
		//pDotNetCOMPtr->InputBytesAndString(arrayByte,length);
		pDotNetCOMPtr->TestString(name);
		E15PropertyItem items[2];


		E15PropertyItem item1;
		item1.nTag = 0x0040;
		item1.nUsID = 0x0041;
		item1.Type = E15PropertyItem::DateTime;
		item1.nLength = LEN;

		hRes = ::CoCreateGuid(&guid);

		MAPINAMEID nameId1;
		nameId1.Kind.lpwstrName = L"123456haha";
		nameId1.ulKind = 1;
		nameId1.lpguid = &guid;
		item1.namedID = nameId1;
		byte valueBuffer1[LEN];

		for (int index = 0; index < LEN; index++)
		{
			valueBuffer1[index] = index + 20;
		}
		item1.pValue = valueBuffer1;
		items[1] = item1;

		pDotNetCOMPtr->GetProperties((long)items, 2);

		hRes = pDotNetCOMPtr->GetChildInterface(2, &pchildInterface);
		pchildInterface->ShowDialog();

	}

	if (pDotNetCOMPtr != NULL){
		pDotNetCOMPtr->Release();
		pDotNetCOMPtr = NULL;
	}

	if (pchildInterface != NULL)
	{
		pchildInterface->Release();
		pchildInterface = NULL;
	}

	CoUninitialize();
}
#include<vector>
using namespace std;

class MyTest;
typedef int(MyTest::*delFunc)(int, int);

class MyTest{
public:
	int add(int a, int b);
	int mul(int a, int b);
public:
	vector<delFunc> m_allDel;
	int doIt(int a, int b);
};

//MyTest::MyTest(){
//	m_allDel.push_back(&MyTest::add);
//}
//
//MyTest::~MyTest()
//{
//
//}

int MyTest::add(int a, int b){
	return a + b;
}

int MyTest::mul(int a, int b){
	return a*b;
}


int MyTest::doIt(int a, int b)
{
	delFunc myfunc = m_allDel[0];
	(this->*myfunc)(a, b);
	return 0;
}

#include<sstream>

int _tmain(int argc, _TCHAR* argv[])
{
	WCHAR szTemp[MAX_PATH * 2] = { 0 };
	GetTempPathW(MAX_PATH * 2, szTemp);
	WCHAR tempFolder[MAX_PATH * 2] = { 0 };

	wstring wstTemp = szTemp;
	wchar_t spec = L'\\';
	bool cantainSpec = (wstTemp.at(wstTemp.size() - 1) == spec);
	SYSTEMTIME stTime;
	GetLocalTime(&stTime);
	if (!cantainSpec)
		swprintf_s(tempFolder, MAX_PATH * 2, L"%s\\Embed%d%d%d%d%d\\", szTemp, stTime.wYear, stTime.wMonth, stTime.wDay, stTime.wHour, stTime.wMinute, stTime.wSecond);
	swprintf_s(tempFolder, MAX_PATH * 2, L"%sEmbed%d%d%d%d%d\\", szTemp, stTime.wYear, stTime.wMonth, stTime.wDay, stTime.wHour, stTime.wMinute, stTime.wSecond);

	wstTemp = szTemp;
	CreateDirectoryW(wstTemp.c_str(), NULL);
	WCHAR fileName[MAX_PATH * 2] = { 0 };
	swprintf_s(fileName, MAX_PATH * 2, L"%s%d", tempFolder, 1);

	FILE* embedFile;
	_wfopen_s(&embedFile, fileName, L"w+b");
	int nFileSize = fwrite(fileName, 2, MAX_PATH, embedFile);
	fclose(embedFile);

	/*
	WCHAR szTemp[MAX_PATH * 2] = { 0 };
	GetTempPathW(MAX_PATH * 2, szTemp);
	wstring wstTemp = szTemp;
	wchar_t spec = L'\\';
	bool cantainSpec = (wstTemp.at(wstTemp.size() - 1) == spec);
	SYSTEMTIME stTime;
	GetLocalTime(&stTime);
	wstringstream dirStream;
	dirStream << wstTemp;
	if (!cantainSpec)
		dirStream << L"\\";
	dirStream << L"Embed" << stTime.wYear << stTime.wMonth << stTime.wDay << stTime.wHour << stTime.wMinute << stTime.wSecond << L"\\";
	wstTemp = dirStream.str().c_str();
	CreateDirectoryW(wstTemp.c_str(), NULL);

	wstringstream wstrStream;
	wstrStream << wstTemp;
	wstrStream << 1;
	wstring fileName = wstrStream.str();

	FILE* embedFile;
	_wfopen_s(&embedFile, fileName.c_str(), L"w+b");
	int nFileSize = fwrite(fileName.c_str(), 1, fileName.size(), embedFile);
	fclose(embedFile);*/
	exit(0);
	delFunc myFunc = &MyTest::add;

	MyTest a;

	delFunc arrayFunc[1];
	arrayFunc[0] = &MyTest::add;
	vector < delFunc > vec;
	vec.push_back(&MyTest::add);
	int c = (a.*myFunc)(1, 2);


	TestSuperChild();
	return 0;
}







