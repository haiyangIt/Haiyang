// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once
#define INITGUID
#include "targetver.h"

#include <stdio.h>
#include <tchar.h>
#include <mapiguid.h>
#include <Windows.h>


#define DEFINE_IF_HR_NT_OK_BREAK(hr) \
	if(hr != S_OK){	\
		break; \
		}

#define PR_PROFILE_RPC_PROXY_SERVER_    PROP_TAG(PT_STRING8, 0x6622)

#define PR_PROFILE_RPC_PROXY_SERVER_FLAGS_   PROP_TAG(PT_LONG, 0x6623)
#define PRXF_ENABLED_				((DWORD)0x00000001)
#define PRXF_SSL_					((DWORD)0x00000002)
#define PRXF_IGNORE_SEC_WARNING_    ((DWORD)0x00000010)

#define PR_PROFILE_RPC_PROXY_SERVER_AUTH_PACKAGE_     PROP_TAG(PT_LONG, 0x6627)
/*
RPC_C_HTTP_AUTHN_SCHEME_BASIC      0x00000001
RPC_C_HTTP_AUTHN_SCHEME_NTLM       0x00000002
RPC_C_HTTP_AUTHN_SCHEME_NEGOTIATE  0x00000010
*/

#define PR_PROFILE_AUTH_PACKAGE_    PROP_TAG(PT_LONG, 0x6619)
/*
RPC_C_AUTHN_NONE(0)
RPC_C_AUTHN_GSS_NEGOTIATE(9) .
RPC_C_AUTHN_WINNT(10)
*/



