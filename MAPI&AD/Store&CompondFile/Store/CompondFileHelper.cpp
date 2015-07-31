#include "stdafx.h"
#include "CompondFileHelper.h"

#pragma comment( lib, "ole32.lib" )

vector<wstring> CCompondFileHelper::s_indent;
CCompondFileHelper::CCompondFileHelper()
{
	if (s_indent.size() == 0){
		s_indent.push_back(L"");
		s_indent.push_back(L"-");
		s_indent.push_back(L"--");
		s_indent.push_back(L"---");
		s_indent.push_back(L"----");
		s_indent.push_back(L"-----");
		s_indent.push_back(L"------");
		s_indent.push_back(L"-------");
		s_indent.push_back(L"--------");
		s_indent.push_back(L"---------");
		s_indent.push_back(L"----------");
	}
}

CCompondFileHelper::~CCompondFileHelper()
{
}
const FMTID fmtid = { /* d170df2e-1117-11d2-aa01-00805ffe11b8 */
	0xd170df2e,
	0x1117,
	0x11d2,
	{ 0xaa, 0x01, 0x00, 0x80, 0x5f, 0xfe, 0x11, 0xb8 }
};

void CCompondFileHelper::Test(){
	HRESULT hr = S_OK;
	IPropertySetStorage *pPropSetStg = NULL;
	IPropertyStorage *pPropStg = NULL;
	WCHAR *pwszError = L"";
	PROPSPEC propspec;
	PROPVARIANT propvarWrite;
	PROPVARIANT propvarRead;

	try
	{

		// Create a file and a property set within it.
		hr = StgCreateStorageEx(L"WriteRead.stg",
			STGM_CREATE | STGM_SHARE_EXCLUSIVE | STGM_READWRITE,
			STGFMT_STORAGE,
			// STGFMT_STORAGE => Structured Storage 
			// property sets
			// STGFMT_FILE    => NTFS file system 
			// property sets
			0, NULL, NULL,
			IID_IPropertySetStorage,
			reinterpret_cast<void**>(&pPropSetStg));
		if (FAILED(hr)) throw L"Failed StgCreateStorageEx";

		hr = pPropSetStg->Create(fmtid, NULL, PROPSETFLAG_DEFAULT,
			STGM_CREATE | STGM_READWRITE | STGM_SHARE_EXCLUSIVE,
			&pPropStg);
		if (FAILED(hr)) throw L"Failed IPropertySetStorage::Create";

		// Write a Unicode string property to the property set

		propspec.ulKind = PRSPEC_LPWSTR;
		propspec.lpwstr = L"Property Name";

		propvarWrite.vt = VT_LPWSTR;
		propvarWrite.pwszVal = L"Property Value";

		hr = pPropStg->WriteMultiple(1, &propspec, &propvarWrite,
			PID_FIRST_USABLE);
		if (FAILED(hr))
			throw L"Failed IPropertyStorage::WriteMultiple";

		// Not required, but give the property set a friendly 
		// name.

		PROPID propidDictionary = PID_DICTIONARY;
		WCHAR *pwszFriendlyName =
			L"Write/Read Properties Sample Property Set";
		hr = pPropStg->WritePropertyNames(1, &propidDictionary,
			&pwszFriendlyName);
		if (FAILED(hr))
			throw L"Failed IPropertyStorage::WritePropertyNames";

		// Commit changes to the property set.

		hr = pPropStg->Commit(STGC_DEFAULT);
		if (FAILED(hr))
			throw L"Failed IPropertyStorage::Commit";

		// Close and reopen everything.
		// By using the STGFMT_ANY flag in the StgOpenStorageEx call,
		// it does not matter if this is a Structured Storage 
		// property set or an NTFS file system property set 
		// (for more information see the StgCreateStorageEx 
		// call above).

		pPropStg->Release(); pPropStg = NULL;
		pPropSetStg->Release(); pPropSetStg = NULL;

		hr = StgOpenStorageEx(L"WriteRead.stg",
			STGM_READ | STGM_SHARE_DENY_WRITE,
			STGFMT_ANY,
			0, NULL, NULL,
			IID_IPropertySetStorage,
			reinterpret_cast<void**>(&pPropSetStg));
		if (FAILED(hr))
			throw L"Failed StgOpenStorageEx";

		hr = pPropSetStg->Open(fmtid, STGM_READ | STGM_SHARE_EXCLUSIVE,
			&pPropStg);
		if (FAILED(hr))
			throw L"Failed IPropertySetStorage::Open";

		// Read the property back and validate it

		hr = pPropStg->ReadMultiple(1, &propspec, &propvarRead);
		if (FAILED(hr))
			throw L"Failed IPropertyStorage::ReadMultiple";

		if (S_FALSE == hr)
			throw L"Property didn't exist after reopening the property set";
		else if (propvarWrite.vt != propvarRead.vt)
			throw L"Property types didn't match after reopening the property set";
		else if (0 != wcscmp(propvarWrite.pwszVal, propvarRead.pwszVal))
			throw L"Property values didn't match after reopening the property set";
		else
			wprintf(L"Success\n");

	}
	catch (const WCHAR *pwszError)
	{
		wprintf(L"Error:  %s (hr=%08x)\n", pwszError, hr);
	}

	PropVariantClear(&propvarRead);
	if (pPropStg) pPropStg->Release();
	if (pPropSetStg) pPropSetStg->Release();
}


