#pragma once

#include <opencv2/opencv.hpp>

#include "TypeDefines.h"

using namespace cv;

class IP
{
public:
	static void Threshold(BYTE* pSrc, int nW, int nH, int nThrehold, bool bDark, BYTE* pDst);
	static void Labeling(BYTE* pSrc, BYTE* pBin, int nW, int nH, bool bDark, BYTE* pDst, std::vector<LabeledData>& vtLabeled);

	// Vision
	/*
	※ 참고
	  - 함수 파라미터 순서 (Src - Input Image, Dst - Output Image, **Out**** - Result Data, ...)
	  - 오버로딩 되어있는 함수 사용 (Output이 영상인 경우 제외)
		1. CS에서 원본 이미지를 새로운 이미지 버퍼에 카피하여 함수의 입력으로 사용
		2. 원본 이미지와 ROI 정보를 입력으로 받아 연산 진행 (Unsafe 사용)
	  - 
	*/
	static void Threshold(BYTE* pSrc, BYTE* pDst, int nW, int nH, bool bDark, int thresh);
	static void Threshold(BYTE* pSrc, BYTE* pDst, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark, int thresh);

	static float Average(BYTE* pSrc, int nW, int nH);	
	static float Average(BYTE* pSrc, int nMemW, int nMemH, Point ptLT, Point ptRB);
	
	static void Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nW, int nH, bool bDark);
	static void Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark);

	// Method 참고 : https://docs.opencv.org/2.4/doc/tutorials/imgproc/histograms/template_matching/template_matching.html
	static float TemplateMatching(BYTE* pSrc, BYTE* pTemp, Point& outMatchPoint, int nSrcW, int nSrcH, int nTempW, int nTempH, Point ptLT, Point ptRB, int method = 5);

	// Filtering
	static void GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSig);
	static void MedianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz);
	static void Morphology(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz, std::string strMethod, int nIter);
};


