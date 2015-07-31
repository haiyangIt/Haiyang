#pragma once

#include <vector>
#include <string>
using namespace std;

class CCompondFileHelper
{
public:
	CCompondFileHelper();
	~CCompondFileHelper();

private :
	void ConvertVarTypeToString(VARTYPE vt, WCHAR *pwszType, ULONG cchType);
	void ConvertValueToString(const PROPVARIANT &propvar, WCHAR *pwszValue, ULONG cchValue);
	void DisplayProperty(const PROPVARIANT &propvar, const STATPROPSTG &statpropstg);
	void DisplayPropertySet(FMTID fmtid, const WCHAR *pwszStorageName, IPropertyStorage *pPropStg);
	void DisplayPropertySetsInStorage(const WCHAR *pwszStorageName, IPropertySetStorage *pPropSetStg);
	void DisplayStorageTree(const WCHAR *pwszStorageName, IStorage *pStg);
	void Test();
private:
	void Display(const WCHAR *pwszStorageName);


private:

	void OutputLog(LPCWSTR pwszFormat, ...);

	LPCWSTR GetPrefixIndent(int indent);

	void OutputToConsole(STATSTG statStg, int type, int indent);

	HRESULT GetAllItem(LPSTORAGE pStg, LPENUMSTATSTG &pEnum);

	bool IsStorage(const STATSTG statStg);

	bool IsStream(const STATSTG statStg);

	void DisplayOtherInfo(STATSTG statStg, int indent);

	HRESULT DisplaySubStorage(LPSTORAGE pStg, STATSTG statStg, int indent);

	HRESULT DisplaySubStream(LPSTORAGE pStg, STATSTG statStg, int indent);

	HRESULT DisplayStorage(LPSTORAGE pStg, int indent);

	static vector<wstring> s_indent;

public:
	void DisplayAllInfo(const WCHAR *pwszStorageName);
};