//+-------------------------------------------------------------------
//
//  ConvertVarTypeToString
//  
//  Generate a string for a given PROPVARIANT variable type (VT). 
//  For the given vt, write the string to pwszType, which is a buffer
//  of size cchType characters.
//
//+-------------------------------------------------------------------

void CCompondFileHelper::ConvertVarTypeToString(VARTYPE vt, WCHAR *pwszType, ULONG cchType)
{
	const WCHAR *pwszModifier;

	// Ensure that the output string is terminated
	// (wcsncpy does not guarantee termination)

	pwszType[cchType - 1] = L'\0';
	--cchType;

	// Create a string using the basic type.

	switch (vt & VT_TYPEMASK)
	{
	case VT_EMPTY:
		wcsncpy_s(pwszType, cchType, L"VT_EMPTY", cchType);
		break;
	case VT_NULL:
		wcsncpy_s(pwszType, cchType, L"VT_NULL", cchType);
		break;
	case VT_I2:
		wcsncpy_s(pwszType, cchType, L"VT_I2", cchType);
		break;
	case VT_I4:
		wcsncpy_s(pwszType, cchType, L"VT_I4", cchType);
		break;
	case VT_I8:
		wcsncpy_s(pwszType, cchType, L"VT_I8", cchType);
		break;
	case VT_UI2:
		wcsncpy_s(pwszType, cchType, L"VT_UI2", cchType);
		break;
	case VT_UI4:
		wcsncpy_s(pwszType, cchType, L"VT_UI4", cchType);
		break;
	case VT_UI8:
		wcsncpy_s(pwszType, cchType, L"VT_UI8", cchType);
		break;
	case VT_R4:
		wcsncpy_s(pwszType, cchType, L"VT_R4", cchType);
		break;
	case VT_R8:
		wcsncpy_s(pwszType, cchType, L"VT_R8", cchType);
		break;
	case VT_CY:
		wcsncpy_s(pwszType, cchType, L"VT_CY", cchType);
		break;
	case VT_DATE:
		wcsncpy_s(pwszType, cchType, L"VT_DATE", cchType);
		break;
	case VT_BSTR:
		wcsncpy_s(pwszType, cchType, L"VT_BSTR", cchType);
		break;
	case VT_ERROR:
		wcsncpy_s(pwszType, cchType, L"VT_ERROR", cchType);
		break;
	case VT_BOOL:
		wcsncpy_s(pwszType, cchType, L"VT_BOOL", cchType);
		break;
	case VT_VARIANT:
		wcsncpy_s(pwszType, cchType, L"VT_VARIANT", cchType);
		break;
	case VT_DECIMAL:
		wcsncpy_s(pwszType, cchType, L"VT_DECIMAL", cchType);
		break;
	case VT_I1:
		wcsncpy_s(pwszType, cchType, L"VT_I1", cchType);
		break;
	case VT_UI1:
		wcsncpy_s(pwszType, cchType, L"VT_UI1", cchType);
		break;
	case VT_INT:
		wcsncpy_s(pwszType, cchType, L"VT_INT", cchType);
		break;
	case VT_UINT:
		wcsncpy_s(pwszType, cchType, L"VT_UINT", cchType);
		break;
	case VT_VOID:
		wcsncpy_s(pwszType, cchType, L"VT_VOID", cchType);
		break;
	case VT_SAFEARRAY:
		wcsncpy_s(pwszType, cchType, L"VT_SAFEARRAY", cchType);
		break;
	case VT_USERDEFINED:
		wcsncpy_s(pwszType, cchType, L"VT_USERDEFINED", cchType);
		break;
	case VT_LPSTR:
		wcsncpy_s(pwszType, cchType, L"VT_LPSTR", cchType);
		break;
	case VT_LPWSTR:
		wcsncpy_s(pwszType, cchType, L"VT_LPWSTR", cchType);
		break;
	case VT_RECORD:
		wcsncpy_s(pwszType, cchType, L"VT_RECORD", cchType);
		break;
	case VT_FILETIME:
		wcsncpy_s(pwszType, cchType, L"VT_FILETIME", cchType);
		break;
	case VT_BLOB:
		wcsncpy_s(pwszType, cchType, L"VT_BLOB", cchType);
		break;
	case VT_STREAM:
		wcsncpy_s(pwszType, cchType, L"VT_STREAM", cchType);
		break;
	case VT_STORAGE:
		wcsncpy_s(pwszType, cchType, L"VT_STORAGE", cchType);
		break;
	case VT_STREAMED_OBJECT:
		wcsncpy_s(pwszType, cchType, L"VT_STREAMED_OBJECT", cchType);
		break;
	case VT_STORED_OBJECT:
		wcsncpy_s(pwszType, cchType, L"VT_BLOB_OBJECT", cchType);
		break;
	case VT_CF:
		wcsncpy_s(pwszType, cchType, L"VT_CF", cchType);
		break;
	case VT_CLSID:
		wcsncpy_s(pwszType, cchType, L"VT_CLSID", cchType);
		break;
	default:
		_snwprintf_s(pwszType, cchType, cchType, L"Unknown (%d)",
			vt & VT_TYPEMASK);
		break;
	}

	// Adjust cchType for the added characters.

	cchType -= wcslen(pwszType);

	// Add the type modifiers, if present.

	if (vt & VT_VECTOR)
	{
		pwszModifier = L" | VT_VECTOR";
		wcsncat_s(pwszType, cchType, pwszModifier, cchType);
		cchType -= wcslen(pwszModifier);
	}

	if (vt & VT_ARRAY)
	{
		pwszModifier = L" | VT_ARRAY";
		wcsncat_s(pwszType, cchType, pwszModifier, cchType);
		cchType -= wcslen(pwszModifier);
	}

	if (vt & VT_RESERVED)
	{
		pwszModifier = L" | VT_RESERVED";
		wcsncat_s(pwszType, cchType, pwszModifier, cchType);
		cchType -= wcslen(pwszModifier);
	}

}


