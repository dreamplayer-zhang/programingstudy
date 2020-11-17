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

	static void SubtractAbs(BYTE* pSrc1, BYTE* pSrc2, BYTE* pDst, int nW, int nH);
	static Point FindMinDiffLoc(BYTE* pSrc, BYTE* pInOutTarget, int nTargetW, int nTargetH, int nTrigger);
	// Method 참고 : https://docs.opencv.org/2.4/doc/tutorials/imgproc/histograms/template_matching/template_matching.html
	static float TemplateMatching(BYTE* pSrc, BYTE* pTemp, Point& outMatchPoint, int nSrcW, int nSrcH, int nTempW, int nTempH, Point ptLT, Point ptRB, int method = 5);

	// Filtering
	static void GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSig);
	static void MedianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz);
	static void Morphology(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz, std::string strMethod, int nIter);

	// BackSide
	static std::vector<Point> FindWaferEdge(BYTE* pSrc, float& inoutCenterX, float& inoutCenterY, float& inoutRadius, int nW, int nH, int downScale);
	static std::vector<byte> GenerateMapData(std::vector<Point> vtContour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);
	static std::vector<byte> GenerateMapData(std::vector<Point> vtContour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);

	// Bitmap Save / Load
	static void SaveBMP(String sFilePath, BYTE* pSrc, int nW, int nH, int nByteCnt);
	static void SaveDefectListBMP(String sFilePath, BYTE* pSrc, int nW, int nH, std::vector<Rect> DefectRect, int nByteCnt);
	static void LoadBMP(String sFilePath, BYTE* pOut, int nW, int nH);

	// ETC.
	static void SplitColorChannel(BYTE* pSrc, BYTE* pOutImg, int nW, int nH, Point ptLT, Point ptRB, int nChIndex, int nDownSample);
	static void SubSampling(BYTE* pSrc, BYTE* pOutImg, int nW, int nH, Point ptLT, Point ptRB, int nDownSample);
	static void ConvertRGB2H(BYTE* pR, BYTE* pG, BYTE* pB, BYTE* pOutH, int nW, int nH);
	static void DrawContourMap(BYTE* pSrc, BYTE* pDst, int nW, int nH);
	static void CutOutROI(BYTE* pSrc, BYTE* pDst, int nW, int nH, Point ptLT, Point ptRB);

	static void SobelEdgeDetection(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nDerivativeX = 1, int nDerivativeY = 0, int nKernelSize = 5, int nScale = 1, int nDelta = 1);
	static void Histogram(BYTE* pSrc, BYTE* pDst, int nW, int nH, int channels, int dims, int histSize, float* ranges);
	static void FitEllipse(BYTE* pSrc, BYTE* pDst, int nW, int nH);
};


