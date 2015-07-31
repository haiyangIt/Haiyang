
#define PROFILENAME L"E"				// The profile name
#define MAILSERVER "zhash05-ex13-2.autoex13-1.com"		// The mailbox server
#define CONVERT_CODE_PAGE 936			// The code page
#define CONVERT_LOCALE_ID 2052			// The locale id

#define PR_INETMAIL_OVERRIDE_FORMAT PROP_TAG(PT_LONG, 0x5902)
#define cpidASCII 20127
#define PR_ICON_INDEX PROP_TAG(PT_LONG, 0x1080)

#define MAPI_FORCE_ACCESS 0x00080000

HRESULT ModifyProfileGlobalSection_SaveMsgFile();

HRESULT ModifyGlobalProfileSection();

HRESULT SaveMsg2File();

STDMETHODIMP SvcAdminOpenProfileSection(LPSERVICEADMIN lpSvcAdmin,

	LPMAPIUID lpUID,

	LPCIID lpInterface,

	ULONG ulFlags,

	LPPROFSECT FAR * lppProfSect);

HRESULT ConfigMsgService();

LONG DeleteProfile();


LPSTR ConvertUnicode2Ansi(LPWSTR unicodeStr);

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
	LPMAPISESSION lpMAPISession = NULL);

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
	LPMAPISESSION lpMAPISession = NULL);

HRESULT AddMailA(LPMAPISESSION lpMAPISession,
	LPMAPIFOLDER lpFolder,
	LPSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPSTR szBody, // PR_BODY_W
	LPSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit);

HRESULT AddMailW(LPMAPISESSION lpMAPISession,
	LPMAPIFOLDER lpFolder,
	LPWSTR szSubject, // PR_SUBJECT_W, PR_CONVERSATION_TOPIC
	LPWSTR szBody, // PR_BODY_W
	LPWSTR szRecipientName, // Recipient table
	BOOL bHighImportance, // PR_IMPORTANCE
	BOOL bReadReceipt, // PR_READ_RECEIPT_REQUESTED
	BOOL bSubmit,
	BOOL bDeleteAfterSubmit);



HRESULT HrAllocAdrList(ULONG ulNumProps, LPADRLIST* lpAdrList);

HRESULT AddRecipientW(LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPWSTR szRecipientName);

HRESULT AddRecipientW(LPMAPISESSION lpMAPISession,
	LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPWSTR szRecipientName);

HRESULT AddRecipientA(LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPSTR szRecipientName);

HRESULT AddRecipientA(LPMAPISESSION lpMAPISession,
	LPMESSAGE lpMessage,
	ULONG ulRecipientType,
	LPSTR szRecipientName);

HRESULT BuildConversationIndex(ULONG* lpcbConversationIndex,
	LPBYTE* lppConversationIndex);

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
	LPBYTE* lppReportTag);

HRESULT AddReportTag(LPMESSAGE lpMessage);

