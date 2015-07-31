// Store.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <MAPIX.h>
#include <Mapiutil.h>
#include "edkmdb.h"
#include <IMessage.h>
#include "MapiDemo.h"
#include <guiddef.h>

// {00020D0B-0000-0000-C000-000000000046}
DEFINE_GUID(CLSID_MailMessage,
	0x00020D0B,
	0x0000, 0x0000, 0xC0, 0x00, 0x0, 0x00, 0x0, 0x00, 0x00, 0x46);// Exported from StubUtils.cpp

HMODULE GetMAPIHandle();
void UnLoadPrivateMAPI();
void ForceOutlookMAPI(bool fForce);
void ForceSystemMAPI(bool fForce);
void SetMAPIHandle(HMODULE hinstMAPI);
HMODULE GetPrivateMAPI();
bool GetComponentPath(LPCSTR szComponent, LPSTR szQualifier, LPSTR szDllPath, DWORD cchBufferSize, bool fInstall);

//int _tmain(int argc, _TCHAR* argv[])
//{
//	UnLoadPrivateMAPI();
//#ifdef MAPI32_W
//	//HMODULE hm = LoadLibrary(L"MSMAPI32.DLL");
//#else
//	//HMODULE hm = LoadLibrary(L"EXMAPI32.DLL");
//#endif
//	HRESULT hr = S_OK;
//	//if (hm != NULL)
//	//{
//	//	SetMAPIHandle(hm);
//		Sleep(20 * 1000);
//		hr = ModifyProfileGlobalSection_SaveMsgFile();
//
//	//	FreeLibrary(hm);
//		if (SUCCEEDED(hr))
//			return 0;
//		else
//			return hr;
//	//}
//	//else
//	//	return 1;
//	return 0;
//}

HRESULT ModifyProfileGlobalSection_SaveMsgFile(){
	HRESULT hr = 0;
	LPMAPISESSION lpMapiSession = NULL;
	LPMDB lpMdb = NULL;
	IMAPITable* pIStoreTable = NULL;
	LPSRowSet pRows = NULL;
	IUnknown* lpExchManageStroe = NULL;
	SPropValue*	pAllFoldersPropValue = NULL;

	do{
		MAPIINIT_0 init_0;
		memset(&init_0, 0, sizeof(MAPIINIT_0));

		init_0.ulVersion = MAPI_INIT_VERSION;
		init_0.ulFlags = MAPI_NT_SERVICE | MAPI_MULTITHREAD_NOTIFICATIONS;

		hr = MAPIInitialize(&init_0);

		DEFINE_IF_HR_NT_OK_BREAK(hr);

		// if you want to create profile, uncomment following code.
		DEFINE_IF_HR_NT_OK_BREAK(hr = ConfigMsgService());
		break;
		DEFINE_IF_HR_NT_OK_BREAK(hr = ModifyGlobalProfileSection());
		// break;

		hr = MAPILogonEx(0, PROFILENAME, NULL, MAPI_NEW_SESSION | MAPI_EXPLICIT_PROFILE | MAPI_EXTENDED | MAPI_NO_MAIL, &lpMapiSession);
		DEFINE_IF_HR_NT_OK_BREAK(hr)

		hr = SaveMsg2File();
		DEFINE_IF_HR_NT_OK_BREAK(hr);

	} while (0);

	DWORD dError = GetLastError();

	if (pAllFoldersPropValue){
		MAPIFREEBUFFER(pAllFoldersPropValue);
	}

	if (lpExchManageStroe)
	{
		lpExchManageStroe->Release();
		lpExchManageStroe = NULL;
	}

	if (lpMdb)
	{
		ULONG ulLogOffTag = LOGOFF_NO_WAIT;
		lpMdb->StoreLogoff(&ulLogOffTag);
		lpMdb->Release();
		lpMdb = NULL;
	}

	if (pRows)
	{
		FreeProws(pRows);
		pRows = NULL;
	}

	if (pIStoreTable)
	{
		pIStoreTable->Release();
		pIStoreTable = NULL;
	}

	if (lpMapiSession){
		lpMapiSession->Logoff(0, 0, 0);
		lpMapiSession->Release();
		lpMapiSession = NULL;
	}

	MAPIUninitialize();

	return hr;
}

HRESULT ModifyGlobalProfileSection(){
	HRESULT hRes = S_OK;   // HRESULT returned by this method
	LPPROFADMIN pAdminProfiles = NULL; // Pointer to IProfAdmin object
	LPSERVICEADMIN pSvcAdmin = NULL;  // Pointer to IServiceAdmin object
	LPPROFSECT pGlobalProfSect = NULL; // Pointer to IProfSect object
	LPSPropValue pProps = NULL; // Pointer to PropValue

	do{
		// Get a Profile admin object
		if (FAILED(hRes = MAPIAdminProfiles(0L, &pAdminProfiles)))
			break;

		// Get a ServiceAdmin object
		if (FAILED(hRes = pAdminProfiles->AdminServices(
			PROFILENAME,
			NULL,
			0L,  // Your app's window handle
			0L,
			&pSvcAdmin)))
			break;

		LPPROFSECT lpGlobalProfSect = NULL;
		hRes = SvcAdminOpenProfileSection(pSvcAdmin,
			(LPMAPIUID)&pbGlobalProfileSectionGuid, NULL, MAPI_MODIFY, &lpGlobalProfSect);

		enum { _PR_CODE_PAGE_ID, _PR_LOCALE_ID, _COUNT };
		SPropValue spCodePage[_COUNT] = { 0 };
		spCodePage[_PR_CODE_PAGE_ID].ulPropTag = PR_CODE_PAGE_ID;
		spCodePage[_PR_CODE_PAGE_ID].Value.l = CONVERT_CODE_PAGE;

		spCodePage[_PR_LOCALE_ID].ulPropTag = PR_LOCALE_ID;
		spCodePage[_PR_LOCALE_ID].Value.l = CONVERT_LOCALE_ID;
		//
		DEFINE_IF_HR_NT_OK_BREAK(hRes = lpGlobalProfSect->SetProps(_COUNT, spCodePage, NULL));
	} while (0);

	// Free all memory allocated by any MAPI calls

	if (NULL != pAdminProfiles)
		pAdminProfiles->Release();

	if (NULL != pSvcAdmin)
		pSvcAdmin->Release();

	if (NULL != pGlobalProfSect)
		pGlobalProfSect->Release();

	if (NULL != pProps)
		MAPIFreeBuffer(&pProps);

	pSvcAdmin = NULL;
	pGlobalProfSect = NULL;
	pProps = NULL;
	pAdminProfiles = NULL;

	// Return the HRESULT to the calling function
	return hRes;
}

STDMETHODIMP SvcAdminOpenProfileSection(LPSERVICEADMIN lpSvcAdmin,

	LPMAPIUID lpUID,

	LPCIID lpInterface,

	ULONG ulFlags,

	LPPROFSECT FAR * lppProfSect)

