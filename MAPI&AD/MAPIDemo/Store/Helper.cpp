#include "stdafx.h"
#include "Helper.h"

CHelper::CHelper()
{
}


CHelper::~CHelper()
{
}

wstring CHelper::ConvertAnsi2Unicode(LPSTR ansiStr){
	//ansi to unicode
	wstring result = L"";
	if (ansiStr == NULL)
	return result;

	int sLen = MultiByteToWideChar(CP_ACP, NULL, ansiStr, -1, NULL, 0);
	wchar_t* sUnicode = new wchar_t[sLen + 1];
	if (sUnicode == NULL)
	return result;

	memset((void *)sUnicode, 0, sizeof(wchar_t)*(sLen + 1));
	MultiByteToWideChar(CP_ACP, NULL, ansiStr, strlen(ansiStr), sUnicode, sLen);

	result = sUnicode;

	delete[] sUnicode;
	return result;
}

void CHelper::OutputLog(LPSTR pszFormat, ...){
	char msg[255] = { 0 };
	va_list args;
	va_start(args, pszFormat);
	_vsnprintf_s(msg, 255, 255, pszFormat, args);
	va_end(args);
	OutputDebugStringA(msg);
	OutputDebugStringA("\n");
	printf_s("%s\n", msg);
}
