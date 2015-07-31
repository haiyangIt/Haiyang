// Store.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <MAPIX.h>
#include <Mapiutil.h>
#include "edkmdb.h"
#include <IMessage.h>

//#define MAPI32_W

#define PR_INETMAIL_OVERRIDE_FORMAT PROP_TAG(PT_LONG, 0x5902)
#define cpidASCII 20127
#define PR_ICON_INDEX PROP_TAG(PT_LONG, 0x1080)

#define DEFINE_IF_HR_NT_OK_BREAK(hr) \
	if(hr != S_OK){	\
		break; \
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
	LPMAPISESSION lpMAPISession = NULL) {
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
	LPMAPISESSION lpMAPISession = NULL) {
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


	spvProps[p_PR_MESSAGE_CLASS_W].Value.lpszA = "IPM.Note";
	spvProps[p_PR_ICON_INDEX].Value.l = 0x103; // Unsent Mail
	spvProps[p_PR_SUBJECT_W].Value.lpszA = szSubject;
	spvProps[p_PR_CONVERSATION_TOPIC_W].Value.lpszA = szSubject;
	spvProps[p_PR_BODY_W].Value.lpszA = szBody;
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

HRESULT ConfigMsgService(){
	HRESULT hr = 0;
	do{
		// if not profile, create profile.
		// else use exist profile
		LPPROFADMIN lpProfAdmin;
		DEFINE_IF_HR_NT_OK_BREAK(MAPIAdminProfiles(0, &lpProfAdmin));

		LPTSTR strProfileName = L"lhytest";
		LPTSTR strProfilePsw = L"123.com";
		hr = lpProfAdmin->CreateProfile(strProfileName, strProfilePsw, NULL, 0);
		if (hr == MAPI_E_NO_ACCESS){
			// profile exist;
		}
		else if (hr == S_OK){
			LPSERVICEADMIN lpServiceAdmin;
			DEFINE_IF_HR_NT_OK_BREAK(lpProfAdmin->AdminServices(strProfileName, strProfilePsw, NULL, 0, &lpServiceAdmin));
			DEFINE_IF_HR_NT_OK_BREAK(lpServiceAdmin->CreateMsgService((LPTSTR)"MSEMS", L"", 0, 0));
			// todo config MsgService.
		}
		else {
			break;
		}
	} while (0);

	return hr;
}

void CreateProfile(){

}

LPSTR ConvertUnicode2Ansi(LPWSTR unicodeStr){
	bool isUsedDefaultChar;
	int cchWLen = wcslen(unicodeStr);

 	int cbLen = WideCharToMultiByte(CP_ACP, 0, unicodeStr, cchWLen, NULL, 0, NULL, NULL);
	char* buffer = new char[cbLen];
	WideCharToMultiByte(CP_ACP, 0, unicodeStr, cchWLen, buffer, cbLen, NULL, NULL);
	return buffer;
}

void SaveMsg2File(){
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

	//LPSTR subjectA = ConvertUnicode2Ansi(subject);
	//LPSTR bodyA = ConvertUnicode2Ansi(body);
	//LPSTR receiptA = ConvertUnicode2Ansi(receipt);
	LPSTR subjectA = "ceshi测试12";
	LPSTR bodyA = "lhy测试12";
	LPSTR receiptA = "linghaiyang@lhytest.com";

	do{
		MAPIINIT_0 mapiInit = { 0, MAPI_MULTITHREAD_NOTIFICATIONS };
		hr = MAPIInitialize(&mapiInit);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

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
	MAPIUninitialize();
}

void Test(){
	
	HRESULT hr = 0;
	LPMAPISESSION lpMapiSession = NULL;
	LPMDB lpMdb = NULL;
	IMAPITable* pIStoreTable = NULL;
	LPSRowSet pRows = NULL;
	IUnknown* lpExchManageStroe = NULL;
	SPropValue*	pAllFoldersPropValue = NULL;
	
	do{
		MAPIINIT_0 mapiInit = { 0, MAPI_MULTITHREAD_NOTIFICATIONS };
		hr = MAPIInitialize(&mapiInit);
		DEFINE_IF_HR_NT_OK_BREAK(hr);
		//L"Outlook"
		hr = MAPILogonEx(0, NULL , NULL, MAPI_NEW_SESSION | MAPI_USE_DEFAULT | MAPI_EXTENDED, &lpMapiSession);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		enum{ PR_DEFAULT_STORE_, PR_ENTRYID_, PR_RESOURCE_FLAGS_, PR_MDB_PROVIDER_, PR_DISPLAY_NAME_, COUNT };
		SizedSPropTagArray(COUNT, storeEntryID) = { COUNT, { PR_DEFAULT_STORE, PR_ENTRYID, PR_RESOURCE_FLAGS, PR_MDB_PROVIDER, PR_DISPLAY_NAME} };
		
		ULONG ulRowCount = 0;
		
		ULONG ulRowIndex = 0;
		BOOL bFind = FALSE;
		hr = lpMapiSession->GetMsgStoresTable(0, &pIStoreTable);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		hr = pIStoreTable->SetColumns((LPSPropTagArray)&storeEntryID, 0);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		hr = pIStoreTable->GetRowCount(0, &ulRowCount);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		hr = pIStoreTable->QueryRows(ulRowCount, 0, &pRows);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		ulRowIndex = 0;
		while (ulRowIndex<pRows->cRows)
		{
			_SRow row = pRows->aRow[ulRowIndex];
			if (row.lpProps[PR_DEFAULT_STORE_].Value.b == TRUE && (row.lpProps[PR_RESOURCE_FLAGS_].Value.ul & STATUS_DEFAULT_STORE) )
			{
				bFind = TRUE;
				break;
			}

			ulRowIndex++;
		}

		if (bFind)
		{
			hr = lpMapiSession->OpenMsgStore(0, pRows->aRow[ulRowIndex].lpProps[PR_ENTRYID_].Value.bin.cb,
				(ENTRYID*)pRows->aRow[ulRowIndex].lpProps[PR_ENTRYID_].Value.bin.lpb, NULL,
				MDB_WRITE | MAPI_BEST_ACCESS | MDB_NO_DIALOG, (IMsgStore**)&lpMdb);
			DEFINE_IF_HR_NT_OK_BREAK(hr);
		}
		else {
			break;
		}
		
		enum { PR_IPM_OUTBOX_ENTRYID_, PR_VALID_FOLDER_MASK_, COUNT_ };
		SizedSPropTagArray(COUNT_, rgPropTag) = { COUNT_, { PR_IPM_OUTBOX_ENTRYID, PR_VALID_FOLDER_MASK } };
		
		ULONG ulValues = 0;
		hr = lpMdb->GetProps((LPSPropTagArray)&rgPropTag, 0, &ulValues, &pAllFoldersPropValue);
		DEFINE_IF_HR_NT_OK_BREAK(hr);

		ULONG lpObjType = 0;
		IMAPIFolder* lpMapiFolder = NULL;
		if (pAllFoldersPropValue[PR_VALID_FOLDER_MASK_].Value.ul & FOLDER_IPM_OUTBOX_VALID){
			hr = lpMdb->OpenEntry(pAllFoldersPropValue[PR_IPM_OUTBOX_ENTRYID_].Value.bin.cb, (ENTRYID*)pAllFoldersPropValue[PR_IPM_OUTBOX_ENTRYID_].Value.bin.lpb,
				NULL, MAPI_BEST_ACCESS | MAPI_MODIFY, &lpObjType, (IUnknown**)&lpMapiFolder);
			DEFINE_IF_HR_NT_OK_BREAK(hr);
		}

		hr = AddMailW(lpMapiSession, lpMapiFolder, L"ceshi测试12", L"lhy测试12", L"linghaiyang@lhytest.com", false, false, true, false);
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
}

int _tmain(int argc, _TCHAR* argv[])
{
	UnLoadPrivateMAPI();
#ifdef MAPI32_W
	HMODULE hm = LoadLibrary(L"MSMAPI32.DLL");
#else
	HMODULE hm = LoadLibrary(L"EXMAPI32.DLL");
#endif
	if (hm != NULL)
	{
		SetMAPIHandle(hm);
		//Test();
		SaveMsg2File();
		FreeLibrary(hm);
	}
	return 0;
}

