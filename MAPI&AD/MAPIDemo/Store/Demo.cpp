#include "stdafx.h"
#include "MapiError111.h"
#include <iostream>
using namespace std;

int _tmain(int argc, _TCHAR* argv[]){
	HRESULT hr = S_OK;
	
	CMapiError111 error111;
	hr = error111.Reproduce();

	string spend;
	cout << endl << "input any key to exit" << endl;
	cin >> spend;

	return hr;
}