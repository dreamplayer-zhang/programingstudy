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

typedef struct _DefectInfo
{
	LONG nIdx;
	LONG nClassifyCode;
	double fSize;
	LONG nLength;
	LONG nWidth;
	LONG nHeight;
	LONG nInspMode;
	LONG nFOV;
	double fPosX;
	double fPosY;
} DefectInfo;

// namespace;
using namespace std;


