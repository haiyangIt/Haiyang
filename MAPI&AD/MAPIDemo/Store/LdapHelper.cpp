#include "stdafx.h"
#include "LdapHelper.h"
#include <ActiveDS.h>
#include <string>
#include <Atlbase.h>
#include <vector>
#include <atlstr.h>

using namespace std;

CLdapHelper::CLdapHelper()
{
	HRESULT hr = GetRootDse(m_stRootDSE);
	if (hr){
		m_stRootDSE = NULL;
	}
}


CLdapHelper::~CLdapHelper()
{
	if (m_stRootDSE){
		m_stRootDSE->Release();
		m_stRootDSE = NULL;
	}
}

HRESULT CLdapHelper::GetRootDse(IADs* &stRootDSE){
	HRESULT hr = S_OK;
	stRootDSE = NULL;

	wstring wsExchMDBPath;
	//IADs * piIADs;
	hr = ADsGetObject(L"LDAP://rootDSE", __uuidof(IADs), (void**)&stRootDSE);
	return hr;
}

HRESULT CLdapHelper::GetRootSearch(IDirectorySearch* &spSearch){
	wstring wsExchMDBPath;
	VARIANT varContext;
	VariantInit(&varContext);
	HRESULT hr = m_stRootDSE->Get(L"configurationNamingContext", &varContext);
	if (hr)
		return hr;

	wsExchMDBPath = L"LDAP://";
	wsExchMDBPath += varContext.bstrVal;

	hr = ADsGetObject(wsExchMDBPath.c_str(), __uuidof(IDirectorySearch), (void**)&spSearch);
	if (hr)
		return hr;

	// Search the entire subtree from the root.
	ADS_SEARCHPREF_INFO pSearchPrefs;
	pSearchPrefs.dwSearchPref = ADS_SEARCHPREF_SEARCH_SCOPE;
	pSearchPrefs.vValue.dwType = ADSTYPE_INTEGER;
	pSearchPrefs.vValue.Integer = ADS_SCOPE_SUBTREE;
	DWORD dwNumPrefs = 1;
	// Set the search preference.
	hr = spSearch->SetSearchPreference(&pSearchPrefs, dwNumPrefs);
	return hr;
}

DWORD CLdapHelper::GetSslFlag(){

}

HRESULT CLdapHelper::GetInfo(){
	HRESULT hr = 0;
	vector<ST_GRT_CAS_INFO> casInfoList;

	CoInitialize(NULL);

	//TRACE_TRACE(GRTUT, CA_LOG_COM, L"GetE15CASs start()...");
	do
	{
		HRESULT hr = S_OK;
		IDirectorySearch* spSearch;
		hr = GetRootSearch(spSearch);

		// Search the Public Folder Info.
		ADS_SEARCH_HANDLE hSearch = NULL;
		LPWSTR wzMDBAttName[] = { L"distinguishedName", L"cn", L"serialNumber", L"msExchCurrentServerRoles" };
		hr = spSearch->ExecuteSearch((LPWSTR)L"objectClass=msExchExchangeServer", wzMDBAttName, _countof(wzMDBAttName), &hSearch);
		if (FAILED(hr))
		{
			break;
		}
		hr = spSearch->GetNextRow(hSearch);
		//////////////////////////////////////////////////////////////////////////
		while (hr != S_ADS_NOMORE_ROWS)
		{
			BOOL isE15Server = TRUE;

			HRESULT hrLocal;

			ADS_SEARCH_COLUMN colDN = { 0 };
			ADS_SEARCH_COLUMN colName = { 0 };
			ADS_SEARCH_COLUMN colVersion = { 0 };
			ADS_SEARCH_COLUMN colRoles = { 0 };

			std::wstring strDN = L"";
			std::wstring strCN = L"";

			hrLocal = spSearch->GetColumn(hSearch, wzMDBAttName[0], &colDN);
			if (SUCCEEDED(hrLocal) && (colDN.dwNumValues >0))
			{
				if (colDN.pADsValues->CaseIgnoreString)
				{
					strDN = colDN.pADsValues->CaseIgnoreString;
				}
			}

			hrLocal = spSearch->GetColumn(hSearch, wzMDBAttName[1], &colName);
			if (SUCCEEDED(hrLocal) && (colName.dwNumValues > 0))
			{
				if (colName.pADsValues->CaseIgnoreString)
				{
					strCN = colName.pADsValues->CaseIgnoreString;
				}
			}

			hrLocal = spSearch->GetColumn(hSearch, wzMDBAttName[2], &colVersion);

			CString strVersion = L"";

			if (SUCCEEDED(hrLocal) && (colVersion.dwNumValues >0))
			{
				if (colVersion.pADsValues->CaseIgnoreString)
				{
					strVersion = colVersion.pADsValues->CaseIgnoreString;
					strVersion.MakeLower();
					if (-1 == strVersion.Find(L"version 15"))
					{
						isE15Server = 0;
						//TRACE_TRACE(GRTUT, CA_LOG_COM, L"---Server(%s : %s) isn't E15 Role", strCN.c_str(), strVersion);
					}

				}
			}

			if (isE15Server)
			{
				hrLocal = spSearch->GetColumn(hSearch, wzMDBAttName[3], &colRoles);
				if (SUCCEEDED(hrLocal) && (colRoles.dwNumValues > 0))
				{
					DWORD dwRoles = colRoles.pADsValues->Integer;

					if (0x4001 == dwRoles || 0x4037 == dwRoles) //0x4001: Client access role. 0x4037: both Client access role and Mailbox role
					{
						ST_GRT_CAS_INFO stItem;
						stItem.strCASCN = strCN;
						stItem.strCASDN = strDN;

						GetCASDetail(strCN, strDN, stItem);
						m_CASList.vecCASItems.push_back(stItem);

					}
				}
				if (ADSTYPE_INVALID != colRoles.dwADsType)
				{
					spSearch->FreeColumn(&colRoles);
				}
			}

			if (ADSTYPE_INVALID != colDN.dwADsType)
			{
				spSearch->FreeColumn(&colDN);
			}

			if (ADSTYPE_INVALID != colName.dwADsType)
			{
				spSearch->FreeColumn(&colName);
			}


			if (ADSTYPE_INVALID != colVersion.dwADsType)
			{
				spSearch->FreeColumn(&colVersion);
			}

			hr = spSearch->GetNextRow(hSearch);


		}
		//////////////////////////////////////////////////////////////////////////

		if (spSearch)
		{
			spSearch->CloseSearchHandle(hSearch);
		}

		if (hr == E_ADS_COLUMN_NOT_SET)
		{
			hr = S_OK;
		}

		if (hr == S_ADS_NOMORE_ROWS)
		{
			hr = S_OK;
		}

	} while (0);

	CoUninitialize();

	//TRACE_TRACE(GRTUT, CA_LOG_COM, L"GetE15CASs end() number of cas:%d...", m_CASList.vecCASItems.size());

	return S_OK;
}