//+-------------------------------------------------------------------
//
//  ConvertValueToString
//  
//  Generate a string for the value in a given PROPVARIANT structure.
//  The most common types are supported; that is, those that can be
//  displayed with printf.  For other types, only an ellipses (...) 
//  is displayed.
//
//  The property to create a string from is in propvar, the resulting
//  string is placed into pwszValue, which is a buffer with space for
//  cchValue characters (including the string terminator).
//
//+-------------------------------------------------------------------

void CCompondFileHelper::ConvertValueToString(const PROPVARIANT &propvar, WCHAR *pwszValue, ULONG cchValue)
{
	// Ensure that the output string is terminated

	pwszValue[cchValue - 1] = L'\0';
	--cchValue;

	// Based on the type, put the value into pwszValue as a string.

	switch (propvar.vt)
	{
	case VT_EMPTY:
		wcsncpy_s(pwszValue, cchValue, L"", cchValue);
		break;
	case VT_NULL:
		wcsncpy_s(pwszValue, cchValue, L"", cchValue);
		break;
	case VT_I2:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%i", propvar.iVal);
		break;
	case VT_I4:
	case VT_INT:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%li", propvar.lVal);
		break;
	case VT_I8:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%I64i", propvar.hVal);
		break;
	case VT_UI2:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%u", propvar.uiVal);
		break;
	case VT_UI4:
	case VT_UINT:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%lu", propvar.ulVal);
		break;
	case VT_UI8:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%I64u", propvar.uhVal);
		break;
	case VT_R4:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%f", propvar.fltVal);
		break;
	case VT_R8:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%lf", propvar.dblVal);
		break;
	case VT_BSTR:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"\"%s\"",
			propvar.bstrVal);
		break;
	case VT_ERROR:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"0x%08X", propvar.scode);
		break;
	case VT_BOOL:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%s",
			VARIANT_TRUE == propvar.boolVal ? L"True" : L"False");
		break;
	case VT_I1:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%i", propvar.cVal);
		break;
	case VT_UI1:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%u", propvar.bVal);
		break;
	case VT_VOID:
		wcsncpy_s(pwszValue, cchValue, L"", cchValue);
		break;
	case VT_LPSTR:
		if (0 > _snwprintf_s(pwszValue, cchValue, cchValue,
			L"\"%hs\"", propvar.pszVal))
			// String is too large for pwszValue
			wcsncpy_s(pwszValue, cchValue, L"...", cchValue);
		break;
	case VT_LPWSTR:
		if (0 > _snwprintf_s(pwszValue, cchValue, cchValue,
			L"\"%s\"", propvar.pwszVal))
			// String is too large for pwszValue
			wcsncpy_s(pwszValue, cchValue, L"...", cchValue);
		break;
	case VT_FILETIME:
		_snwprintf_s(pwszValue, cchValue, cchValue, L"%08x:%08x",
			propvar.filetime.dwHighDateTime,
			propvar.filetime.dwLowDateTime);
		break;
	case VT_CLSID:
		pwszValue[0] = L'\0';
		StringFromGUID2(*propvar.puuid, pwszValue, cchValue);
		break;
	default:
		wcsncpy_s(pwszValue, cchValue, L"...", cchValue);
		break;
	}

}