{

	HRESULT hRes = S_OK;



	// Note: We have to open the profile section with full access.

	// MAPI discriminates  who can modify profiles, especially

	// in certain sections.  The way to force access has changed in

	// different versions of Outlook. Therefore, there are two methods.  See KB article 822977

	// for more information.



	// First, let us try the easier method of passing the MAPI_FORCE_ACCESS flag

	// to OpenProfileSection.  This method is available only in Outlook 2003 and in later versions of Outlook.



	hRes = lpSvcAdmin->OpenProfileSection(lpUID,

		lpInterface,

		ulFlags | MAPI_FORCE_ACCESS,

		lppProfSect);

	if (FAILED(hRes))

	{

		// If this does not succeed, it may be because you are using an earlier version of Outlook.

		// In this case, use the sample code

		// from KB article 228736 for more information.  Note: This information was compiled from that sample.

		// 



		///////////////////////////////////////////////////////////////////

		//  MAPI will always return E_ACCESSDENIED

		// when we open a profile section on the service if we are a client.  The workaround

		// is to call into one of MAPI's internal functions that bypasses

		// the security check.  We build an interface to it, and then point to it from our

		// offset of 0x48.  USE THIS METHOD AT YOUR OWN RISK!  THIS METHOD IS NOT SUPPORTED!

		interface IOpenSectionHack : public IUnknown

		{

		public:

			virtual HRESULT STDMETHODCALLTYPE OpenSection(LPMAPIUID, ULONG, LPPROFSECT*) = 0;

		};

		IOpenSectionHack** ppProfile = (IOpenSectionHack**)((((BYTE*)lpSvcAdmin) + 0x48));

		// Now, we want to open the Services Profile Section and store that

		// interface with the Object

		hRes = (*ppProfile)->OpenSection(lpUID,

			ulFlags,

			lppProfSect);

	}

	return hRes;
}


HRESULT ConfigMsgService(){
	HRESULT hr = 0;
	LPPROFADMIN lpProfAdmin = NULL;
	LPSERVICEADMIN lpServiceAdmin = NULL;
	LPMAPITABLE lpMapiTable = NULL;
	SRestriction sres;                   // Restriction structure.
	SPropValue SvcProps;                 // Property structure for restriction.
	LPSRowSet  lpSvcRows = NULL;        // Rowset to hold results of table query.
	LPSTR szServer = MAILSERVER;
	LPSTR szMailbox = "administrator";
	//enum { _PR_PROFILE_UNRESOLVED_NAME, _PR_PROFILE_UNRESOLVED_SERVER, _PR_PROFILE_UI_STATE, _PR_PROFILE_CONFIG_FLAGS, _PR_PROFILE_CONNECT_FLAGS, _PR_COUNT };


	enum {
		_PR_PROFILE_UNRESOLVED_NAME, _PR_PROFILE_UNRESOLVED_SERVER,
		_PR_PROFILE_UI_STATE, _PR_PROFILE_CONFIG_FLAGS,
		_PR_PROFILE_CONNECT_FLAGS, _PR_CONVERSION_PROHIBITED,
		_PR_PROFILE_RPC_PROXY_SERVER_, _PR_PROFILE_RPC_PROXY_SERVER_FLAGS_,
		_PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_, _PR_PROFILE_AUTH_PACKAGE_, _PR_COUNT
	};
	SPropValue rgval[_PR_COUNT + 1] = { 0 };                // Property structure to hold values we want to set.

	enum { iSvcName, iSvcUID, cptaSvc };
	bool isCreatedProfile = false;
	SizedSPropTagArray(cptaSvc, sptCols) = { cptaSvc, PR_SERVICE_NAME, PR_SERVICE_UID };

	do{
		// if not profile, create profile.
		// else use exist profile

		DEFINE_IF_HR_NT_OK_BREAK(MAPIAdminProfiles(0, &lpProfAdmin));

		LPTSTR strProfileName = PROFILENAME;
		hr = lpProfAdmin->CreateProfile(strProfileName, NULL, NULL, 0);
		if (hr == MAPI_E_NO_ACCESS){
			// profile exist;
			break;
		}
		else if (hr == S_OK){
			isCreatedProfile = true;
			DEFINE_IF_HR_NT_OK_BREAK(lpProfAdmin->AdminServices(strProfileName, NULL, NULL, 0, &lpServiceAdmin));

			DEFINE_IF_HR_NT_OK_BREAK(lpServiceAdmin->CreateMsgService((LPTSTR)"MSEMS", NULL, 0, 0));
			// todo config MsgService.

			hr = lpServiceAdmin->GetMsgServiceTable(0, &lpMapiTable);
			DEFINE_IF_HR_NT_OK_BREAK(hr);

			hr = lpMapiTable->SetColumns((LPSPropTagArray)&sptCols, 0);
			if (S_OK != hr)
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

			if (S_OK != hr)
			{
				//TRACE_TRACE(GRTUT, CA_LOG_ERR, L" Failed to IServiceTable->FindRow(%x) ... ", hr);
				break;
			}

			hr = lpMapiTable->QueryRows(1, 0, &lpSvcRows);
			if (hr || lpSvcRows == 0 || (PROP_TYPE(lpSvcRows->aRow[0].lpProps[iSvcUID].ulPropTag) == PT_ERROR)
				|| ((MAPIUID*)lpSvcRows->aRow[0].lpProps[iSvcUID].Value.bin.lpb == 0))
			{
				//TRACE_TRACE(GRTUT, CA_LOG_ERR, L" Failed to IServiceTable->QueryRows(%x) ... ", hr);
				break;
			}


			//sres.rt = RES_CONTENT;
			//sres.res.resContent.ulFuzzyLevel = FL_FULLSTRING;
			//sres.res.resContent.ulPropTag = PR_SERVICE_NAME_A;
			//sres.res.resContent.lpProp = &SvcProps;

			//SvcProps.ulPropTag = PR_SERVICE_NAME_A;
			//SvcProps.Value.lpszA = "MSEMS";

			//// Query the table to obtain the entry for the newly created message service.

			//if (FAILED(hr = HrQueryAllRows(lpMapiTable,
			//	(LPSPropTagArray)&sptCols,
			//	&sres,
			//	NULL,
			//	0,
			//	&lpSvcRows)))
			//{
			//	break;
			//}

			// Set up a SPropValue array for the properties that you have to configure.

			// First, the server name.
			ZeroMemory(&rgval[_PR_PROFILE_UNRESOLVED_SERVER], sizeof(SPropValue));
			rgval[_PR_PROFILE_UNRESOLVED_SERVER].ulPropTag = PR_PROFILE_UNRESOLVED_SERVER;
			rgval[_PR_PROFILE_UNRESOLVED_SERVER].Value.lpszA = szServer;

			// Next, the mailbox name.
			ZeroMemory(&rgval[_PR_PROFILE_UNRESOLVED_NAME], sizeof(SPropValue));
			rgval[_PR_PROFILE_UNRESOLVED_NAME].ulPropTag = PR_PROFILE_UNRESOLVED_NAME;
			rgval[_PR_PROFILE_UNRESOLVED_NAME].Value.lpszA = szMailbox;

			ZeroMemory(&rgval[_PR_PROFILE_UI_STATE], sizeof(SPropValue));
			rgval[_PR_PROFILE_UI_STATE].ulPropTag = PR_PROFILE_UI_STATE;
			rgval[_PR_PROFILE_UI_STATE].Value.l = 0x100;

			ZeroMemory(&rgval[_PR_PROFILE_CONFIG_FLAGS], sizeof(SPropValue));
			rgval[_PR_PROFILE_CONFIG_FLAGS].ulPropTag = PR_PROFILE_CONFIG_FLAGS;
			rgval[_PR_PROFILE_CONFIG_FLAGS].Value.l = CONFIG_SERVICE;

			ZeroMemory(&rgval[_PR_PROFILE_CONNECT_FLAGS], sizeof(SPropValue));
			rgval[_PR_PROFILE_CONNECT_FLAGS].ulPropTag = PR_PROFILE_CONNECT_FLAGS;
			rgval[_PR_PROFILE_CONNECT_FLAGS].Value.l = 0x8024; //CONNECT_USE_ADMIN_PRIVILEGE(0x01) CONNECT_NO_NOTIFICATIONS(0x20)|CONNECT_USE_SEPARATE_CONNECTION (0x04) | CONNECT_IGNORE_NO_PF (0x8000)

			ZeroMemory(&rgval[_PR_CONVERSION_PROHIBITED], sizeof(SPropValue));
			rgval[_PR_CONVERSION_PROHIBITED].ulPropTag = PR_CONVERSION_PROHIBITED;
			rgval[_PR_CONVERSION_PROHIBITED].Value.b = 1;

			ZeroMemory(&rgval[_PR_PROFILE_RPC_PROXY_SERVER_], sizeof(SPropValue));
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_;
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_].Value.lpszA = MAILSERVER;



			ZeroMemory(&rgval[_PR_PROFILE_RPC_PROXY_SERVER_FLAGS_], sizeof(SPropValue));
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_FLAGS_].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_FLAGS_;
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_FLAGS_].Value.l = PRXF_ENABLED_;

			ZeroMemory(&rgval[_PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_], sizeof(SPropValue));
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_].ulPropTag = PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_;
			rgval[_PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_].Value.l = 0x02;

			ZeroMemory(&rgval[_PR_PROFILE_AUTH_PACKAGE_], sizeof(SPropValue));
			rgval[_PR_PROFILE_AUTH_PACKAGE_].ulPropTag = PR_PROFILE_AUTH_PACKAGE_;
			rgval[_PR_PROFILE_AUTH_PACKAGE_].Value.l = 0x0A;

			// Configure the message service by using the previous properties.

			if (FAILED(hr = lpServiceAdmin->ConfigureMsgService(
				(LPMAPIUID)lpSvcRows->aRow->lpProps[iSvcUID].Value.bin.lpb, // Entry ID of service to configure.
				NULL,                                                       // Handle to parent window.
				0,                                                          // Flags.
				_PR_COUNT,                                                          // Number of properties we are setting.
				rgval)))                                                    // Pointer to SPropValue array.
			{
				break;
			}

		}
		else {
			break;
		}
	} while (0);


	if (FAILED(hr)){
		if (isCreatedProfile)
			DeleteProfile();
	}

	if (lpSvcRows != NULL){
		FreeProws(lpSvcRows);
		lpSvcRows = NULL;
	}

	if (lpMapiTable != NULL){
		lpMapiTable->Release();
		lpMapiTable = NULL;
	}

	if (lpServiceAdmin != NULL){
		lpServiceAdmin->Release();
		lpServiceAdmin = NULL;
	}

	if (lpProfAdmin != NULL){
		lpProfAdmin->Release();
		lpProfAdmin = NULL;
	}

	return hr;
}

