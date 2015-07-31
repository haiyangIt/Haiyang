// DomainSearch.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <ActiveDS.h>
#include <vector>

using namespace std;

#include "DomainSearch.h"
//Activeds.lib;Adsiid.lib;
int _tmain(int argc, _TCHAR* argv[])
{
	HRESULT hr = S_OK;

	hr = Test();

	return hr;
}

HRESULT Test(){
	HRESULT hr = S_OK;

	IDirectorySearch *pDirectorySearch = NULL;

	CoInitialize(NULL);

	do{
		//hr = GetGC(&pDirectorySearch);
		hr = EnumAllObject(L"GC:", 0);

	} while (0);

	if (pDirectorySearch != NULL){
		pDirectorySearch->Release();
		pDirectorySearch = NULL;
	}

	CoUninitialize();
	getchar();
	return hr;
}

HRESULT GetGC(IDirectorySearch **ppDS)
{
	HRESULT hr;
	IEnumVARIANT *pEnum = NULL;
	IADsContainer *pCont = NULL;
	VARIANT var;
	IDispatch *pDisp = NULL;
	ULONG lFetch;

	// Set IDirectorySearch pointer to NULL.
	*ppDS = NULL;

	// First, bind to the GC: namespace container object. 
	hr = ADsOpenObject(TEXT("GC:"),
		NULL,
		NULL,
		ADS_SECURE_AUTHENTICATION, // Use Secure Authentication.
		IID_IADsContainer,
		(void**)&pCont);
	if (FAILED(hr)) {
		_tprintf(TEXT("ADsOpenObject failed: 0x%x\n"), hr);
		goto cleanup;
	}

	// Get an enumeration interface for the GC container to enumerate the 
	// contents. The actual GC is the only child of the GC container.
	hr = ADsBuildEnumerator(pCont, &pEnum);
	if (FAILED(hr)) {
		_tprintf(TEXT("ADsBuildEnumerator failed: 0x%x\n"), hr);
		goto cleanup;
	}

	// Now enumerate. There is only one child of the GC: object.
	hr = pEnum->Next(1, &var, &lFetch);
	if (FAILED(hr)) {
		_tprintf(TEXT("ADsEnumerateNext failed: 0x%x\n"), hr);
		goto cleanup;
	}

	// Get the IDirectorySearch pointer.
	if ((hr == S_OK) && (lFetch == 1))
	{
		pDisp = V_DISPATCH(&var);
		hr = pDisp->QueryInterface(IID_IDirectorySearch, (void**)ppDS);
	}

cleanup:

	if (pEnum)
		ADsFreeEnumerator(pEnum);
	if (pCont)
		pCont->Release();
	if (pDisp)
		(pDisp)->Release();
	return hr;
}

/*****************************************************************************
Function:    TestEnumObject
Arguments:   pszADsPath - ADsPath of the container to be enumerated (WCHAR*).
Return:      S_OK if successful, an error code otherwise.
Purpose:     Example using ADsBuildEnumerator, ADsEnumerateNext and
ADsFreeEnumerator.
******************************************************************************/
#define MAX_ENUM      100  