//+-------------------------------------------------------------------
//
//  DisplayProperty
//
//  Dump the ID, name, type, and value of a property.
//
//+-------------------------------------------------------------------

void CCompondFileHelper::DisplayProperty(const PROPVARIANT &propvar, const STATPROPSTG &statpropstg)
{
	WCHAR wsz[MAX_PATH + 1];

	ConvertVarTypeToString(statpropstg.vt, wsz,
		sizeof(wsz) / sizeof(wsz[0]));

	wprintf(L"   -------------------------------------------------\n"
		L"   PropID = %-5d VarType = %-23s",
		statpropstg.propid, wsz);

	if (NULL != statpropstg.lpwstrName)
		wprintf(L" Name = %s", statpropstg.lpwstrName);

	ConvertValueToString(propvar, wsz, sizeof(wsz) / sizeof(wsz[0]));

	wprintf(L"\n   Value = %s\n", wsz);

}


//+-------------------------------------------------------------------
//
//  DisplayPropertySet
//
//  Dump all the properties into a given property set.
//
//+-------------------------------------------------------------------

void CCompondFileHelper::DisplayPropertySet(FMTID fmtid, const WCHAR *pwszStorageName, IPropertyStorage *pPropStg)
{
	IEnumSTATPROPSTG *penum = NULL;
	HRESULT hr = S_OK;
	STATPROPSTG statpropstg;
	PROPVARIANT propvar;
	PROPSPEC propspec;
	PROPID propid;
	WCHAR *pwszFriendlyName = NULL;

	// This string will hold a string-formatted FMTID. It must
	// be 38 characters, plus the terminator character.
	// For best practice, create a moderately longer string.
	WCHAR wszFMTID[64] = { L"" };

	PropVariantInit(&propvar);
	memset(&statpropstg, 0, sizeof(statpropstg));

	try
	{
		// Display the ID of the property set.

		StringFromGUID2(fmtid,
			wszFMTID,
			sizeof(wszFMTID) / sizeof(wszFMTID[0]));
		wprintf(L"\n Property Set %s\n", wszFMTID);

		// If this is a common property set, display.

		if (FMTID_SummaryInformation == fmtid)
			wprintf(L"   (SummaryInformation property set)\n");
		else if (FMTID_DocSummaryInformation == fmtid)
			wprintf(L"   (DocumentSummaryInformation property set)\n");
		else if (FMTID_UserDefinedProperties == fmtid)
			wprintf(L"   (UserDefined property set)\n");

		// Also display the name of the storage that contains
		// this property set.

		wprintf(L"   in \"%s\":\n", pwszStorageName);

		// If this property set has a friendly name, display it now.
		// (Property names are stored in the special dictionary
		// property - the name of the property set is indicated by
		// naming the dictionary property itself.)

		propid = PID_DICTIONARY;
		pwszFriendlyName = NULL;

		hr = pPropStg->ReadPropertyNames(1, &propid,
			&pwszFriendlyName);
		if (S_OK == hr)
		{
			wprintf(L"   (Friendly name is \"%s\")\n\n",
				pwszFriendlyName);
			CoTaskMemFree(pwszFriendlyName);
			pwszFriendlyName = NULL;
		}
		else
			wprintf(L"\n");

		// Get a property enumerator.

		hr = pPropStg->Enum(&penum);
		if (FAILED(hr))
			throw L"Failed IPropertyStorage::Enum";

		// Get the first property in the enumeration.

		hr = penum->Next(1, &statpropstg, NULL);

		// Loop through and display each property.  The 'Next'
		// call above, and at the bottom of the while loop,
		// will return S_OK if it returns another property,
		// S_FALSE if there are no more properties,
		// and anything else is an error.

		while (S_OK == hr)
		{

			// Read the property out of the property set

			PropVariantInit(&propvar);
			propspec.ulKind = PRSPEC_PROPID;
			propspec.propid = statpropstg.propid;

			hr = pPropStg->ReadMultiple(1, &propspec, &propvar);
			if (FAILED(hr))
				throw L"Failed IPropertyStorage::ReadMultiple";

			// Display the property value, type, and so on.

			DisplayProperty(propvar, statpropstg);

			// Free buffers allocated during the read, and
			// by the enumerator.

			PropVariantClear(&propvar);
			CoTaskMemFree(statpropstg.lpwstrName);
			statpropstg.lpwstrName = NULL;

			// Move to the next property in the enumeration

			hr = penum->Next(1, &statpropstg, NULL);
		}
		if (FAILED(hr)) throw L"Failed IEnumSTATPROPSTG::Next";
	}
	catch (LPCWSTR pwszErrorMessage)
	{
		wprintf(L"Error in DumpPropertySet: %s (hr = %08x)\n",
			pwszErrorMessage, hr);
	}

	if (NULL != penum)
		penum->Release();

	if (NULL != statpropstg.lpwstrName)
		CoTaskMemFree(statpropstg.lpwstrName);

	PropVariantClear(&propvar);
}