LONG DeleteProfile()
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

		hr = pProfAdmin->DeleteProfile(PROFILENAME, 0); // unicode attention 
		if (hr)
		{
			break;
		}

	} while (0);

	if (pProfAdmin)
	{
		pProfAdmin->Release();
	}

	return	hr;
}

HRESULT SaveMsg2File(){
	ULONG cbStrSize = 0L;
	LPWSTR lpWideCharStr = NULL;
	wchar_t szPath[_MAX_PATH];
	// get temp file directory
	GetTempPath(_MAX_PATH, szPath);
#ifdef MAPI32_W
	wcscat_s(szPath, L"Correct.msg");
#else 
	wcscat_s(szPath, L"Garbage.msg");
#endif
	IStorage *pStorage = NULL;
	LPMSGSESS pMsgSession = NULL;
	LPMESSAGE pMessage = NULL;
	HRESULT hr = S_OK;

	LPWSTR subject = L"ceshi测试12";
	LPWSTR body = L"lhy测试12";
	LPWSTR receipt = L"linghaiyang@lhytest.com";

	LPSTR subjectA = ConvertUnicode2Ansi(subject);
	LPSTR bodyA = ConvertUnicode2Ansi(body);
	LPSTR receiptA = ConvertUnicode2Ansi(receipt);

	do{
		LPMALLOC pMalloc = MAPIGetDefaultMalloc();

		hr = StgCreateDocfile(szPath, STGM_READWRITE | STGM_TRANSACTED | STGM_CREATE, 0, &pStorage);
		DEFINE_IF_HR_NT_OK_BREAK(hr);


		hr = OpenIMsgSession(pMalloc, 0, &pMsgSession);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

#ifdef MAPI32_W
		// lhy comment:if load exmapi32.dll, this function will failed with error code 0x80040106.
		hr = OpenIMsgOnIStg(pMsgSession, MAPIAllocateBuffer, MAPIAllocateMore, MAPIFreeBuffer, pMalloc, NULL, pStorage, NULL, 0, MAPI_UNICODE, &pMessage);
#else
		hr = OpenIMsgOnIStg(pMsgSession, MAPIAllocateBuffer, MAPIAllocateMore, MAPIFreeBuffer, pMalloc, NULL, pStorage, NULL, 0, 0, &pMessage);
#endif
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		hr = WriteClassStg(pStorage, CLSID_MailMessage);
		DEFINE_IF_HR_NT_OK_BREAK(hr);
#ifdef MAPI32_W
		hr = SetPropsW(pMessage, subject, body, receipt, false, false, false, false, FORCE_SAVE);
#else 

		hr = SetPropsA(pMessage, subjectA, bodyA, receiptA, false, false, false, false, FORCE_SAVE);
#endif
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		hr = pStorage->Commit(STGC_DEFAULT);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

	} while (0);

	delete subjectA;
	delete bodyA;
	delete receiptA;
	if (pMessage){
		pMessage->Release();
		pMessage = NULL;
	}
	if (pStorage){
		pStorage->Release();
		pStorage = NULL;
	}
	if (pMsgSession){
		CloseIMsgSession(pMsgSession);
		pMsgSession = NULL;
	}
	return hr;
}

LPSTR ConvertUnicode2Ansi(LPWSTR unicodeStr){
	bool isUsedDefaultChar;
	int cchWLen = wcslen(unicodeStr);

	int cbLen = WideCharToMultiByte(CONVERT_CODE_PAGE, 0, unicodeStr, -1, NULL, 0, NULL, NULL);
	char* buffer = new char[cbLen + 1];
	memset((void *)buffer, 0, sizeof(char) * (cbLen + 1));
	WideCharToMultiByte(CONVERT_CODE_PAGE, 0, unicodeStr, cchWLen, buffer, cbLen, NULL, NULL);
	return buffer;
}