HRESULT TestEnumObject(LPWSTR pszADsPath)
{
	ULONG cElementFetched = 0L;
	IEnumVARIANT * pEnumVariant = NULL;
	VARIANT VariantArray[MAX_ENUM];
	HRESULT hr = S_OK;
	IADsContainer * pADsContainer = NULL;
	DWORD dwObjects = 0, dwEnumCount = 0, i = 0;
	BOOL  fContinue = TRUE;


	hr = ADsGetObject(
		pszADsPath,
		IID_IADsContainer,
		(void **)&pADsContainer
		);


	if (FAILED(hr)) {

		printf("\"%S\" is not a valid container object.\n", pszADsPath);
		goto exitpoint;
	}

	hr = ADsBuildEnumerator(
		pADsContainer,
		&pEnumVariant
		);

	if (FAILED(hr))
	{
		printf("ADsBuildEnumerator failed with %lx\n", hr);
		goto exitpoint;
	}

	fContinue = TRUE;
	while (fContinue) {

		IADs *pObject;

		hr = ADsEnumerateNext(
			pEnumVariant,
			MAX_ENUM,
			VariantArray,
			&cElementFetched
			);

		if (FAILED(hr))
		{
			printf("ADsEnumerateNext failed with %lx\n", hr);
			goto exitpoint;
		}

		if (hr == S_FALSE) {
			fContinue = FALSE;
		}

		dwEnumCount++;

		for (i = 0; i < cElementFetched; i++) {

			IDispatch *pDispatch = NULL;
			BSTR        bstrADsPath = NULL;

			pDispatch = VariantArray[i].pdispVal;

			hr = V_DISPATCH(VariantArray + i)->QueryInterface(IID_IADs, (void **)&pObject);

			if (SUCCEEDED(hr))
			{
				pObject->get_ADsPath(&bstrADsPath);
				
				printf("%S\n", bstrADsPath);
			}
			pObject->Release();
			VariantClear(VariantArray + i);
			SysFreeString(bstrADsPath);
		}

		dwObjects += cElementFetched;
	}

	printf("Total Number of Objects enumerated is %d\n", dwObjects);

exitpoint:
	if (pEnumVariant) {
		ADsFreeEnumerator(pEnumVariant);
	}

	if (pADsContainer) {
		pADsContainer->Release();
	}

	return(hr);
}

vector<wstring> indentVector;
LPCWSTR GetIndent(int indent){
	if (indentVector.size() == 0){
		indentVector.push_back(L"");
		indentVector.push_back(L"--");
		indentVector.push_back(L"----");
		indentVector.push_back(L"------");
		indentVector.push_back(L"--------");
		indentVector.push_back(L"----------");
		indentVector.push_back(L"------------");
		indentVector.push_back(L"--------------");
		indentVector.push_back(L"----------------");
		indentVector.push_back(L"------------------");
		indentVector.push_back(L"--------------------");
	}
	return indentVector[indent].c_str();
}

HRESULT EnumAllObject(LPWSTR pszADsPath, int indent)
{
	ULONG cElementFetched = 0L;
	IEnumVARIANT * pEnumVariant = NULL;
	VARIANT VariantArray[MAX_ENUM];
	HRESULT hr = S_OK;
	IADsContainer * pADsContainer = NULL;
	DWORD dwObjects = 0, dwEnumCount = 0, i = 0;
	BOOL  fContinue = TRUE;


	hr = ADsGetObject(
		pszADsPath,
		IID_IADsContainer,
		(void **)&pADsContainer
		);


	if (FAILED(hr)) {

		printf("\"%S\" is not a valid container object.\n", pszADsPath);
		goto exitpoint;
	}

	hr = ADsBuildEnumerator(
		pADsContainer,
		&pEnumVariant
		);

	if (FAILED(hr))
	{
		printf("ADsBuildEnumerator failed with %lx\n", hr);
		goto exitpoint;
	}

	fContinue = TRUE;
	while (fContinue) {

		IADs *pObject;

		hr = ADsEnumerateNext(
			pEnumVariant,
			MAX_ENUM,
			VariantArray,
			&cElementFetched
			);

		if (FAILED(hr))
		{
			printf("ADsEnumerateNext failed with %lx\n", hr);
			goto exitpoint;
		}

		if (hr == S_FALSE) {
			fContinue = FALSE;
		}

		dwEnumCount++;

		for (i = 0; i < cElementFetched; i++) {

			IDispatch *pDispatch = NULL;
			BSTR        bstrADsPath = NULL;

			pDispatch = VariantArray[i].pdispVal;

			hr = V_DISPATCH(VariantArray + i)->QueryInterface(IID_IADs, (void **)&pObject);

			if (SUCCEEDED(hr))
			{
				pObject->get_ADsPath(&bstrADsPath);
				printf("%S\n", bstrADsPath);
				EnumAllObject(bstrADsPath, indent + 2);
			}
			pObject->Release();
			VariantClear(VariantArray + i);
			SysFreeString(bstrADsPath);
		}

		dwObjects += cElementFetched;
	}

	printf("Total Number of Objects enumerated is %d\n", dwObjects);

exitpoint:
	if (pEnumVariant) {
		ADsFreeEnumerator(pEnumVariant);
	}

	if (pADsContainer) {
		pADsContainer->Release();
	}

	return(hr);
}