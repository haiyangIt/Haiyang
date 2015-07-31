#pragma once

class CLdapHelper
{
public:
	CLdapHelper();
	~CLdapHelper();

	DWORD GetSslFlag();

private:
	HRESULT GetRootDse(IADs* &stRootDSE);

private:
	IADs* m_stRootDSE;
};

typedef struct st_cas_info
{
	std::wstring strCASCN;
	std::wstring strCASDN;

	std::wstring strCASrpcDN;
	std::wstring strExternalHostName;
	std::wstring strInternalHostName;

	DWORD rpcHttpFlags;
	DWORD ExternalMethods;
	DWORD InternalMethods;

	//////////////////////////////////////////////////////////////////////////
	st_cas_info()
	{
		strCASCN = L"";
		strCASDN = L"";

		rpcHttpFlags = 0;
		strCASrpcDN = L"";

		strExternalHostName = L"";
		strInternalHostName = L"";
		ExternalMethods = 2;
		InternalMethods = 8195;
	};
	//////////////////////////////////////////////////////////////////////////
}ST_GRT_CAS_INFO, *PST_GRT_CAS_INFO;

