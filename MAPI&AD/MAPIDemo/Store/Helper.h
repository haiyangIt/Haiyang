#pragma once
#include <string>
using namespace std;

class CHelper
{
public:
	CHelper();
	~CHelper();

	wstring ConvertAnsi2Unicode(LPSTR ansiStr);
	void OutputLog(LPSTR pszFormat, ...);
};

