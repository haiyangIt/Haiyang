#include <mapix.h>

#include <mapiutil.h>

#include <edkmdb.h>

#include "Profiles.h"



/***********************************************************************



STDMETHODIMP CreateProfile(LPSTR lpszProfileName,

LPSTR lpszExchangeServer,

LPSTR lpszMailbox)



- lpszProfileName:                    [in] Name of profile to be created

- lpszExchangeServer:   [in] Name of the Exchange server

- lpszMailbox:                           [in] Name of the mailbox



This procedure is fairly straightforward. It creates a profile by using the MAPI interfaces.



***********************************************************************/

STDMETHODIMP CreateProfile(LPSTR lpszProfileName,

	LPSTR lpszExchangeServer,

	LPSTR lpszMailbox)

{

	HRESULT         hRes = S_OK;                                   // Result from MAPI calls                      

	LPPROFADMIN     lpProfAdmin = NULL;             // Profile Admin object



	// Get an IProfAdmin interface

	hRes = MAPIAdminProfiles(0,                             // Flags

		&lpProfAdmin); // Pointer to new IProfAdmin

	if (SUCCEEDED(hRes) && lpProfAdmin)

	{

		// Create a new profile

		hRes = lpProfAdmin->CreateProfile((LPTSTR)lpszProfileName,          // Name of new profile

			NULL,                                               // Password for profile

			NULL,                                               // Handle to parent window

			NULL);                                  // Flags

		if (SUCCEEDED(hRes))

		{

			LPSERVICEADMIN  lpSvcAdmin = NULL;              // Service Admin object



			// Get an IMsgServiceAdmin interface off the IProfAdmin interface

			hRes = lpProfAdmin->AdminServices((LPTSTR)lpszProfileName,       // Profile that we want to modify

				NULL,                                               // Password for that profile

				NULL,                                               // Handle to parent window

				0,                                           // Flags

				&lpSvcAdmin);                      // Pointer to new IMsgServiceAdmin

			if (SUCCEEDED(hRes) && lpSvcAdmin)

			{

				// Create the new message service for Exchange

				hRes = lpSvcAdmin->CreateMsgService((LPTSTR)"MSEMS",                       // Name of service from MAPISVC.INF

					NULL,                         // Display name of service

					NULL,                         // Handle to parent window

					NULL);                                    // Flags

				if (SUCCEEDED(hRes))

				{

					// We now have to obtain the entry ID for the new service.

					// You can do this by obtaining the message service table

					// and by obtaining the entry that corresponds to the new service.



					LPMAPITABLE     lpMsgSvcTable = NULL;  // Table to hold services



					hRes = lpSvcAdmin->GetMsgServiceTable(0,                                      // Flags

						&lpMsgSvcTable);       // Pointer to table

					if (SUCCEEDED(hRes) && lpMsgSvcTable)

					{

						LPSRowSet       lpSvcRows = NULL;             // Rowset to hold results of table query

						SRestriction      sres;                                                     // Restriction structure

						SPropValue                  SvcProps;                                            // Property structure for restriction



						// Set up restriction to query table.



						sres.rt = RES_CONTENT;

						sres.res.resContent.ulFuzzyLevel = FL_FULLSTRING;

						sres.res.resContent.ulPropTag = PR_SERVICE_NAME_A;

						sres.res.resContent.lpProp = &SvcProps;



						SvcProps.ulPropTag = PR_SERVICE_NAME;

						SvcProps.Value.lpszA = "MSEMS";



						// Query the table to obtain the entry for the newly created message service.

						// This indicates the columns that we want to be returned from HrQueryAllRows

						enum { iSvcName, iSvcUID, cptaSvc };

						SizedSPropTagArray(cptaSvc, sptCols) = { cptaSvc, PR_SERVICE_NAME_A, PR_SERVICE_UID };



						hRes = HrQueryAllRows(lpMsgSvcTable,

							(LPSPropTagArray)&sptCols,

							&sres,

							NULL,

							0,

							&lpSvcRows);

						if (SUCCEEDED(hRes) && lpSvcRows)

						{

							// Set up a SPropValue array for the properties that you have to configure.

							SPropValue rgval[2];

							int i = 0;



							// First, the server name

							ZeroMemory(&rgval[i], sizeof(SPropValue));

							rgval[i].ulPropTag = PR_PROFILE_UNRESOLVED_SERVER;

							rgval[i++].Value.lpszA = lpszExchangeServer;



							// Next, the mailbox name

							ZeroMemory(&rgval[i], sizeof(SPropValue));

							rgval[i].ulPropTag = PR_PROFILE_UNRESOLVED_NAME;

							rgval[i++].Value.lpszA = lpszMailbox;



							hRes = lpSvcAdmin->ConfigureMsgService((LPMAPIUID)lpSvcRows->aRow[0].lpProps[1].Value.bin.lpb, // Entry ID of service to configure

								NULL,                                                                                                                                              // Handle to parent window

								0,                                                                                                                                                      // Flags

								i,                                                                                                                                                       // Number of properties that we are setting

								rgval);                                                                                                                                                // Pointer to SPropValue array

						}



						if (lpSvcRows) FreeProws(lpSvcRows);

					}



					if (lpMsgSvcTable) lpMsgSvcTable->Release();

				}

			}



			if (lpSvcAdmin) lpSvcAdmin->Release();

		}

	}



	if (lpProfAdmin) lpProfAdmin->Release();



	return hRes;

}