// The enum is to aid in building the property values for SetProps.
enum {
	p_PR_MESSAGE_CLASS_W,
	p_PR_ICON_INDEX,
	p_PR_SUBJECT_W,
	p_PR_CONVERSATION_TOPIC_W,
	p_PR_BODY_W,
	p_PR_IMPORTANCE,
	p_PR_READ_RECEIPT_REQUESTED,
	p_PR_MESSAGE_FLAGS,
	p_PR_MSG_EDITOR_FORMAT,
	p_PR_MESSAGE_LOCALE_ID,
	p_PR_INETMAIL_OVERRIDE_FORMAT,
	p_PR_DELETE_AFTER_SUBMIT,
	p_PR_INTERNET_CPID,
	p_PR_CONVERSATION_INDEX,
	p_PR_CODE_PAGE_ID,						// Code Page
	NUM_PROPS
};

HRESULT SetPropsW(
	LPMESSAGE lpMessage,
	LPWSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPWSTR szBody, // PR_BODY_W
	LPWSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit,
	ULONG msgSaveFlag,
	LPMAPISESSION lpMAPISession) {
	SPropValue spvProps[NUM_PROPS] = { 0 };
	spvProps[p_PR_MESSAGE_CLASS_W].ulPropTag = PR_MESSAGE_CLASS_W;
	spvProps[p_PR_ICON_INDEX].ulPropTag = PR_ICON_INDEX;
	spvProps[p_PR_SUBJECT_W].ulPropTag = PR_SUBJECT_W;
	spvProps[p_PR_CONVERSATION_TOPIC_W].ulPropTag = PR_CONVERSATION_TOPIC_W;
	spvProps[p_PR_BODY_W].ulPropTag = PR_BODY_W;
	spvProps[p_PR_IMPORTANCE].ulPropTag = PR_IMPORTANCE;
	spvProps[p_PR_READ_RECEIPT_REQUESTED].ulPropTag = PR_READ_RECEIPT_REQUESTED;
	spvProps[p_PR_MESSAGE_FLAGS].ulPropTag = PR_MESSAGE_FLAGS;
	spvProps[p_PR_MSG_EDITOR_FORMAT].ulPropTag = PR_MSG_EDITOR_FORMAT;
	spvProps[p_PR_MESSAGE_LOCALE_ID].ulPropTag = PR_MESSAGE_LOCALE_ID;
	spvProps[p_PR_INETMAIL_OVERRIDE_FORMAT].ulPropTag = PR_INETMAIL_OVERRIDE_FORMAT;
	spvProps[p_PR_DELETE_AFTER_SUBMIT].ulPropTag = PR_DELETE_AFTER_SUBMIT;
	spvProps[p_PR_INTERNET_CPID].ulPropTag = PR_INTERNET_CPID;
	spvProps[p_PR_CONVERSATION_INDEX].ulPropTag = PR_CONVERSATION_INDEX;


	spvProps[p_PR_MESSAGE_CLASS_W].Value.lpszW = L"IPM.Note";
	spvProps[p_PR_ICON_INDEX].Value.l = 0x103; // Unsent Mail
	spvProps[p_PR_SUBJECT_W].Value.lpszW = szSubject;
	spvProps[p_PR_CONVERSATION_TOPIC_W].Value.lpszW = szSubject;
	spvProps[p_PR_BODY_W].Value.lpszW = szBody;
	spvProps[p_PR_IMPORTANCE].Value.l = bHighImportance ? IMPORTANCE_HIGH : IMPORTANCE_NORMAL;
	spvProps[p_PR_READ_RECEIPT_REQUESTED].Value.b = bReadReceipt ? true : false;
	spvProps[p_PR_MESSAGE_FLAGS].Value.l = MSGFLAG_UNSENT;
	spvProps[p_PR_MSG_EDITOR_FORMAT].Value.l = EDITOR_FORMAT_PLAINTEXT;
	spvProps[p_PR_MESSAGE_LOCALE_ID].Value.l = 1033; // (en-us)
	spvProps[p_PR_INETMAIL_OVERRIDE_FORMAT].Value.l = NULL; // Mail system chooses default encoding scheme
	spvProps[p_PR_DELETE_AFTER_SUBMIT].Value.b = bDeleteAfterSubmit ? true : false;
	spvProps[p_PR_INTERNET_CPID].Value.l = cpidASCII;

	HRESULT hRes = BuildConversationIndex(
		&spvProps[p_PR_CONVERSATION_INDEX].Value.bin.cb,
		&spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb);

	if (SUCCEEDED(hRes))
	{
		hRes = lpMessage->SetProps(NUM_PROPS, spvProps, NULL);
		if (SUCCEEDED(hRes))
		{
			if (lpMAPISession)
				hRes = AddRecipientW(lpMAPISession,
				lpMessage,
				MAPI_TO,
				szRecipientName);
			else
				hRes = AddRecipientW(lpMessage,
				MAPI_TO,
				szRecipientName);

			if (SUCCEEDED(hRes))
			{
				if (bReadReceipt)
				{
					hRes = AddReportTag(lpMessage);
				}

				if (SUCCEEDED(hRes))
				{
					hRes = lpMessage->SaveChanges(msgSaveFlag);
					if (SUCCEEDED(hRes) && bSubmit)
					{
						hRes = lpMessage->SubmitMessage(NULL);
					}
				}
			}
		}
	}
	if (spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb)
		delete[] spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb;

	return hRes;
}

