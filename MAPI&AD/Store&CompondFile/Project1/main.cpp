#include<windows.h>
#include<stdio.h>
#include<mbctype.h>
#include<locale.h>
#include<string>
using namespace std;

#include <malloc.h>
#include <stdlib.h>
#include <string.h>


////////// The "real" decoding stuff //////////
const char base64chars[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

#define SKIP '\202'
#define NOSKIP 'A'
//#define MaxLineLength 76
#define MaxLineLength 256

const char base64map[] =
{
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, 62, SKIP, SKIP, SKIP, 63,
	52, 53, 54, 55, 56, 57, 58, 59,
	60, 61, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, 0, 1, 2, 3, 4, 5, 6,
	7, 8, 9, 10, 11, 12, 13, 14,
	15, 16, 17, 18, 19, 20, 21, 22,
	23, 24, 25, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, 26, 27, 28, 29, 30, 31, 32,
	33, 34, 35, 36, 37, 38, 39, 40,
	41, 42, 43, 44, 45, 46, 47, 48,
	49, 50, 51, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP
};

const char hexmap[] = {
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	0, 1, 2, 3, 4, 5, 6, 7,
	8, 9, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, 10, 11, 12, 13, 14, 15, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP
};

const char QpEncodeMap[] = {
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, NOSKIP, SKIP, SKIP, NOSKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	NOSKIP, SKIP, SKIP, SKIP, SKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, SKIP, NOSKIP, NOSKIP,
	SKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, SKIP, SKIP, SKIP, SKIP, NOSKIP,
	SKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP, NOSKIP,
	NOSKIP, NOSKIP, NOSKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP,
	SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP, SKIP
};


char* Encode(char *input, int bufsize)
{
	int alsize = ((bufsize * 4) / 3);
	char *finalresult = (char*)calloc(alsize + ((alsize / 76) * 2) + (10 * sizeof(char)), sizeof(char));
	int count = 0;
	int LineLen = 0;
	char* fresult = finalresult;
	char *s = input;
	int tmp = 0;
	//let's step through the buffer and encode it...
	while (count <= bufsize)
	{
		if (count % 3 == 0 && count != 0)
		{
			tmp >>= 8;
			tmp &= 0xFFFFFF;
			//we have 4 new b64 chars, add them to finalresult
			int mid = tmp;
			mid >>= 18;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
			LineLen++;
			mid = tmp;
			mid >>= 12;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
			LineLen++;
			mid = tmp;
			mid >>= 6;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
			LineLen++;
			mid = tmp;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
			LineLen++;
			//reset tmp
			tmp = 0;
			//should we break the line...
			if (LineLen >= MaxLineLength)
			{
				*(fresult++) = '\r';
				*(fresult++) = '\n';
				LineLen = 0;
			}
			if (bufsize - count < 3)
				break;
		}
		unsigned char mid = (256 - (0 - *s));
		tmp |= mid;
		tmp <<= 8;
		count++;
		s++;
	}
	//do we have some chars left...
	int rest = (bufsize - count) % 3;
	if (rest != 0)
	{
		tmp = 0;
		int i;
		for (i = 0; i < 3; i++)
		{
			if (i < rest)
			{
				unsigned char mid = (256 - (0 - *s));
				tmp |= mid;
				tmp |= *s;
				tmp <<= 8;
				count++;
				s++;
			}
			else
			{
				tmp |= 0;
				tmp <<= 8;
			}
		}
		tmp >>= 8;
		tmp &= 0xFFFFFF;
		//we have some new b64 chars, add them to finalresult
		int mid = tmp;
		if (rest >= 1)
		{
			mid >>= 18;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
			mid = tmp;
			mid >>= 12;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
		}
		if (rest >= 2)
		{
			mid = tmp;
			mid >>= 6;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
		}
		if (rest >= 3)
		{
			mid = tmp;
			mid &= 0x3F;
			*(fresult++) = base64chars[mid];
		}
		for (int c = 3; c > rest; c--)
		{
			*(fresult++) = '=';
		}
	}
	return finalresult;
}

char* Decode(char *input, int *bufsize)
{
	int std = 0, count = 1, resultlen = 0;
	char *finalresult = (char*)calloc(*bufsize + sizeof(char), sizeof(char));
	char *s = input, *result = finalresult;
	while (*s != '=' && count <= *bufsize)
	{
		//check to see if it's a legal base64 char...
		while (base64map[*s] == SKIP)
		{
			if (*s != '\r' && *s != '\n')
			{
				//bad char...
				//we might want to tell the user that there was error in the encoded data...
				//ErrorCode = 1;
			}
			s++;
			(*bufsize)--;
			if (count >= *bufsize)
			{
				break;
			}
		}
		//add the base64 char to std...
		std |= base64map[*(s++) & 0xFF];
		std <<= 6;
		if (count % 4 == 0) //we have 3 more real chars...
		{
			//put std in the next 3 chars in finalresult
			int tmp;
			std >>= 6;
			tmp = std;
			tmp >>= 16;
			tmp &= 0xFF;
			*(result++) = (tmp);
			tmp = std;
			tmp >>= 8;
			tmp &= 0xFF;
			*(result++) = (tmp);
			tmp = std;
			tmp &= 0xFF;
			*(result++) = (tmp);
			std = 0; //empty std
			resultlen += 3;
		}
		count++;
	}
	//find and decode the remaining chars, if any...
	count--;
	if (count % 4 != 0)
	{
		//we have some remaining chars, now decode them...
		for (int i = 0; i < 4 - (count % 4); i++)
		{
			std <<= 6;
			resultlen++;
		}
		int tmp;
		std >>= 6;
		tmp = std;
		tmp >>= 16;
		tmp &= 0xFF;
		*(result++) = (tmp);
		tmp = std;
		tmp >>= 8;
		tmp &= 0xFF;
		*(result++) = (tmp);
		tmp = std;
		tmp &= 0xFF;
		*(result++) = (tmp);
	}
	*bufsize = resultlen;
	return finalresult;
}

#include<vector>
#define CNCODEPAGE 936	// Chinese
#define JPCODEPAGE 932	// Japanese
#define GRCODEPAGE 1253 // Greek
#define KRCODEPAGE 949	// Korean
#define DECODEPAGE 1250	// German
#define ENCODEPAGE 1252	// English


class CHelper{
public:
	CHelper();
	~CHelper();
private:
	static HRESULT ConvertUnicode2AnsiStrictly(LPCWSTR unicodeStr, ULONG ulCodePage, DWORD dwFlag, LPCSTR lpDefaultChar, string &result);
	//static HRESULT ConvertAnsi2UnicodeStrictly(LPCSTR ansiStr, ULONG ulCodePage, DWORD dwFlag, wstring &result);
	static vector<ULONG> GetCodepageArray();
	//static bool IsCodepageRight(ULONG codePage, string strAnsi, wstring wstrUnicode, bool isA2W);
public:
	static HRESULT ConvertUnicode2Ansi(LPCWSTR unicodeStr, ULONG ulCodePage, string &strAnsi);
	static HRESULT ConvertAnsi2Unicode(LPCSTR ansiStr, ULONG ulCodePage, wstring &strUnicode);
	static string ConvertUnicode2Ansi(LPCWSTR unicodeStr);
	static wstring ConvertAnsi2Unicode(LPCSTR ansiStr);
	static ULONG GetCurCodePage();

private:
	static string g_strDefaultChar;
};


string CHelper::g_strDefaultChar = "?";

CHelper::CHelper()
{

}

CHelper::~CHelper()
{

}


ULONG CHelper::GetCurCodePage(){
	return 1252;
}

HRESULT CHelper::ConvertUnicode2AnsiStrictly(LPCWSTR unicodeStr, ULONG ulCodePage, DWORD dwFlag, LPCSTR lpDefaultChar, string &result)
{
	result = "";
	if (unicodeStr == NULL)
		return 0;

	int cchWLen = wcslen(unicodeStr);
	BOOL isUsedDefaultChar = FALSE;
	int cbLen = WideCharToMultiByte(ulCodePage, dwFlag, unicodeStr, -1, NULL, 0, lpDefaultChar, &isUsedDefaultChar);
	if (isUsedDefaultChar)
	{
		return ERROR_NO_UNICODE_TRANSLATION;
	}

	if (cbLen == 0)
	{
		return GetLastError();
	}

	char* buffer = new char[cbLen + 1];
	if (buffer == NULL){
		return ERROR_BUFFER_OVERFLOW;
	}

	memset((void *)buffer, 0, sizeof(char) * (cbLen + 1));
	HRESULT hr = WideCharToMultiByte(ulCodePage, dwFlag, unicodeStr, cchWLen, buffer, cbLen, lpDefaultChar, &isUsedDefaultChar);

	if (isUsedDefaultChar)
	{
		return ERROR_NO_UNICODE_TRANSLATION;
	}

	if (hr == 0)
	{
		return GetLastError();
	}
	result = buffer;

	delete[] buffer;
	return 0;
}

//HRESULT CHelper::ConvertAnsi2UnicodeStrictly(LPCSTR ansiStr, ULONG ulCodePage, DWORD dwFlag, wstring &result)
//{
//	result = L"";
//	if (ansiStr == NULL)
//		return 0;
//
//	int sLen = MultiByteToWideChar(ulCodePage, dwFlag, ansiStr, -1, NULL, 0);
//	if (sLen == 0)
//	{
//		return GetLastError();
//	}
//
//	wchar_t* sUnicode = new wchar_t[sLen + 1];
//	if (sUnicode == NULL)
//		return ERROR_BUFFER_OVERFLOW;
//
//	memset((void *)sUnicode, 0, sizeof(wchar_t)*(sLen + 1));
//	HRESULT hr = MultiByteToWideChar(ulCodePage, dwFlag, ansiStr, strlen(ansiStr), sUnicode, sLen);
//	if (hr == 0){
//		return GetLastError();
//	}
//	result = sUnicode;
//	delete[] sUnicode;
//	return 0;
//}

HRESULT CHelper::ConvertUnicode2Ansi(LPCWSTR unicodeStr, ULONG ulCodePage, string &strAnsi)
{
	strAnsi = "";
	if (unicodeStr == NULL)
		return 0;

	int cchWLen = wcslen(unicodeStr);
	int cbLen = WideCharToMultiByte(ulCodePage, 0, unicodeStr, -1, NULL, 0, NULL, NULL);

	if (cbLen == 0)
	{
		return GetLastError();
	}

	char* buffer = new char[cbLen + 1];
	if (buffer == NULL){
		return ERROR_BUFFER_OVERFLOW;
	}

	memset((void *)buffer, 0, sizeof(char) * (cbLen + 1));
	HRESULT hr = WideCharToMultiByte(ulCodePage, 0, unicodeStr, cchWLen, buffer, cbLen, NULL, NULL);

	if (hr == 0)
	{
		return GetLastError();
	}
	strAnsi = buffer;

	delete[] buffer;
	return 0;
}

HRESULT CHelper::ConvertAnsi2Unicode(LPCSTR ansiStr, ULONG ulCodePage, wstring &strUnicode)
{
	strUnicode = L"";
	if (ansiStr == NULL)
		return 0;

	int sLen = MultiByteToWideChar(ulCodePage, 0, ansiStr, -1, NULL, 0);
	if (sLen == 0)
	{
		return GetLastError();
	}

	wchar_t* sUnicode = new wchar_t[sLen + 1];
	if (sUnicode == NULL)
		return ERROR_BUFFER_OVERFLOW;

	memset((void *)sUnicode, 0, sizeof(wchar_t)*(sLen + 1));
	HRESULT hr = MultiByteToWideChar(ulCodePage, 0, ansiStr, strlen(ansiStr), sUnicode, sLen);

	if (hr == 0){
		return GetLastError();
	}
	strUnicode = sUnicode;
	delete[] sUnicode;
	return 0;
}

vector<ULONG> CHelper::GetCodepageArray()
{
	vector<ULONG> vecCodepages;
	vecCodepages.reserve(5);
	vecCodepages.push_back(ENCODEPAGE);
	vecCodepages.push_back(JPCODEPAGE);
	vecCodepages.push_back(DECODEPAGE);
	vecCodepages.push_back(KRCODEPAGE);
	vecCodepages.push_back(CNCODEPAGE);

	return vecCodepages;
}

//bool CHelper::IsCodepageRight(ULONG codePage, string strAnsi, wstring wstrUnicode, bool isA2W)
//{
//	if (isA2W)
//	{
//		string backAnsi = "";
//		HRESULT hr = ConvertUnicode2AnsiStrictly(wstrUnicode.c_str(), codePage, WC_NO_BEST_FIT_CHARS, g_strDefaultChar.c_str(), backAnsi);
//		if (!hr)
//			return memcmp(strAnsi.c_str(), backAnsi.c_str(), (strAnsi.length() + 1)*sizeof(char)) == 0;
//		return false;
//	}
//	else
//	{
//		wstring backUnicode = L"";
//		HRESULT hr = ConvertAnsi2UnicodeStrictly(strAnsi.c_str(), codePage, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, backUnicode);
//		if (!hr)
//			return memcmp(wstrUnicode.c_str(), backUnicode.c_str(), (wstrUnicode.length() + 1)*sizeof(wchar_t)) == 0;
//		return false;
//	}
//}

string CHelper::ConvertUnicode2Ansi(LPCWSTR unicodeStr)
{
	ULONG codePage = GetCurCodePage();
	//1 getDefaultAnsiStr
	string strResultByDefault = "";
	HRESULT hr = ConvertUnicode2Ansi(unicodeStr, codePage, strResultByDefault);
	if (hr)
		return "";
	//2 loop try each code page
	vector<ULONG> vecCodepages = GetCodepageArray();
	vector<ULONG>::iterator end = vecCodepages.end();

	for (vector<ULONG>::iterator begin = vecCodepages.begin(); begin != end; begin++)
	{
		ULONG codepage = *begin;
		string strAnsi = "";
		HRESULT hr = ConvertUnicode2AnsiStrictly(unicodeStr, codepage, WC_NO_BEST_FIT_CHARS, g_strDefaultChar.c_str(), strAnsi);
		if (!hr)
		{
			return strAnsi;
		}
	}

	return strResultByDefault;
}

wstring CHelper::ConvertAnsi2Unicode(LPCSTR ansiStr)
{
	ULONG codePage = GetCurCodePage();

	wstring wstrResultByDefault = L"";
	HRESULT hr = ConvertAnsi2Unicode(ansiStr, codePage, wstrResultByDefault);
	if (hr)
		return L"";
	return wstrResultByDefault;

	/*vector<ULONG> vecCodepages = GetCodepageArray();
	vector<ULONG>::iterator end = vecCodepages.end();

	for (vector<ULONG>::iterator begin = vecCodepages.begin(); begin != end; begin++)
	{
		ULONG codepage = *begin;
		wstring wstrUnicode = L"";
		HRESULT hr = ConvertAnsi2UnicodeStrictly(ansiStr, codepage, MB_ERR_INVALID_CHARS | MB_PRECOMPOSED, wstrUnicode);
		if (!hr)
		{
			if (IsCodepageRight(codepage, ansiStr, wstrUnicode, true)){
				return wstrUnicode;
			}
		}
	}

	return false;*/
}


int main(int argc, char* argv[])
{
	//string testJp = "かんぽ生命";
	wstring testJPW = L"かんぽ生命";
	string testJPRight = "";
	CHelper::ConvertUnicode2Ansi(testJPW.c_str(), JPCODEPAGE, testJPRight);

	wstring testGRW = L"Υποχώρηση της κυβέρνησης στις διαπραγματεύσεις με την τρόικα για την υπαγωγή του ΕΝΦΙΑ στη νέα ευνοϊκή ρύθμιση των 100 δόσεων";
	string testGRRight = "";
	CHelper::ConvertUnicode2Ansi(testGRW.c_str(), GRCODEPAGE, testGRRight);

	//wstring testDEW = L"Seit dem mutmaßlichen Massaker an 43 Studenten vor zwei Monaten steht die mexikanische Regierung unter Druck. Jetzt greift Präsident Peña Nieto durch: Der Sicherheitsapparat soll umfassend reformiert werden.";
	//string testDERight = "";
	//CHelper::ConvertUnicode2Ansi(testDEW.c_str(), DECODEPAGE, testDERight);

	//wstring testKRW = L"한국어/조선말";
	//string testKRRight = "";
	//CHelper::ConvertUnicode2Ansi(testKRW.c_str(), KRCODEPAGE, testKRRight);

	//wstring testCNW = L"我爱我家";
	//string testCNRight = "";
	//CHelper::ConvertUnicode2Ansi(testCNW.c_str(), CNCODEPAGE, testCNRight);

	////1 convert right;
	//wstring testJPW1 = L"";
	//CHelper::ConvertAnsi2Unicode(testJPRight.c_str(), JPCODEPAGE, testJPW1);
	////2 try convert 
	//string testJPTry = CHelper::ConvertUnicode2Ansi(testJPW.c_str());
	//wstring testJPTryW = CHelper::ConvertAnsi2Unicode(testJPRight.c_str());

	//1 convert right;
	wstring testGRW1 = L"";
	CHelper::ConvertAnsi2Unicode(testGRRight.c_str(), GRCODEPAGE, testGRW1);
	//2 try convert 
	string testGRTry = CHelper::ConvertUnicode2Ansi(testGRW.c_str());
	wstring testGRTryW = CHelper::ConvertAnsi2Unicode(testGRRight.c_str());

	////1 convert right;
	//wstring testDEW1 = L"";
	//CHelper::ConvertAnsi2Unicode(testDERight.c_str(), DECODEPAGE, testDEW1);
	////2 try convert 
	//string testDETry = CHelper::ConvertUnicode2Ansi(testDEW.c_str());
	//wstring testDETryW = CHelper::ConvertAnsi2Unicode(testDERight.c_str());

	////1 convert right;
	//wstring testKRW1 = L"";
	//CHelper::ConvertAnsi2Unicode(testKRRight.c_str(), KRCODEPAGE, testKRW1);
	////2 try convert 
	//string testKRTry = CHelper::ConvertUnicode2Ansi(testKRW.c_str());
	//wstring testKRTryW = CHelper::ConvertAnsi2Unicode(testKRRight.c_str());

	////1 convert right;
	//wstring testCNW1 = L"";
	//CHelper::ConvertAnsi2Unicode(testCNRight.c_str(), CNCODEPAGE, testCNW1);
	////2 try convert 
	//string testCNTry = CHelper::ConvertUnicode2Ansi(testCNW.c_str());
	//wstring testCNTryW = CHelper::ConvertAnsi2Unicode(testCNRight.c_str());


	exit(0);


	wstring test1 = L"12345";
	wstring findStr1 = L"23";

	int pos = test1.find(findStr1);
	wstring substr = test1.substr(pos + findStr1.length(), test1.length() - findStr1.length() - pos);
	char* encodeStr = Encode((char*)test1.c_str(), (test1.size()+1)*sizeof(wchar_t));
	printf("base 64:%s\n", encodeStr);

	int size = 1024;
	char* decodeStr = Decode(encodeStr, &size);
	wchar_t* decodeWStr = (wchar_t*)decodeStr;
	wstring tempStr(decodeWStr);
	exit(0);


	string test2 = "12345";

	wchar_t test3[] = L"12345";
	char test4[] = "12345";

	printf("12345 w length:%d\n", test1.length());
	printf("12345 a length:%d\n", test2.length());

	printf("12345 w sizeof:%d\n", sizeof(test1)/sizeof(wchar_t));
	printf("12345 a sizeof:%d\n", sizeof(test2)/sizeof(char));

	printf("12345 w strLen:%d\n", wcsnlen_s(test1.c_str(),2048));
	printf("12345 a strLen:%d\n", strnlen_s(test2.c_str(), 2048));

	{
		char a;
		scanf_s("%c", &a);
	}
	exit(0);

	UINT cpId = GetACP();
	
	printf("CodePageId:%d\n", cpId);
	LCID lcid = GetSystemDefaultLCID();
	printf("LCID:%d\n", lcid);
	LANGID langId = GetSystemDefaultLangID();
	printf("LangId:%d\n", langId);

	LCID threadLcid = GetThreadLocale();
	printf("threadLcid:%d,%x\n", threadLcid, threadLcid);

	
	
	//int cp = _getmbcp();
	//printf("CodePageId by getmbcp:%d\n\n", cp);
	int success = SetThreadLocale(1032);
	printf("\n\n");
	if (success){
		UINT cpId = GetACP();
		printf("CodePageId:%d\n", cpId);
		LCID lcid = GetSystemDefaultLCID();
		printf("LCID:%d\n", lcid);
		LANGID langId = GetSystemDefaultLangID();
		printf("LangId:%d\n", langId);

		LCID threadLcid = GetThreadLocale();
		printf("threadLcid:%d,%x\n", threadLcid, threadLcid);
	}

	char a;
	scanf_s("%c", &a);
}