//+-------------------------------------------------------------------
//
//  DisplayPropertySetsInStorage
//
//  Dump the property sets in the top level of a given storage.
//
//+-------------------------------------------------------------------

void CCompondFileHelper::DisplayPropertySetsInStorage(const WCHAR *pwszStorageName, IPropertySetStorage *pPropSetStg)
{
	IEnumSTATPROPSETSTG *penum = NULL;
	HRESULT hr = S_OK;
	IPropertyStorage *pPropStg = NULL;
	STATPROPSETSTG statpropsetstg;

	try
	{
		// Get a property-set enumerator, which only enumerates
		// the property sets at this level of the storage, not 
		// its child objects.

		hr = pPropSetStg->Enum(&penum);
		if (FAILED(hr))
			throw L"failed IPropertySetStorage::Enum";

		// Get the first property set in the enumeration.
		// (The field used to open the property set is
		// statpropsetstg.fmtid.

		memset(&statpropsetstg, 0, sizeof(statpropsetstg));
		hr = penum->Next(1, &statpropsetstg, NULL);

		// Loop through all the property sets.

		while (S_OK == hr)
		{
			// Open the property set.

			hr = pPropSetStg->Open(statpropsetstg.fmtid,
				STGM_READ | STGM_SHARE_EXCLUSIVE,
				&pPropStg);
			if (FAILED(hr))
				throw L"failed IPropertySetStorage::Open";

			// Display the properties in the property set.

			DisplayPropertySet(statpropsetstg.fmtid,
				pwszStorageName,
				pPropStg);

			pPropStg->Release();
			pPropStg = NULL;

			// Get the FMTID of the next property set in the
			// enumeration.

			hr = penum->Next(1, &statpropsetstg, NULL);

		}
		if (FAILED(hr)) throw L"Failed IEnumSTATPROPSETSTG::Next";

		// Special-case handling for the UserDefined property set:
		// This property set actually lives inside the well-known
		// DocumentSummaryInformation property set. It is the only
		// property set which is allowed to live inside another
		// (and exists for legacy compatibility).  It does not get
		// included in a normal enumeration, so verify that it is
		// done explicitly. Look for it when the end of the
		// enumerator is reached.

		hr = pPropSetStg->Open(FMTID_UserDefinedProperties,
			STGM_READ | STGM_SHARE_EXCLUSIVE,
			&pPropStg);
		if (SUCCEEDED(hr))
		{
			DisplayPropertySet(FMTID_UserDefinedProperties,
				pwszStorageName,
				pPropStg);
			pPropStg->Release();
			pPropStg = NULL;
		}

	}
	catch (LPCWSTR pwszErrorMessage)
	{
		wprintf(L"Error in DumpPropertySetsInStorage: %s(hr = % 08x)\n",
			pwszErrorMessage, hr);
	}

	if (NULL != pPropStg)
		pPropStg->Release();
	if (NULL != penum)
		penum->Release();
}