HRESULT SetPropsA(
	LPMESSAGE lpMessage,
	LPSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPSTR szBody, // PR_BODY_W
	LPSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit,
	ULONG msgSaveFlag,
	LPMAPISESSION lpMAPISession) {
	SPropValue spvProps[NUM_PROPS] = { 0 };
	spvProps[p_PR_MESSAGE_CLASS_W].ulPropTag = PR_MESSAGE_CLASS_A;
	spvProps[p_PR_ICON_INDEX].ulPropTag = PR_ICON_INDEX;
	spvProps[p_PR_SUBJECT_W].ulPropTag = PR_SUBJECT_A;
	spvProps[p_PR_CONVERSATION_TOPIC_W].ulPropTag = PR_CONVERSATION_TOPIC_A;
	spvProps[p_PR_BODY_W].ulPropTag = PR_BODY_A;
	spvProps[p_PR_IMPORTANCE].ulPropTag = PR_IMPORTANCE;
	spvProps[p_PR_READ_RECEIPT_REQUESTED].ulPropTag = PR_READ_RECEIPT_REQUESTED;
	spvProps[p_PR_MESSAGE_FLAGS].ulPropTag = PR_MESSAGE_FLAGS;
	spvProps[p_PR_MSG_EDITOR_FORMAT].ulPropTag = PR_MSG_EDITOR_FORMAT;
	spvProps[p_PR_MESSAGE_LOCALE_ID].ulPropTag = PR_MESSAGE_LOCALE_ID;
	spvProps[p_PR_INETMAIL_OVERRIDE_FORMAT].ulPropTag = PR_INETMAIL_OVERRIDE_FORMAT;
	spvProps[p_PR_DELETE_AFTER_SUBMIT].ulPropTag = PR_DELETE_AFTER_SUBMIT;
	spvProps[p_PR_INTERNET_CPID].ulPropTag = PR_INTERNET_CPID;
	spvProps[p_PR_CONVERSATION_INDEX].ulPropTag = PR_CONVERSATION_INDEX;
	spvProps[p_PR_CODE_PAGE_ID].ulPropTag = PR_CODE_PAGE_ID;


	spvProps[p_PR_MESSAGE_CLASS_W].Value.lpszA = "IPM.Note";
	spvProps[p_PR_ICON_INDEX].Value.l = 0x103; // Unsent Mail
	spvProps[p_PR_SUBJECT_W].Value.lpszA = szSubject;
	spvProps[p_PR_CONVERSATION_TOPIC_W].Value.lpszA = szSubject;
	spvProps[p_PR_BODY_W].Value.lpszA = szBody;
	spvProps[p_PR_IMPORTANCE].Value.l = bHighImportance ? IMPORTANCE_HIGH : IMPORTANCE_NORMAL;
	spvProps[p_PR_READ_RECEIPT_REQUESTED].Value.b = bReadReceipt ? true : false;
	spvProps[p_PR_MESSAGE_FLAGS].Value.l = MSGFLAG_UNSENT;
	spvProps[p_PR_MSG_EDITOR_FORMAT].Value.l = EDITOR_FORMAT_PLAINTEXT;
	spvProps[p_PR_MESSAGE_LOCALE_ID].Value.l = CONVERT_LOCALE_ID;//2052; // (en-us)
	spvProps[p_PR_INETMAIL_OVERRIDE_FORMAT].Value.l = NULL; // Mail system chooses default encoding scheme
	spvProps[p_PR_DELETE_AFTER_SUBMIT].Value.b = bDeleteAfterSubmit ? true : false;
	spvProps[p_PR_INTERNET_CPID].Value.l = CONVERT_CODE_PAGE;// 936;
	spvProps[p_PR_CODE_PAGE_ID].Value.l = CONVERT_CODE_PAGE;

	HRESULT hRes = BuildConversationIndex(
		&spvProps[p_PR_CONVERSATION_INDEX].Value.bin.cb,
		&spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb);

	if (SUCCEEDED(hRes))
	{
		hRes = lpMessage->SetProps(NUM_PROPS, spvProps, NULL);
		if (SUCCEEDED(hRes))
		{
			if (lpMAPISession)
				hRes = AddRecipientA(lpMAPISession,
				lpMessage,
				MAPI_TO,
				szRecipientName);
			else
				hRes = AddRecipientA(lpMessage,
				MAPI_TO,
				szRecipientName);

			if (SUCCEEDED(hRes))
			{
				if (bReadReceipt)
				{
					hRes = AddReportTag(lpMessage);
				}

				if (SUCCEEDED(hRes))
				{
					hRes = lpMessage->SaveChanges(msgSaveFlag);
					if (SUCCEEDED(hRes) && bSubmit)
					{
						hRes = lpMessage->SubmitMessage(NULL);
					}
				}
			}
		}
	}
	if (spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb)
		delete[] spvProps[p_PR_CONVERSATION_INDEX].Value.bin.lpb;

	return hRes;
}

HRESULT AddMailA(LPMAPISESSION lpMAPISession,
	LPMAPIFOLDER lpFolder,
	LPSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPSTR szBody, // PR_BODY_W
	LPSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit)
{
	if (!lpFolder) return MAPI_E_INVALID_PARAMETER;
	HRESULT hRes = S_OK;
	LPMESSAGE lpMessage = 0;

	// Create a message and set its properties
	hRes = lpFolder->CreateMessage(0,
		0,
		&lpMessage);
	if (SUCCEEDED(hRes))
	{
		// Since we know in advance which props we'll be setting, we can statically declare most of the structures involved and save expensive MAPIAllocateBuffer calls
		hRes = SetPropsA(lpMessage, szSubject, szBody, szRecipientName, bHighImportance, bReadReceipt, bSubmit, bDeleteAfterSubmit, KEEP_OPEN_READWRITE, lpMAPISession);
	}
	if (lpMessage) {
		lpMessage->Release();
		lpMessage = NULL;
	}
	return hRes;
}

HRESULT AddMailW(LPMAPISESSION lpMAPISession,
	LPMAPIFOLDER lpFolder,
	LPWSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPWSTR szBody, // PR_BODY_W
	LPWSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit)
{
	if (!lpFolder) return MAPI_E_INVALID_PARAMETER;
	HRESULT hRes = S_OK;
	LPMESSAGE lpMessage = 0;

	// Create a message and set its properties
	hRes = lpFolder->CreateMessage(0,
		0,
		&lpMessage);
	if (SUCCEEDED(hRes))
	{
		// Since we know in advance which props we'll be setting, we can statically declare most of the structures involved and save expensive MAPIAllocateBuffer calls
		hRes = SetPropsW(lpMessage, szSubject, szBody, szRecipientName, bHighImportance, bReadReceipt, bSubmit, bDeleteAfterSubmit, KEEP_OPEN_READWRITE, lpMAPISession);
	}
	if (lpMessage) {
		lpMessage->Release();
		lpMessage = NULL;
	}
	return hRes;
}

HRESULT HrAllocAdrList(ULONG ulNumProps, LPADRLIST* lpAdrList)
{
	if (!lpAdrList || ulNumProps > ULONG_MAX / sizeof(SPropValue)) return MAPI_E_INVALID_PARAMETER;
	HRESULT hRes = S_OK;
	LPADRLIST lpLocalAdrList = NULL;

	*lpAdrList = NULL;

	// Allocate memory for new SRowSet structure.
	hRes = MAPIAllocateBuffer(CbNewSRowSet(1), (LPVOID*)&lpLocalAdrList);

	if (SUCCEEDED(hRes) && lpLocalAdrList)
	{
		// Zero out allocated memory.
		ZeroMemory(lpLocalAdrList, CbNewSRowSet(1));

		// Allocate memory for SPropValue structure that indicates what
		// recipient properties will be set.
		hRes = MAPIAllocateBuffer(
			ulNumProps * sizeof(SPropValue),
			(LPVOID*)&lpLocalAdrList->aEntries[0].rgPropVals);

		if (SUCCEEDED(hRes) && lpLocalAdrList->aEntries[0].rgPropVals)
		{
			// Zero out allocated memory.
			ZeroMemory(lpLocalAdrList->aEntries[0].rgPropVals, ulNumProps * sizeof(SPropValue));
			*lpAdrList = lpLocalAdrList;
		}
		else
		{
			MAPIFreeBuffer(lpLocalAdrList);
		}
	}

	return hRes;
}

// The enum is to aid in building the property values for ResolveName/ModifyRecipients
enum {
	p_PR_DISPLAY_NAME_W,
	p_PR_RECIPIENT_TYPE,
	NUM_RECIP_PROPS
};

HRESULT AddRecipientW(LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPWSTR szRecipientName){
	HRESULT			hRes = S_OK;
	LPADRLIST		lpAdrList = NULL;  // ModifyRecips takes LPADRLIST
	LPADRBOOK		lpAddrBook = NULL;

	
	hRes = HrAllocAdrList(NUM_RECIP_PROPS, &lpAdrList);
	if (SUCCEEDED(hRes) && lpAdrList)
	{
		// Set up the recipient by indicating how many recipients
		// and how many properties will be set on each recipient.
		lpAdrList->cEntries = 1;	// How many recipients.
		lpAdrList->aEntries[0].cValues = NUM_RECIP_PROPS;  // How many properties per recipient

		lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].ulPropTag = PR_DISPLAY_NAME_W;
		lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].ulPropTag = PR_RECIPIENT_TYPE;

		lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].Value.lpszW = szRecipientName;
		lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].Value.l = ulRecipientType;

		// If everything goes right, add the new recipient to the message
		// object passed into us.
		hRes = lpMessage->ModifyRecipients(MODRECIP_ADD, lpAdrList);
		
	}
	
	if (lpAdrList) FreePadrlist(lpAdrList);
	if (lpAddrBook) lpAddrBook->Release();
	return hRes;
}

