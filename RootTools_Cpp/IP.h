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
	�� ����
	  - �Լ� �Ķ���� ���� (Src - Input Image, Dst - Output Image, **Out**** - Result Data, ...)
	  - �����ε� �Ǿ��ִ� �Լ� ��� (Output�� ������ ��� ����)
		1. CS���� ���� �̹����� ���ο� �̹��� ���ۿ� ī���Ͽ� �Լ��� �Է����� ���
		2. ���� �̹����� ROI ������ �Է����� �޾� ���� ���� (Unsafe ���)
	  - 
	*/
	static void Threshold(BYTE* pSrc, BYTE* pDst, int nW, int nH, bool bDark, int thresh);
	static void Threshold(BYTE* pSrc, BYTE* pDst, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark, int thresh);

	static float Average(BYTE* pSrc, int nW, int nH);	
	static float Average(BYTE* pSrc, int nMemW, int nMemH, Point ptLT, Point ptRB);
	
	static void Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nW, int nH, bool bDark);
	static void Labeling(BYTE* pSrc, BYTE* pBin, std::vector<LabeledData>& vtOutLabeled, int nMemW, int nMemH, Point ptLT, Point ptRB, bool bDark);

	// Method ���� : https://docs.opencv.org/2.4/doc/tutorials/imgproc/histograms/template_matching/template_matching.html
	static float TemplateMatching(BYTE* pSrc, BYTE* pTemp, Point& outMatchPoint, int nSrcW, int nSrcH, int nTempW, int nTempH, Point ptLT, Point ptRB, int method = 5);

	// Filtering
	static void GaussianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nSig);
	static void MedianBlur(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz);
	static void Morphology(BYTE* pSrc, BYTE* pDst, int nW, int nH, int nFiltSz, std::string strMethod, int nIter);
};