//+-------------------------------------------------------------------
//
//  DisplayStorageTree
//
//  Dump all the property sets in the given storage and recursively in
//  all its child objects.
//
//+-------------------------------------------------------------------

void CCompondFileHelper::DisplayStorageTree(const WCHAR *pwszStorageName, IStorage *pStg)
{
	IPropertySetStorage *pPropSetStg = NULL;
	IStorage *pStgChild = NULL;
	WCHAR *pwszChildStorageName = NULL;
	IEnumSTATSTG *penum = NULL;
	HRESULT hr = S_OK;
	STATSTG statstg;

	memset(&statstg, 0, sizeof(statstg));

	try
	{

		// Dump the property sets at this storage level

		hr = pStg->QueryInterface(IID_IPropertySetStorage,
			reinterpret_cast<void**>(&pPropSetStg));
		if (FAILED(hr))
			throw L"Failed IStorage::QueryInterface(IID_IPropertySetStorage)";

		DisplayPropertySetsInStorage(pwszStorageName, pPropSetStg);

		// Get an enumerator for this storage.

		hr = pStg->EnumElements(NULL, NULL, NULL, &penum);
		if (FAILED(hr)) throw L"failed IStorage::Enum";

		// Get the name of the first element (stream/storage)
		// in the enumeration.  As usual, 'Next' will return
		// S_OK if it returns an element of the enumerator,
		// S_FALSE if there are no more elements, and an
		// error otherwise.

		hr = penum->Next(1, &statstg, 0);

		// Loop through all the child objects of this storage.

		while (S_OK == hr)
		{
			// Verify that this is a storage that is not a property
			// set, because the property sets are displayed above).
			// If the first character of its name is the '\005'
			// reserved character, it is a stream /storage property
			// set.

			if (STGTY_STORAGE == statstg.type && L'\005' != statstg.pwcsName[0])
			{
				// Indicates normal storage, not a propset.
				// Open the storage.

				ULONG cchChildStorageName;

				hr = pStg->OpenStorage(statstg.pwcsName,
					NULL,
					STGM_READ | STGM_SHARE_EXCLUSIVE,
					NULL, 0,
					&pStgChild);
				if (FAILED(hr))
					throw L"failed IStorage::OpenStorage";

				// Compose a name of the form 
				// "Storage\ChildStorage\..." for display purposes.
				// First, allocate it.

				cchChildStorageName = wcslen(pwszStorageName)
					+ wcslen(statstg.pwcsName)
					+ 2  // For two "\" chars
					+ 1; // For string terminator

				pwszChildStorageName =
					new WCHAR[cchChildStorageName];
				if (NULL == pwszChildStorageName)
				{
					hr = HRESULT_FROM_WIN32(ERROR_NOT_ENOUGH_MEMORY);
					throw L"couldn't allocate memory";
				}

				// Terminate the name.

				pwszChildStorageName[cchChildStorageName - 1] = L'\0';
				//--cchChildStorageName;

				// Build the name.

				wcsncpy_s(pwszChildStorageName, cchChildStorageName,
					pwszStorageName, cchChildStorageName);
				//cchChildStorageName -= wcslen(pwszStorageName);

				wcsncat_s(pwszChildStorageName, cchChildStorageName,
					L"\\", cchChildStorageName);
				//cchChildStorageName -= 2;

				wcsncat_s(pwszChildStorageName, cchChildStorageName,
					statstg.pwcsName, cchChildStorageName);

				// Dump all property sets under this child storage.

				DisplayStorageTree(pwszChildStorageName, pStgChild);

				pStgChild->Release();
				pStgChild = NULL;

				delete[] pwszChildStorageName;
				pwszChildStorageName = NULL;
			}

			// Move to the next element in the enumeration of 
			// this storage.

			CoTaskMemFree(statstg.pwcsName);
			statstg.pwcsName = NULL;

			hr = penum->Next(1, &statstg, 0);
		}
		if (FAILED(hr)) throw L"failed IEnumSTATSTG::Next";
	}
	catch (LPCWSTR pwszErrorMessage)
	{
		wprintf(L"Error in DumpStorageTree: %s (hr = %08x)\n",
			pwszErrorMessage, hr);
	}

	// Cleanup before returning.

	if (NULL != statstg.pwcsName)
		CoTaskMemFree(statstg.pwcsName);
	if (NULL != pStgChild)
		pStgChild->Release();
	if (NULL != pStg)
		pStg->Release();
	if (NULL != penum)
		penum->Release();
	if (NULL != pwszChildStorageName)
		delete[] pwszChildStorageName;

}


