#include "stdafx.h"
#include <EDK.H>
#include "MapiError111.h"
#include <MAPIX.h>
#include <edkmdb.h>
#include <MAPIUtil.h>
#include <time.h>
#include <stdlib.h>
#include <iostream>
#include <EDK.H>

using namespace std;

#define NUM_PROFILE_PROP 12

CMapiError111::CMapiError111()
{
	LONG hr = 0;
	MAPIINIT_0 init_0;

	memset(&init_0, 0, sizeof(MAPIINIT_0));

	init_0.ulVersion = MAPI_INIT_VERSION;
	init_0.ulFlags = MAPI_NT_SERVICE | MAPI_MULTITHREAD_NOTIFICATIONS;

	hr = MAPIInitialize(&init_0);
	if (FAILED(hr))
		m_initError = hr;
}

CMapiError111::~CMapiError111()
{
	if (SUCCEEDED(m_initError))
	{
		MAPIUninitialize();
	}
}

HRESULT CMapiError111::Reproduce(){
	//Sleep(20 * 1000);
	string temp;
	cout << "Keep profile? Y/N:" << endl;
	cin >> temp;
	if (temp == "Y" || temp == "y"){
		m_bKeepProfile = true;
	}
	else {
		m_bKeepProfile = false;
	}

	cout << "Need change mailbox service or mailbox name? Y/N:" << endl;
	cin >> temp;
	if (temp == "Y" || temp == "y"){
		cout << "Please input exchange server(For PR_PROFILE_UNRESOLVED_SERVER):" << endl;
		cin >> temp;
		m_mailBoxServer = temp;

		cout << "Please input mailbox name(For PR_PROFILE_UNRESOLVED_NAME):" << endl;
		cin >> temp;
		m_mailBoxName = temp;

		cout << "Please input proxy server name(For PR_PROFILE_RPC_PROXY_SERVER_)" << endl;
		cin >> temp;
		m_mailProxyServer = temp;
	}
	else {
		//m_mailBoxServer = "zhash05-ex13-2.autoex13-1.com";
		//m_mailBoxServer = "c3a48656-026d-4a05-850a-c8add3e7c3f7@autoex13-1.com";
		m_mailBoxServer = "2f7f1176-6e9c-40a2-8655-b2b4c8bec32d@LINHA05-EX20131.com";
		 m_mailBoxName = "administrator";
		//m_mailBoxName = "/o=Exchange GRT E15/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Recipients/cn=061becc8b46e40ae8592f53dbaa6b67e-Adminis";
		//m_mailProxyServer = "mail.autoex13-1.com";
		//m_mailProxyServer = "zhash05-ex13-2.autoex13-1.com";
		 m_mailProxyServer = "linha05-m-cas10.linha05-ex20131.com";
	}

	cout << "Is SSL On" << endl;
	cin >> temp;
	if (temp == "Y")
		m_hasSSL = true;
	else
		m_hasSSL = false;

	cout << "Basic:0, NTLM:1, Negotiate:2" << endl;
	cin >> temp;
	if (temp == "0")
		m_auth = 1;
	else if (temp == "1")
		m_auth = 2;
	else if (temp == "2")
		m_auth = 16;

	cout << "----------------------------------------------------" << endl << endl;

	HRESULT hr = ConnectServer();

	getchar();
	return hr;
}

HRESULT CMapiError111::ConnectServer(){
	string profileName = "";
	HRESULT hr = S_OK;
	LPMAPISESSION lpSession = NULL;
	bool bCreateProfile = false;
	bool bCreateProfileError = false;
	do{
		hr = CreateProfile(profileName, bCreateProfile);
		if (FAILED(hr)){
			bCreateProfileError = true;
			m_helper.OutputLog("Create profile failed with %0x", hr);
			break;
		}

		hr = MAPILogonEx(0, (LPTSTR)profileName.c_str(), NULL, MAPI_NO_MAIL | MAPI_NEW_SESSION | MAPI_NT_SERVICE | MAPI_EXPLICIT_PROFILE | MAPI_EXTENDED, &(lpSession));
		if (FAILED(hr)){
			m_helper.OutputLog("MAPILogonEx failed with %0x", hr);
			break;
		}

		hr = OpenStore(lpSession);
		if (FAILED(hr)){
			m_helper.OutputLog("OpenStore failed with %0x", hr);
			if (hr == 0x80040111){
				m_helper.OutputLog("Reproduce ok");
			}
		}
		else
		{
			m_helper.OutputLog("OpenStore success");
		}

	} while (0);

	if (bCreateProfile && (bCreateProfileError || !m_bKeepProfile))
		DeleteProfile(profileName);

	if (lpSession){
		lpSession->Release();
		lpSession = NULL;
	}
	return hr;
}