HRESULT AddRecipientW(LPMAPISESSION lpMAPISession,
	LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPWSTR szRecipientName)
{
	HRESULT			hRes = S_OK;
	LPADRLIST		lpAdrList = NULL;  // ModifyRecips takes LPADRLIST
	LPADRBOOK		lpAddrBook = NULL;

	if (!lpMessage || !lpMAPISession) return MAPI_E_INVALID_PARAMETER;

	hRes = lpMAPISession->OpenAddressBook(
		NULL,
		NULL,
		NULL,
		&lpAddrBook);
	if (SUCCEEDED(hRes) && lpAddrBook)
	{
		hRes = HrAllocAdrList(NUM_RECIP_PROPS, &lpAdrList);
		if (SUCCEEDED(hRes) && lpAdrList)
		{
			// Set up the recipient by indicating how many recipients
			// and how many properties will be set on each recipient.
			lpAdrList->cEntries = 1;	// How many recipients.
			lpAdrList->aEntries[0].cValues = NUM_RECIP_PROPS;  // How many properties per recipient

			lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].ulPropTag = PR_DISPLAY_NAME_W;
			lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].ulPropTag = PR_RECIPIENT_TYPE;

			lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].Value.lpszW = szRecipientName;
			lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].Value.l = ulRecipientType;

			hRes = lpAddrBook->ResolveName(
				0L,
				MAPI_UNICODE,
				NULL,
				lpAdrList);
			if (SUCCEEDED(hRes))
			{
				// If everything goes right, add the new recipient to the message
				// object passed into us.
				hRes = lpMessage->ModifyRecipients(MODRECIP_ADD, lpAdrList);
			}
		}
	}
	if (lpAdrList) FreePadrlist(lpAdrList);
	if (lpAddrBook) lpAddrBook->Release();
	return hRes;
}


HRESULT AddRecipientA(LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPSTR szRecipientName){
	HRESULT			hRes = S_OK;
	LPADRLIST		lpAdrList = NULL;  // ModifyRecips takes LPADRLIST
	LPADRBOOK		lpAddrBook = NULL;


	hRes = HrAllocAdrList(NUM_RECIP_PROPS, &lpAdrList);
	if (SUCCEEDED(hRes) && lpAdrList)
	{
		// Set up the recipient by indicating how many recipients
		// and how many properties will be set on each recipient.
		lpAdrList->cEntries = 1;	// How many recipients.
		lpAdrList->aEntries[0].cValues = NUM_RECIP_PROPS;  // How many properties per recipient

		lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].ulPropTag = PR_DISPLAY_NAME_A;
		lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].ulPropTag = PR_RECIPIENT_TYPE;

		lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].Value.lpszA = szRecipientName;
		lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].Value.l = ulRecipientType;

		// If everything goes right, add the new recipient to the message
		// object passed into us.
		hRes = lpMessage->ModifyRecipients(MODRECIP_ADD, lpAdrList);

	}

	if (lpAdrList) FreePadrlist(lpAdrList);
	if (lpAddrBook) lpAddrBook->Release();
	return hRes;
}

HRESULT AddRecipientA(LPMAPISESSION lpMAPISession,
	LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPSTR szRecipientName)
{
	HRESULT			hRes = S_OK;
	LPADRLIST		lpAdrList = NULL;  // ModifyRecips takes LPADRLIST
	LPADRBOOK		lpAddrBook = NULL;

	if (!lpMessage || !lpMAPISession) return MAPI_E_INVALID_PARAMETER;

	hRes = lpMAPISession->OpenAddressBook(
		NULL,
		NULL,
		NULL,
		&lpAddrBook);
	if (SUCCEEDED(hRes) && lpAddrBook)
	{
		hRes = HrAllocAdrList(NUM_RECIP_PROPS, &lpAdrList);
		if (SUCCEEDED(hRes) && lpAdrList)
		{
			// Set up the recipient by indicating how many recipients
			// and how many properties will be set on each recipient.
			lpAdrList->cEntries = 1;	// How many recipients.
			lpAdrList->aEntries[0].cValues = NUM_RECIP_PROPS;  // How many properties per recipient

			lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].ulPropTag = PR_DISPLAY_NAME_A;
			lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].ulPropTag = PR_RECIPIENT_TYPE;

			lpAdrList->aEntries[0].rgPropVals[p_PR_DISPLAY_NAME_W].Value.lpszA = szRecipientName;
			lpAdrList->aEntries[0].rgPropVals[p_PR_RECIPIENT_TYPE].Value.l = ulRecipientType;

			hRes = lpAddrBook->ResolveName(
				0L,
				MAPI_UNICODE,
				NULL,
				lpAdrList);
			if (SUCCEEDED(hRes))
			{
				// If everything goes right, add the new recipient to the message
				// object passed into us.
				hRes = lpMessage->ModifyRecipients(MODRECIP_ADD, lpAdrList);
			}
		}
	}
	if (lpAdrList) FreePadrlist(lpAdrList);
	if (lpAddrBook) lpAddrBook->Release();
	return hRes;
}

// Build ConversationIndex - does basically what ScCreateConversationIndex does when passed a NULL parent
// Allocates with new, free with delete[]
HRESULT BuildConversationIndex(ULONG* lpcbConversationIndex,
	LPBYTE* lppConversationIndex)
{
	if (!lpcbConversationIndex || !lppConversationIndex) return MAPI_E_INVALID_PARAMETER;

	HRESULT hRes = S_OK;

	*lpcbConversationIndex = NULL;
	*lppConversationIndex = NULL;

	// Calculate how large our struct will be
	size_t cbStruct = sizeof(BYTE) + // UnnamedByte
		5 * sizeof(BYTE) + // ftCurrent
		sizeof(GUID); // guid

	// Allocate our buffer
	LPBYTE lpConversationIndex = new BYTE[cbStruct];

	// Populate it
	if (lpConversationIndex)
	{
		memset(lpConversationIndex, 0, cbStruct);
		lpConversationIndex[0] = 0x1;

		SYSTEMTIME st = { 0 };
		FILETIME ft = { 0 };
		GUID guid = { 0 };
		GetSystemTime(&st);
		SystemTimeToFileTime(&st, &ft);

		lpConversationIndex[1] = (BYTE)((ft.dwHighDateTime & 0x00FF0000) >> 16);
		lpConversationIndex[2] = (BYTE)((ft.dwHighDateTime & 0x0000FF00) >> 8);
		lpConversationIndex[3] = (BYTE)(ft.dwHighDateTime & 0x000000FF);
		lpConversationIndex[4] = (BYTE)((ft.dwLowDateTime & 0xFF000000) >> 24);
		lpConversationIndex[5] = (BYTE)((ft.dwLowDateTime & 0x00FF0000) >> 16);
		hRes = CoCreateGuid(&guid);
		if (SUCCEEDED(hRes))
		{
			lpConversationIndex[6] = (BYTE)((guid.Data1 & 0xFF000000) >> 24);
			lpConversationIndex[7] = (BYTE)((guid.Data1 & 0x00FF0000) >> 16);
			lpConversationIndex[8] = (BYTE)((guid.Data1 & 0x0000FF00) >> 8);
			lpConversationIndex[9] = (BYTE)((guid.Data1 & 0x000000FF));
			lpConversationIndex[10] = (BYTE)((guid.Data2 & 0xFF00) >> 8);
			lpConversationIndex[11] = (BYTE)((guid.Data2 & 0x00FF));
			lpConversationIndex[12] = (BYTE)((guid.Data3 & 0xFF00) >> 8);
			lpConversationIndex[13] = (BYTE)((guid.Data3 & 0x00FF));
			memcpy(&lpConversationIndex[14], &guid.Data4, 8);
			// Return it
			*lpcbConversationIndex = cbStruct;
			*lppConversationIndex = lpConversationIndex;
			return S_OK;
		}
	}

	// We can only get here if we failed
	return FAILED(hRes) ? hRes : MAPI_E_CALL_FAILED;
}