HRESULT GetCASDetail(const wstring strCN, const wstring strDN, ST_GRT_CAS_INFO & item){
	HRESULT hr = 0;
	//TRACE_TRACE(GRTUT, CA_LOG_COM, L"GetCASDetail start(%s)...", strDN.c_str());
	do
	{
		if (strDN.length() == 0)
		{
			//TRACE_TRACE(GRTUT, CA_LOG_COM, L"empty DN");
			hr = E_INVALIDARG;
			break;
		}

		VARIANT varContext;
		VariantInit(&varContext);

		std::wstring strLdapPath;
		strLdapPath = L"LDAP://";
		strLdapPath += strDN;

		// Bind to the root of the configuration context
		CComPtr<IDirectorySearch> spSearch;

		hr = ADsGetObject(strLdapPath.c_str(), __uuidof(IDirectorySearch), (void**)&spSearch);
		if (FAILED(hr))
		{
			//TRACE_TRACE(GRTUT, CA_LOG_WARN, L" Failed to ADsGetObject(%s):0x%08x", strLdapPath.c_str(), hr);
			break;
		}

		// Search the entire subtree from the root.
		ADS_SEARCHPREF_INFO pSearchPrefs;
		pSearchPrefs.dwSearchPref = ADS_SEARCHPREF_SEARCH_SCOPE;
		pSearchPrefs.vValue.dwType = ADSTYPE_INTEGER;
		pSearchPrefs.vValue.Integer = ADS_SCOPE_SUBTREE;
		DWORD dwNumPrefs = 1;
		// Set the search preference.
		hr = spSearch->SetSearchPreference(&pSearchPrefs, dwNumPrefs);
		if (FAILED(hr))
		{
			//TRACE_TRACE(GRTUT, CA_LOG_WARN, L" Failed to SetSearchPreference():0x%08x", hr);
			break;
		}

		// Search the Public Folder Info.
		ADS_SEARCH_HANDLE hSearch = NULL;
		LPWSTR wzATTrNames[] = { L"distinguishedName", L"msExchRpcHttpFlags", L"msExchExternalAuthenticationMethods", L"msExchInternalAuthenticationMethods", L"msExchExternalHostName", L"msExchInternalHostName" };
		hr = spSearch->ExecuteSearch((LPWSTR)L"objectClass=msExchRpcHttpVirtualDirectory", wzATTrNames, _countof(wzATTrNames), &hSearch);
		if (FAILED(hr))
		{
			//TRACE_TRACE(GRTUT, CA_LOG_WARN, L" Failed to ExecuteSearch():0x%08x", hr);
			break;
		}
		hr = spSearch->GetNextRow(hSearch);
		//////////////////////////////////////////////////////////////////////////
		while (hr != S_ADS_NOMORE_ROWS)
		{
			BOOL isE15Server = TRUE;

			HRESULT hrLocal;

			ADS_SEARCH_COLUMN colDN = { 0 };
			ADS_SEARCH_COLUMN colRpcFlags = { 0 };
			ADS_SEARCH_COLUMN colExternalMethods = { 0 };
			ADS_SEARCH_COLUMN colInternalMethods = { 0 };

			ADS_SEARCH_COLUMN colExternalHostName = { 0 };
			ADS_SEARCH_COLUMN colInternalHostName = { 0 };



			std::wstring strDN = L"";
			std::wstring strExHostName = strCN;
			std::wstring strInHostName = strCN;

			DWORD dwFlags = 0;
			// BASIC:1, NTLM:2, Negotiate:8192
			DWORD dwExternalMethods = 2;   //NTLM
			DWORD dwInternalMethods = 8195;//BASIC & NTLM & Negotiate

			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[0], &colDN);
			if (SUCCEEDED(hrLocal) && (colDN.dwNumValues >0))
			{
				if (colDN.pADsValues->CaseIgnoreString)
				{
					strDN = colDN.pADsValues->CaseIgnoreString;
				}
			}

			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[1], &colRpcFlags);
			if (SUCCEEDED(hrLocal) && (colRpcFlags.dwNumValues > 0))
			{
				dwFlags = colRpcFlags.pADsValues->Integer;
			}

			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[2], &colExternalMethods);
			if (SUCCEEDED(hrLocal) && (colExternalMethods.dwNumValues >0))
			{
				dwExternalMethods = colExternalMethods.pADsValues->Integer;
			}

			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[3], &colInternalMethods);
			if (SUCCEEDED(hrLocal) && (colInternalMethods.dwNumValues >0))
			{
				dwInternalMethods = colInternalMethods.pADsValues->Integer;
			}

			// msExchExternalHostName index 4
			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[4], &colExternalHostName);
			if (SUCCEEDED(hrLocal) && (colExternalHostName.dwNumValues >0))
			{
				if (colExternalHostName.pADsValues->CaseIgnoreString)
				{
					strExHostName = colExternalHostName.pADsValues->CaseIgnoreString;
				}
			}

			// msExchInternalHostName index 5
			hrLocal = spSearch->GetColumn(hSearch, wzATTrNames[5], &colInternalHostName);
			if (SUCCEEDED(hrLocal) && (colInternalHostName.dwNumValues >0))
			{
				if (colInternalHostName.pADsValues->CaseIgnoreString)
				{
					strInHostName = colInternalHostName.pADsValues->CaseIgnoreString;
				}
			}
			//////////////////////////////////////////////////////////////////////////

			item.strCASrpcDN = strDN;
			item.rpcHttpFlags = dwFlags;
			item.ExternalMethods = dwExternalMethods;
			item.InternalMethods = dwInternalMethods;

			item.strExternalHostName = Helper::FormatHostname(strExHostName);
			item.strInternalHostName = Helper::FormatHostname(strInHostName);

			/*TRACE_TRACE(GRTUT, CA_LOG_COM, L"Flags:%d ExMethods:%d InMethods:%d \r\nExHostName:%s[%s] \r\nInHostName:%s[%s]\r\nDN:%s",
				item.rpcHttpFlags, item.ExternalMethods, item.InternalMethods,
				item.strExternalHostName.c_str(), strExHostName.c_str(), item.strInternalHostName.c_str(), strInHostName.c_str(), item.strCASrpcDN.c_str());*/
			//////////////////////////////////////////////////////////////////////////


			if (ADSTYPE_INVALID != colDN.dwADsType)
			{
				spSearch->FreeColumn(&colDN);
			}

			if (ADSTYPE_INVALID != colRpcFlags.dwADsType)
			{
				spSearch->FreeColumn(&colRpcFlags);
			}

			if (ADSTYPE_INVALID != colExternalMethods.dwADsType)
			{
				spSearch->FreeColumn(&colExternalMethods);
			}


			if (ADSTYPE_INVALID != colInternalMethods.dwADsType)
			{
				spSearch->FreeColumn(&colInternalMethods);
			}

			if (ADSTYPE_INVALID != colExternalHostName.dwADsType)
			{
				spSearch->FreeColumn(&colExternalHostName);
			}

			if (ADSTYPE_INVALID != colInternalHostName.dwADsType)
			{
				spSearch->FreeColumn(&colInternalHostName);
			}


			hr = spSearch->GetNextRow(hSearch);


		}
		//////////////////////////////////////////////////////////////////////////

		if (spSearch)
		{
			spSearch->CloseSearchHandle(hSearch);
		}

		if (hr == E_ADS_COLUMN_NOT_SET)
		{
			hr = S_OK;
		}

		if (hr == S_ADS_NOMORE_ROWS)
		{
			hr = S_OK;
		}

	} while (0);

	//TRACE_TRACE(GRTUT, CA_LOG_COM, L"GetCASDetail end()...");

	return 0;
}