/***********************************************************************



STDMETHODIMP DeleteProfile(LPSTR lpszProfileName)



- lpszProfileName:                    [in] Name of profile to delete



This procedure is fairly straightforward.  It deletes the indicated profile by using the

MAPI interfaces.



***********************************************************************/

STDMETHODIMP DeleteProfile(LPSTR lpszProfileName)

{

	HRESULT                               hRes = S_OK;

	LPPROFADMIN     lpProfAdmin = NULL;



	// Get an IProfAdmin interface



	hRes = MAPIAdminProfiles(0,                             // Flags

		&lpProfAdmin); // Pointer to new IProfAdmin

	if (SUCCEEDED(hRes) && lpProfAdmin)

	{

		hRes = lpProfAdmin->DeleteProfile(lpszProfileName, 0);

	}



	if (lpProfAdmin) lpProfAdmin->Release();



	return hRes;

}



STDMETHODIMP OpenGlobalProfileSection(LPSTR lpszProfile, LPPROFSECT * lppProfSect)

{

	HRESULT hRes = S_OK;

	LPPROFADMIN lpProfAdmin = NULL;



	hRes = MAPIAdminProfiles(0, &lpProfAdmin);

	if (SUCCEEDED(hRes) && lpProfAdmin)

	{

		LPSERVICEADMIN lpSvcAdmin = NULL;



		hRes = lpProfAdmin->AdminServices((LPTSTR)lpszProfile,

			NULL,

			NULL,

			0,

			&lpSvcAdmin);

		if (SUCCEEDED(hRes) && lpSvcAdmin)

		{

			hRes = lpSvcAdmin->OpenProfileSection((LPMAPIUID)&pbGlobalProfileSectionGuid,

				NULL,

				0,

				lppProfSect);



			lpSvcAdmin->Release();

		}



		lpProfAdmin->Release();

	}



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



		// 

		///////////////////////////////////////////////////////////////////

	}



	return hRes;

}



STDMETHODIMP ProvAdminOpenProfileSection(LPPROVIDERADMIN lpProvAdmin,

	LPMAPIUID lpUID,

	LPCIID lpInterface,

	ULONG ulFlags,

	LPPROFSECT FAR * lppProfSect)

{

	HRESULT hRes = S_OK;



	hRes = lpProvAdmin->OpenProfileSection(lpUID,

		lpInterface,

		ulFlags | MAPI_FORCE_ACCESS,

		lppProfSect);



	if ((FAILED(hRes)) && (MAPI_E_UNKNOWN_FLAGS == hRes))

	{

		// The MAPI_FORCE_ACCESS flag is implemented only in Outlook 2002 and in later versions of Outlook.

		// 



		// 

		//                                     Makes MAPI think we are a service and not a client.

		//                                     MAPI grants us Service Access.  This makes it all possible.

		*(((BYTE*)lpProvAdmin) + 0x60) = 0x2;  // USE THIS METHOD AT YOUR OWN RISK! THIS METHOD IS NOT SUPPORTED!



		hRes = lpProvAdmin->OpenProfileSection(lpUID,

			lpInterface,

			ulFlags,

			lppProfSect);

	}



	return hRes;

}



STDMETHODIMP EnableReconnect(LPSTR lpszProfile)

{

	HRESULT hRes = S_OK;

	LPPROFADMIN lpProfAdmin = NULL;



	hRes = MAPIAdminProfiles(0, &lpProfAdmin);

	if (SUCCEEDED(hRes) && lpProfAdmin)

	{

		LPSERVICEADMIN lpSvcAdmin = NULL;



		hRes = lpProfAdmin->AdminServices((LPTSTR)lpszProfile,

			NULL,

			NULL,

			0,

			&lpSvcAdmin);

		if (SUCCEEDED(hRes) && lpSvcAdmin)

		{

			LPPROFSECT lpABProfSect = NULL;



			hRes = SvcAdminOpenProfileSection(lpSvcAdmin,

				(LPMAPIUID)&MUIDEMSAB, NULL, MAPI_MODIFY, &lpABProfSect);

			if (SUCCEEDED(hRes) && lpABProfSect)

			{

				SPropValue spReconnectProps[2] = { 0 };



				spReconnectProps[0].ulPropTag = PR_PROFILE_ABP_ALLOW_RECONNECT;

				spReconnectProps[0].Value.l = 1;



				spReconnectProps[1].ulPropTag = PR_PROFILE_ABP_MTHREAD_TIMEOUT_SECS;

				spReconnectProps[1].Value.l = 10;



				hRes = lpABProfSect->SetProps(2, spReconnectProps, NULL);

				if (SUCCEEDED(hRes))

				{

					LPPROFSECT lpGlobalProfSect = NULL;



					hRes = SvcAdminOpenProfileSection(lpSvcAdmin,

						(LPMAPIUID)&pbGlobalProfileSectionGuid, NULL, MAPI_MODIFY, &lpGlobalProfSect);

					if (SUCCEEDED(hRes) && lpGlobalProfSect)

					{

						SPropValue spServerVersion = { 0 };



						spServerVersion.ulPropTag = PR_PROFILE_SERVER_VERSION;

						spServerVersion.Value.l = 3000;



						hRes = lpGlobalProfSect->SetProps(1, &spServerVersion, NULL);

					}



					if (lpGlobalProfSect)

						lpGlobalProfSect->Release();

				}

			}



			if (lpABProfSect)

				lpABProfSect->Release();

		}



		if (lpSvcAdmin)

			lpSvcAdmin->Release();

	}



	if (lpProfAdmin)

		lpProfAdmin->Release();



	return hRes;

}