// Allocates with new, free with delete[]
HRESULT BuildReportTag(ULONG cbStoreEntryID,
	LPBYTE lpStoreEntryID,
	ULONG cbFolderEntryID,
	LPBYTE lpFolderEntryID,
	ULONG cbMessageEntryID,
	LPBYTE lpMessageEntryID,
	ULONG cbSearchFolderEntryID,
	LPBYTE lpSearchFolderEntryID,
	ULONG cbMessageSearchKey, // MUST not be NULL
	LPBYTE lpMessageSearchKey, // MUST not be NULL
	LPSTR lpszAnsiText,
	ULONG* lpcbReportTag,
	LPBYTE* lppReportTag)
{
	if (!lpcbReportTag || !lppReportTag || !cbMessageSearchKey || !lpMessageSearchKey) return MAPI_E_INVALID_PARAMETER;

	*lpcbReportTag = NULL;
	*lppReportTag = NULL;

	size_t cchAnsiText = NULL;
	if (lpszAnsiText) cchAnsiText = strlen(lpszAnsiText) + 1; // Count the NULL terminator

	// Calculate how large our struct will be
	size_t cbStruct = 9 * sizeof(CHAR) + // Cookie
		sizeof(DWORD) + // Version
		sizeof(DWORD) + // cbStoreEntryID
		cbStoreEntryID + // lpStoreEntryID
		sizeof(DWORD) + // cbFolderEntryID
		cbFolderEntryID + // lpFolderEntryID
		sizeof(DWORD) + // cbMessageEntryID
		cbMessageEntryID + // lpMessageEntryID
		sizeof(DWORD) + // cbSearchFolderEntryID
		cbSearchFolderEntryID + // lpSearchFolderEntryID
		sizeof(DWORD) + // cbMessageSearchKey
		cbMessageSearchKey + // lpMessageSearchKey
		sizeof(DWORD) + // cchAnsiText
		cchAnsiText; // lpszAnsiText

	// Allocate our buffer
	LPBYTE lpReportTag = new BYTE[cbStruct];

	// Populate it
	if (lpReportTag)
	{
		memset(lpReportTag, 0, cbStruct);

		LPBYTE pb = lpReportTag;
		// this will copy the string and the NULL terminator together
		memcpy(pb, "PCDFEB09", 9);
		pb += 9;
		*(WORD*)pb = 0x0001;
		pb += sizeof(WORD);
		*(WORD*)pb = 0x0002;
		pb += sizeof(WORD);
		*((DWORD*)pb) = cbStoreEntryID;
		pb += sizeof(DWORD);
		if (cbStoreEntryID)
		{
			memcpy(pb, lpStoreEntryID, cbStoreEntryID);
			pb += cbStoreEntryID;
		}
		*((DWORD*)pb) = cbFolderEntryID;
		pb += sizeof(DWORD);
		if (cbFolderEntryID)
		{
			memcpy(pb, lpFolderEntryID, cbFolderEntryID);
			pb += cbFolderEntryID;
		}
		*((DWORD*)pb) = cbMessageEntryID;
		pb += sizeof(DWORD);
		if (cbMessageEntryID)
		{
			memcpy(pb, lpMessageEntryID, cbMessageEntryID);
			pb += cbMessageEntryID;
		}
		*((DWORD*)pb) = cbSearchFolderEntryID;
		pb += sizeof(DWORD);
		if (cbSearchFolderEntryID)
		{
			memcpy(pb, lpSearchFolderEntryID, cbSearchFolderEntryID);
			pb += cbSearchFolderEntryID;
		}
		*((DWORD*)pb) = cbMessageSearchKey;
		pb += sizeof(DWORD);
		if (cbMessageSearchKey)
		{
			memcpy(pb, lpMessageSearchKey, cbMessageSearchKey);
			pb += cbMessageSearchKey;
		}
		*((DWORD*)pb) = cchAnsiText;
		pb += sizeof(DWORD);
		if (cchAnsiText)
		{
			memcpy(pb, lpszAnsiText, cchAnsiText * sizeof(CHAR));
			pb += cchAnsiText * sizeof(CHAR);
		}

		// Return it
		*lpcbReportTag = cbStruct;
		*lppReportTag = lpReportTag;
		return S_OK;
	}

	return MAPI_E_CALL_FAILED;
}


HRESULT AddReportTag(LPMESSAGE lpMessage)
{
	if (!lpMessage) return MAPI_E_INVALID_PARAMETER;
	HRESULT hRes = S_OK;
	ULONG cValues = 0;
	LPSPropValue lpPropArray = NULL;

	SizedSPropTagArray(2, sptaProps) = { 2, { PR_PARENT_ENTRYID, PR_SEARCH_KEY } };

	hRes = lpMessage->GetProps((LPSPropTagArray)&sptaProps, 0, &cValues, &lpPropArray);
	if (SUCCEEDED(hRes))
	{
		SPropValue sProp = { 0 };
		sProp.ulPropTag = PR_REPORT_TAG;
		hRes = BuildReportTag(NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			NULL,
			(lpPropArray[0].ulPropTag == PR_PARENT_ENTRYID) ? lpPropArray[0].Value.bin.cb : 0,
			(lpPropArray[0].ulPropTag == PR_PARENT_ENTRYID) ? lpPropArray[0].Value.bin.lpb : 0,
			(lpPropArray[1].ulPropTag == PR_SEARCH_KEY) ? lpPropArray[1].Value.bin.cb : 0,
			(lpPropArray[1].ulPropTag == PR_SEARCH_KEY) ? lpPropArray[1].Value.bin.lpb : 0,
			"",
			&sProp.Value.bin.cb,
			&sProp.Value.bin.lpb);
		if (SUCCEEDED(hRes) && sProp.Value.bin.cb && sProp.Value.bin.lpb)
		{
			hRes = lpMessage->SetProps(1, &sProp, NULL);
		}
		delete[] sProp.Value.bin.lpb;
	}

	return hRes;
}
//
//void GetAllProfileNames(){
//	HRESULT hr = S_OK;
//	LPPROFADMIN lpProfAdmin = NULL;
//	LPMAPITABLE lpProfileTable = NULL;
//	LPSRowSet lpRowSet = NULL;
//	do{
//		DEFINE_IF_HR_NT_OK_BREAK(hr = MAPIAdminProfiles(0, &lpProfAdmin));
//		DEFINE_IF_HR_NT_OK_BREAK(hr = lpProfAdmin->GetProfileTable(NULL, &lpProfileTable));
//		
//		enum {_PR_DEFAULT_PROFILE,_PR_DISPLAY_NAME,_COUNT};
//		SizedSPropTagArray(_COUNT, storeEntryID) = { _COUNT, { PR_DEFAULT_PROFILE, PR_DISPLAY_NAME } };
//
//		DEFINE_IF_HR_NT_OK_BREAK(hr = lpProfileTable->SetColumns((LPSPropTagArray)&storeEntryID, NULL));
//
//		ULONG lRowCount = 0;
//		DEFINE_IF_HR_NT_OK_BREAK(hr = lpProfileTable->GetRowCount(NULL, &lRowCount));
//		DEFINE_IF_HR_NT_OK_BREAK(hr = lpProfileTable->QueryRows(lRowCount, NULL, &lpRowSet));
//		ULONG lRowIndex = 0;
//		while (lRowIndex < lpRowSet->cRows){
//			LPWSTR test = lpRowSet->aRow[lRowIndex].lpProps[_PR_DISPLAY_NAME].Value.lpszW;
//			//LPSTR testA = lpRowSet->aRow[lRowIndex].lpProps[_PR_DISPLAY_NAME].Value.lpszA;
//			bool isB = lpRowSet->aRow[lRowIndex].lpProps[_PR_DEFAULT_PROFILE].Value.b;
//			//OutputDebugStringA(testA);
//			OutputDebugStringW(test);
//			lRowIndex++;
//		}
//
//	} while (0);
//
//	if (lpRowSet != NULL){
//		FreeProws(lpRowSet);
//		lpRowSet = NULL;
//	}
//
//	if (lpProfileTable != NULL){
//		lpProfileTable->Release();
//		lpProfileTable = NULL;
//	}
//
//	if (lpProfAdmin != NULL){
//		lpProfAdmin->Release();
//		lpProfAdmin = NULL;
//	}
//}