void CCompondFileHelper::Display(const WCHAR *pwszStorageName){
	IStorage *pStg = NULL;
	HRESULT hr = StgOpenStorageEx(pwszStorageName,
		STGM_READ | STGM_SHARE_DENY_WRITE,
		STGFMT_ANY,
		0,
		NULL,
		NULL,
		IID_IStorage,
		reinterpret_cast<void**>(&pStg));

	if (FAILED(hr))
	{
		wprintf(L"Error: couldn't open storage \"%s\" (hr = % 08x)\n",
			pwszStorageName, hr);
		
	}
	else
	{
		printf("\nDisplaying all property sets ...\n");
		DisplayStorageTree(pwszStorageName, pStg);
		pStg->Release();
	}
}

#define INCREMENT 2

void CCompondFileHelper::OutputLog(LPCWSTR pwszFormat, ...){
	wchar_t msg[255] = { 0 };
	va_list args;
	va_start(args, pwszFormat);
	swprintf_s(msg, pwszFormat, args);
	va_end(args);
	OutputDebugString(msg);
}

LPCWSTR CCompondFileHelper::GetPrefixIndent(int indent){
	if (indent >= s_indent.size()){
		throw L"Prefix string is out of range";
	}
	return s_indent[indent].c_str();
}

void CCompondFileHelper::OutputToConsole(STATSTG statStg, int type, int indent){
	wchar_t msg[255] = { 0 };
	LPCWSTR pwszPrefix = GetPrefixIndent(indent);
	if (type == STGTY_STORAGE){
		swprintf_s(msg, L"%sStorage:%s\n", pwszPrefix, statStg.pwcsName);
	}
	else if(type == STGTY_STREAM) {
		swprintf_s(msg, L"%sStream:%s\n", pwszPrefix, statStg.pwcsName);
	}
	wprintf_s(L"%s", msg);
	OutputDebugString(msg);
}


