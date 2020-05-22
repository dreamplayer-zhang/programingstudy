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

typedef struct _DefectDataStruct
{
	LONG nIdx;
	LONG nClassifyCode;
	float fAreaSize;
	LONG nLength;
	LONG nWidth;
	LONG nHeight;
	LONG nInspMode;
	LONG nFOV;
	double fPosX;
	double fPosY;
} DefectDataStruct;

// namespace;