//void Test(){
//	
//	HRESULT hr = 0;
//	LPMAPISESSION lpMapiSession = NULL;
//	LPMDB lpMdb = NULL;
//	IMAPITable* pIStoreTable = NULL;
//	LPSRowSet pRows = NULL;
//	IUnknown* lpExchManageStroe = NULL;
//	SPropValue*	pAllFoldersPropValue = NULL;
//	
//	do{
//		MAPIINIT_0 mapiInit = { 0, MAPI_MULTITHREAD_NOTIFICATIONS };
//		hr = MAPIInitialize(&mapiInit);
//
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//		//L"Outlook"
//		hr = MAPILogonEx(0, NULL , NULL, MAPI_NEW_SESSION | MAPI_USE_DEFAULT | MAPI_EXTENDED, &lpMapiSession);
//		
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		enum{ PR_DEFAULT_STORE_, PR_ENTRYID_, PR_RESOURCE_FLAGS_, PR_MDB_PROVIDER_, PR_DISPLAY_NAME_, COUNT };
//		SizedSPropTagArray(COUNT, storeEntryID) = { COUNT, { PR_DEFAULT_STORE, PR_ENTRYID, PR_RESOURCE_FLAGS, PR_MDB_PROVIDER, PR_DISPLAY_NAME} };
//		
//		ULONG ulRowCount = 0;
//		
//		ULONG ulRowIndex = 0;
//		BOOL bFind = FALSE;
//		hr = lpMapiSession->GetMsgStoresTable(0, &pIStoreTable);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		hr = pIStoreTable->SetColumns((LPSPropTagArray)&storeEntryID, 0);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		hr = pIStoreTable->GetRowCount(0, &ulRowCount);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		hr = pIStoreTable->QueryRows(ulRowCount, 0, &pRows);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		ulRowIndex = 0;
//		while (ulRowIndex<pRows->cRows)
//		{
//			_SRow row = pRows->aRow[ulRowIndex];
//			if (row.lpProps[PR_DEFAULT_STORE_].Value.b == TRUE && (row.lpProps[PR_RESOURCE_FLAGS_].Value.ul & STATUS_DEFAULT_STORE) )
//			{
//				bFind = TRUE;
//				break;
//			}
//
//			ulRowIndex++;
//		}
//
//		if (bFind)
//		{
//			hr = lpMapiSession->OpenMsgStore(0, pRows->aRow[ulRowIndex].lpProps[PR_ENTRYID_].Value.bin.cb,
//				(ENTRYID*)pRows->aRow[ulRowIndex].lpProps[PR_ENTRYID_].Value.bin.lpb, NULL,
//				MDB_WRITE | MAPI_BEST_ACCESS | MDB_NO_DIALOG, (IMsgStore**)&lpMdb);
//			DEFINE_IF_HR_NT_OK_BREAK(hr);
//		}
//		else {
//			break;
//		}
//		
//		enum { PR_IPM_OUTBOX_ENTRYID_, PR_VALID_FOLDER_MASK_, COUNT_ };
//		SizedSPropTagArray(COUNT_, rgPropTag) = { COUNT_, { PR_IPM_OUTBOX_ENTRYID, PR_VALID_FOLDER_MASK } };
//		
//		ULONG ulValues = 0;
//		hr = lpMdb->GetProps((LPSPropTagArray)&rgPropTag, 0, &ulValues, &pAllFoldersPropValue);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//		ULONG lpObjType = 0;
//		IMAPIFolder* lpMapiFolder = NULL;
//		if (pAllFoldersPropValue[PR_VALID_FOLDER_MASK_].Value.ul & FOLDER_IPM_OUTBOX_VALID){
//			hr = lpMdb->OpenEntry(pAllFoldersPropValue[PR_IPM_OUTBOX_ENTRYID_].Value.bin.cb, (ENTRYID*)pAllFoldersPropValue[PR_IPM_OUTBOX_ENTRYID_].Value.bin.lpb,
//				NULL, MAPI_BEST_ACCESS | MAPI_MODIFY, &lpObjType, (IUnknown**)&lpMapiFolder);
//			DEFINE_IF_HR_NT_OK_BREAK(hr);
//		}
//
//		hr = AddMailW(lpMapiSession, lpMapiFolder, L"ceshi测试12", L"lhy测试12", L"linghaiyang@lhytest.com", false, false, true, false);
//		DEFINE_IF_HR_NT_OK_BREAK(hr);
//
//	} while (0);
//
//	DWORD dError = GetLastError();
//
//	if (pAllFoldersPropValue){
//		MAPIFREEBUFFER(pAllFoldersPropValue);
//	}
//
//	if (lpExchManageStroe)
//	{
//		lpExchManageStroe->Release();
//		lpExchManageStroe = NULL;
//	}
//
//	if (lpMdb)
//	{
//		ULONG ulLogOffTag = LOGOFF_NO_WAIT;
//		lpMdb->StoreLogoff(&ulLogOffTag);
//		lpMdb->Release();
//		lpMdb = NULL;
//	}
//
//	if (pRows)
//	{
//		FreeProws(pRows);
//		pRows = NULL;
//	}
//
//	if (pIStoreTable)
//	{
//		pIStoreTable->Release();
//		pIStoreTable = NULL;
//	}
//
//	if (lpMapiSession){
//		lpMapiSession->Logoff(0, 0, 0);
//		lpMapiSession->Release();
//		lpMapiSession = NULL;
//	}
//
//	MAPIUninitialize();
//}