HRESULT CMapiError111::OpenStore(const LPMAPISESSION lpSession){
	LONG hr = 0;
	IMAPITable* pIStoreTable = 0;
	LPSRowSet pRows = 0;
	ULONG ulRowCount = 0;
	ULONG ulRowIndex = 0;
	BOOL bFind = FALSE;
	IMsgStore* pMsgStore = NULL;
	IExchangeManageStore * pExManageStore = NULL;

	enum{ IDX_ID, IDX_PR, COUNT };
	SizedSPropTagArray(COUNT, storeEntryID) = { COUNT, { PR_ENTRYID, PR_MDB_PROVIDER } };

	hr = lpSession->GetMsgStoresTable(0, &pIStoreTable);

	if (FAILED(hr))
	{
		m_helper.OutputLog("Failed to GetMsgStoresTable():%x", hr);
	}

	if (SUCCEEDED(hr))
	{
		hr = pIStoreTable->SetColumns((LPSPropTagArray)&storeEntryID, 0);

		if (FAILED(hr))
		{
			m_helper.OutputLog("Failed to pIStoreTable->SetColumns():%x", hr);
		}
	}

	if (SUCCEEDED(hr))
	{
		hr = pIStoreTable->GetRowCount(0, &ulRowCount);
		if (FAILED(hr))
		{
			m_helper.OutputLog("Failed to pIStoreTable->GetRowCount():%x", hr);
		}
	}

	if (SUCCEEDED(hr))
	{
		hr = pIStoreTable->QueryRows(ulRowCount, 0, &pRows);
		if (FAILED(hr))
		{
			m_helper.OutputLog("Failed to pIStoreTable->QueryRows():%x", hr);
		}
	}

	if (SUCCEEDED(hr))
	{
		ulRowIndex = 0;
		while (ulRowIndex<pRows->cRows)
		{
			if (memcmp((void*)pbExchangeProviderPrimaryUserGuid, pRows->aRow[ulRowIndex].lpProps[IDX_PR].Value.bin.lpb,
				pRows->aRow[ulRowIndex].lpProps[IDX_PR].Value.bin.cb) == 0 &&
				pRows->aRow[ulRowIndex].lpProps[IDX_PR].ulPropTag == PR_MDB_PROVIDER)
			{
				bFind = TRUE;
				break;
			}

			ulRowIndex++;
		}
	}

	if (bFind)
	{
		hr = lpSession->OpenMsgStore(0, pRows->aRow[ulRowIndex].lpProps[IDX_ID].Value.bin.cb,
			(ENTRYID*)pRows->aRow[ulRowIndex].lpProps[IDX_ID].Value.bin.lpb, NULL,
			MDB_WRITE | MAPI_BEST_ACCESS | MDB_NO_DIALOG, (IMsgStore**)&pMsgStore);
		if (FAILED( hr))
		{
			m_helper.OutputLog("Failed m_pSession->OpenMsgStore():%x", hr);
		}
	}

	if (SUCCEEDED(hr) && pMsgStore)
	{
		hr = pMsgStore->QueryInterface(IID_IExchangeManageStore, (LPVOID*)&pExManageStore);
		if (FAILED( hr))
		{
			m_helper.OutputLog("m_pExStore->QueryInterface(IID_IExchangeManageStore):%x", hr);
		}
	}

	if (pIStoreTable)
	{
		pIStoreTable->Release();
		pIStoreTable = NULL;
	}

	if (pRows)
	{
		FreeProws(pRows);
		pRows = NULL;
	}

	if (pExManageStore){
		pExManageStore->Release();
		pExManageStore = NULL;
	}

	if (pMsgStore){
		pMsgStore->Release();
		pMsgStore = NULL;
	}

	return hr;
}

HRESULT CMapiError111::GetProfileName(string &profileName){
	//todo
	SYSTEMTIME time;
	GetLocalTime(&time);
	char timeStr[60] = { 0 };
	sprintf_s(timeStr,"%4d-%02d-%02d-%02d-%02d-%02d",time.wYear,time.wMonth,time.wDay,time.wHour,time.wMinute,time.wSecond);
	profileName = timeStr;
	return S_OK;
}

