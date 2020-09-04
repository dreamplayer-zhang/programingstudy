#pragma once

#include <opencv2/opencv.hpp>

#include "TypeDefines.h"

using namespace cv;

class IP
{
public:
	static void Threshold(BYTE* pSrc, int nW, int nH, int nThrehold, bool bDark, BYTE* pDst);
	static void Labeling(BYTE* pSrc, BYTE* pBin, int nW, int nH, bool bDark, BYTE* pDst, std::vector<LabeledData>& vtLabeled);

	//Vision
	static void Threshold(BYTE* pSrc, BYTE* pDst, int nW, int nH, bool bDark, int threshold);
	
	static float Average(BYTE* pSrc, int nW, int nH);	
	static void Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtLabeled, int nW, int nH, bool bDark);
	
	// Filtering
	static void GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSigma);
	static void MedianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFilterSz);
	static void Morphology(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFilterSz, std::string strMethod, int nIter);
};


