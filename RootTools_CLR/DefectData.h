#pragma once
#include <string>
#include <stdarg.h>

public ref class DefectData
{
public:
	int nIdx;
	int nClassifyCode;
	double fSize;
	int nLength;
	int nWidth;
	int nHeight;
	int nInspMode;
	int nFOV;
	double fPosX;
	double fPosY;

	bool bMerged;
	bool bMergeUsed;
};