void CCompondFileHelper::DisplayAllInfo(const WCHAR *pwszStorageName)
{
	LPSTORAGE pStorage = NULL;
	
	HRESULT hr = S_OK;
	do{
		hr = StgOpenStorage(pwszStorageName, NULL, STGM_READ | STGM_SHARE_EXCLUSIVE, NULL, 0, &pStorage);

		if (FAILED(hr)){

			OutputLog(L"Open storage %s failed with code %0x", pwszStorageName, hr);
			break;
		}

		hr = DisplayStorage(pStorage, 0);
		if (FAILED(hr))
			break;
	} while (0);
	
	if (pStorage != NULL){
		pStorage->Release();
		pStorage = NULL;
	}
}

HRESULT CCompondFileHelper::GetAllItem(LPSTORAGE pStg, LPENUMSTATSTG &pEnum){
	HRESULT hr = pStg->EnumElements(0, NULL, 0, &pEnum);
	if (FAILED(hr)){
		OutputLog(L"EnumElements failed with code %0x", hr);
	}
	return hr;
}

bool CCompondFileHelper::IsStorage(const STATSTG statStg){
	return statStg.type == STGTY_STORAGE;
}

bool CCompondFileHelper::IsStream(const STATSTG statStg){
	return statStg.type == STGTY_STREAM;
}

void CCompondFileHelper::DisplayOtherInfo(STATSTG statStg, int indent){
	OutputLog(L"Other Info %s", statStg.pwcsName);
}

HRESULT CCompondFileHelper::DisplaySubStorage(LPSTORAGE pStg, STATSTG statStg, int indent){
	LPSTORAGE pSubStg = NULL;
	HRESULT hr = S_OK;
	do{
		OutputToConsole(statStg,STGTY_STORAGE, indent);
		hr = pStg->OpenStorage(statStg.pwcsName, NULL, STGM_READ | STGM_SHARE_EXCLUSIVE, NULL, 0, &pSubStg);
		if (FAILED(hr)){
			OutputLog(L"Open storage %s failed with code %0x", statStg.pwcsName, hr);
			break;
		}
		hr = DisplayStorage(pSubStg, indent + INCREMENT);
		if (FAILED(hr)){
			break;
		}
	} while (0);

	if (pSubStg != NULL){
		pSubStg->Release();
		pSubStg = NULL;
	}
	return hr;
}

HRESULT CCompondFileHelper::DisplaySubStream(LPSTORAGE pStg, STATSTG statStg, int indent){
	LPSTREAM pStream = NULL;
	HRESULT hr = S_OK;

	do{
		// todo display stream propertyInfo.
		OutputToConsole(statStg, STGTY_STREAM, indent);
		hr = pStg->OpenStream(statStg.pwcsName, NULL, STGM_READ | STGM_SHARE_EXCLUSIVE, NULL, &pStream);
		if (FAILED(hr)){
			OutputLog(L"Open stream %s failed with code %0x", statStg.pwcsName, hr);
			break;
		}

		//todo read stream.

	} while (0);

	if (pStream != NULL){
		pStream->Release();
		pStream = NULL;
	}
	return hr;
}

HRESULT CCompondFileHelper::DisplayStorage(LPSTORAGE pStg, int indent){
	LPENUMSTATSTG pEnum = NULL;
	LPSTORAGE pSubStg = NULL;
	STATSTG statStg;
	HRESULT hr = S_OK;
	memset(&statStg, 0, sizeof(statStg));

	hr = GetAllItem(pStg, pEnum);
	if (FAILED(hr))
		return hr;

	while (pEnum->Next(1, &statStg, NULL) == NOERROR)
	{
		if (IsStorage(statStg)){
			hr = DisplaySubStorage(pStg, statStg, indent);
			if (FAILED(hr))
				break;
		}
		else if (IsStream(statStg)){
			hr = DisplaySubStream(pStg, statStg, indent);
			if (FAILED(hr))
				break;
		}
		else {
			DisplayOtherInfo(statStg,indent);
		}
		CoTaskMemFree(statStg.pwcsName);
		statStg.pwcsName = NULL;
	}

	if (pEnum != NULL){
		pEnum->Release();
		pEnum = NULL;
	}
	return hr;
}
