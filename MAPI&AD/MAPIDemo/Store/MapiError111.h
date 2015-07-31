#pragma once
#include<string>
#include <MAPIX.h>
#include "Helper.h"

using namespace std;

class CMapiError111
{
public:
	CMapiError111();
	~CMapiError111();

	HRESULT Reproduce();
private:
	HRESULT ConnectServer();
	HRESULT CreateProfile(string &profileName, bool &bCreateProfile);

	HRESULT GetProfileName(string &profileName);
	HRESULT DeleteProfile(const string profileName);
	HRESULT OpenStore(const LPMAPISESSION lpSession);
private:
	HRESULT m_initError;
	string m_profileName;
	CHelper m_helper;
	bool m_bKeepProfile;
	string m_mailBoxServer;
	string m_mailBoxName;
	string m_mailProxyServer;
	bool m_hasSSL;
	int m_auth;
};