HRESULT CMapiError111::CreateProfile(string &profileName, bool &bCreateProfile){
	LPPROFADMIN lpProfAdmin = NULL;
	HRESULT hr = S_OK;
	profileName = "";
	LPSERVICEADMIN lpServiceAdmin = NULL;
	LPMAPITABLE lpMapiTable = NULL;
	LPSRowSet pRows = NULL;
	SPropValue * pPropArray = NULL;

	const int	MAX_PROF_PROP = 2;
	SizedSPropTagArray(MAX_PROF_PROP, pProfProps) = { MAX_PROF_PROP, { PR_SERVICE_NAME_A, PR_SERVICE_UID } };
	enum	ProfPropIndex	{ IDX_PF_SERVICE_NAME, IDX_PF_SERVICE_UID };

	bCreateProfile = false;

	do{
		hr = MAPIAdminProfiles(0, &lpProfAdmin);
		if (FAILED(hr)){
			m_helper.OutputLog("Admin Profiles failed with %0x", hr);
			break;
		}

		hr = GetProfileName(profileName);
		if (FAILED(hr)){
			m_helper.OutputLog("GetProfileName failed with %0x", hr);
			break;
		}
		else {
			m_helper.OutputLog("ProfileName is %s", profileName.c_str());
		}

		hr = lpProfAdmin->CreateProfile((LPTSTR)profileName.c_str(), NULL, NULL, 0);
		if (hr == MAPI_E_NO_ACCESS){
			m_helper.OutputLog("Profile %s is Exists", profileName.c_str());
			break;
		}
		
		bCreateProfile = true;
		hr = lpProfAdmin->AdminServices((LPTSTR)profileName.c_str(), NULL, NULL, 0, &lpServiceAdmin);
		if (FAILED(hr)){
			m_helper.OutputLog("AdminService failed with %0x", hr);
			break;
		}

		hr = lpServiceAdmin->CreateMsgService((LPTSTR)"MSEMS", NULL, 0, 0);
		if (FAILED(hr)){
			m_helper.OutputLog("CreateMsgService failed with %0x", hr);
			break;
		}

		// Set profile Properties
		ULONG ulIndex = 0;
		std::string strPO = ""; //parameter output

		pPropArray = new SPropValue[NUM_PROFILE_PROP];
		if (NULL == pPropArray)
		{
			hr = E_OUTOFMEMORY;
			m_helper.OutputLog("Failed to allocate buffer for SPropValue");
			break;
		}

		memset(pPropArray, 0, sizeof(SPropValue)* NUM_PROFILE_PROP);

		//0 PR_PROFILE_UNRESOLVED_SERVER 
		// Format is "Mailbox GUID @domain" 
		ulIndex = 0;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_UNRESOLVED_SERVER;
		//pPropArray[ulIndex].Value.lpszA = "c3a48656-026d-4a05-850a-c8add3e7c3f7@autoex13-1.com";
		//pPropArray[ulIndex].Value.lpszA = "zhash05-ex13-2.autoex13-1.com";
		pPropArray[ulIndex].Value.lpszA = (LPSTR)m_mailBoxServer.c_str();

		//1 PR_PROFILE_UNRESOLVED_NAME
		// The legacyDN of mailbox OR display name
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_UNRESOLVED_NAME;
		//pPropArray[ulIndex].Value.lpszA = "/o=Exchange GRT E15/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Recipients/cn=061becc8b46e40ae8592f53dbaa6b67e-Adminis";
		pPropArray[ulIndex].Value.lpszA = (LPSTR)m_mailBoxName.c_str();

		//2 PR_PROFILE_UI_STATE
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_UI_STATE;
		pPropArray[ulIndex].Value.l = 0x4000;

		//3 PR_PROFILE_CONFIG_FLAGS
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_CONFIG_FLAGS;
		pPropArray[ulIndex].Value.l = CONFIG_SERVICE;

		//4 PR_PROFILE_CONNECT_FLAGS
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_CONNECT_FLAGS;
		pPropArray[ulIndex].Value.l = 0x8024;

		//5 PR_CONVERSION_PROHIBITED 0x3A03000B
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_CONVERSION_PROHIBITED;
		pPropArray[ulIndex].Value.b = 1;

		//6 PR_PROFILE_RPC_PROXY_SERVER		CAS machine name
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_;
		//pPropArray[ulIndex].Value.lpszA = "zhash05-ex13-2.autoex13-1.com";
		pPropArray[ulIndex].Value.lpszA = (LPSTR)m_mailProxyServer.c_str();

		//7 PR_PROFILE_RPC_PROXY_SERVER_FLAGS
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_FLAGS_;
		if (m_hasSSL)
			pPropArray[ulIndex].Value.l = 0x13;
		else 
			pPropArray[ulIndex].Value.l = 0x01;
		
		//8 PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE

		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_;
		pPropArray[ulIndex].Value.l = m_auth;

		//9 PR_PROFILE_AUTH_PACKAGE	
		ulIndex++;
		pPropArray[ulIndex].ulPropTag = PR_PROFILE_AUTH_PACKAGE_;
		pPropArray[ulIndex].Value.l = 10;
		ulIndex++;

		hr = lpServiceAdmin->GetMsgServiceTable(0, &lpMapiTable);

		if (FAILED( hr))
		{
			m_helper.OutputLog(" Failed to pIServiceAdmin->GetMsgServiceTable(%x) ... ", hr);
			break;
		}
		
		hr = lpMapiTable->SetColumns((LPSPropTagArray)&pProfProps, 0);
		if (FAILED (hr))
		{
			break;
		}

		SPropValue ServiceName;
		SRestriction sRestrict;

		ServiceName.ulPropTag = PR_SERVICE_NAME_A;
		ServiceName.Value.lpszA = (char*)"MSEMS";

		sRestrict.rt = RES_CONTENT;
		sRestrict.res.resContent.ulFuzzyLevel = FL_FULLSTRING;
		sRestrict.res.resContent.ulPropTag = PR_SERVICE_NAME_A;
		sRestrict.res.resContent.lpProp = &ServiceName;

		hr = lpMapiTable->FindRow(&sRestrict, BOOKMARK_BEGINNING, 0);

		if (FAILED(hr))
		{
			m_helper.OutputLog(" Failed to IServiceTable->FindRow(%x) ... ", hr);
			break;
		}

		hr = lpMapiTable->QueryRows(1, 0, &pRows);
		if (hr || pRows == 0 || (PROP_TYPE(pRows->aRow[0].lpProps[IDX_PF_SERVICE_UID].ulPropTag) == PT_ERROR) || ((MAPIUID*)pRows->aRow[0].lpProps[IDX_PF_SERVICE_UID].Value.bin.lpb == 0))
		{
			m_helper.OutputLog(" Failed to IServiceTable->QueryRows(%x) ... ", hr);
			break;
		}

		hr = lpServiceAdmin->ConfigureMsgService((MAPIUID*)pRows->aRow[0].lpProps[IDX_PF_SERVICE_UID].Value.bin.lpb,
			0, 0, ulIndex, pPropArray);

		if (FAILED(hr))
		{
			m_helper.OutputLog(" Failed to pIServiceAdmin->ConfigureMsgService(%x) ... ", hr);
			break;
		}
	} while (false);

	if (pRows)
	{
		FreeProws(pRows);
		pRows = NULL;
	}

	if (lpMapiTable)
	{
		lpMapiTable->Release();
		lpMapiTable = NULL;
	}

	if (lpServiceAdmin)
	{
		lpServiceAdmin->Release();
		lpServiceAdmin = NULL;
	}

	if (lpProfAdmin)
	{
		lpProfAdmin->Release();
		lpProfAdmin = NULL;
	}

	if (pPropArray)
	{
		delete[]pPropArray;
		pPropArray = NULL;
	}
	return hr;
}

LONG CMapiError111::DeleteProfile(const string profileName)
{
	IProfAdmin* pProfAdmin = 0;
	ULONG hr = 0;

	do
	{
		hr = MAPIAdminProfiles(0, &pProfAdmin);
		if (hr)
		{
			break;
		}
			
		hr = pProfAdmin->DeleteProfile((LPTSTR)profileName.c_str(), 0); // unicode attention 
		if (hr)
		{
			break;
		}
	} while (0);

	if (pProfAdmin)
	{
		pProfAdmin->Release();
		pProfAdmin = NULL;
	}
	return	hr;
}