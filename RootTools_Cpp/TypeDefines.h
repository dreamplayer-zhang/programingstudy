#pragma once

// standary library
#include <windows.h>
#include <vector>

// User Type Defines
typedef struct _LabeledData
{	
	POINT center;
	RECT bound;
	LONG area;
	LONG value;
} LabeledData;

// namespace;
using namespace std;


