#pragma once

#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
#include "CLR_IP_ParamStruct.h"
#include "..\RootTools_Cpp\\IP.h"
#include "..\RootTools_Cpp\\ResizeSSE.h"
#include <iostream>

using namespace System::Collections::Generic; // List

namespace RootTools_CLR
{
	public ref class CLR_IP
	{
	protected:

	public:
		// Function(Src - Input Image, Dst - Output Image, **Out**** - Result Data, ...)
		static void Cpp_Threshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, bool bDark, int nThresh);
		static void Cpp_Threshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nThresh);
		static void Cpp_Threshold(byte* pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark, int nThresh);

		static float Cpp_Average(array<byte>^ pSrcImg, int  nMemW, int  nMemH);
		static float Cpp_Average(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB);

		static array<Cpp_LabelParam^>^ Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, bool bDark);
		static array<Cpp_LabelParam^>^ Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH);
		static array<Cpp_LabelParam^>^ Cpp_Labeling_SubPix(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, bool bDark, int thresh, float Scale);
		static void Cpp_Masking(array<byte>^ pSrcImg, array<byte>^ pDstImg, array<Cpp_Point^>^ startPoint, array<int>^ length, int  nMemW, int  nMemH);
		// Method는 3, 5 권장 (다른 Method들은 정규화 되지 않아 Score가 100 이상 나올 수 있음) - 정확성 3 < 5, 속도 3 > 5
		// OutPosX, OutPosY는 Matching Box의 좌상단. {CenterX, CenterY} = {OutPosX + nTempW / 2, OutPosY + nTempH / 2}
		static float Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx);
		static float Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx);
		static float Cpp_TemplateMatching(array<byte>^ pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt);
		static float Cpp_TemplateMatching_LargeTrigger(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx);
		static float Cpp_TemplateMatching_LargeTrigger(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx);
		// ********* D2D ******** //
		static void Cpp_SubtractAbs(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_SelectMinDiffinArea(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ RefROILT, Cpp_Point^ CurROILT, int stride, int nROIW, int nROIH);
		static void Cpp_FindMinDiffLoc(array<byte>^ pSrcImg, array<byte>^ pInOutTarget, int nTransX, int nTransY, int nTargetW, int nTargetH, int nTrigger);

		// Create Golden Image	
		// 전체 이미지 포인터를 받아와 필요한 만큼 잘라서 사용 (추후 SSE 등으로 한번 더 최적화 필요)
		static void Cpp_CreateGoldenImage_Avg(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH);
		static void Cpp_CreateGoldenImage_MedianAvg(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH);
		static void Cpp_CreateGoldenImage_Median(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH);

		// D2D 3.0
		// EdgeSuppression, BrightSuppression Level은 1~10 단계, 0 = 사용 x
		static void Cpp_CreateDiffScaleMap(array<byte>^ pSrcImg, array<float>^ pDstImg, int  nMemW, int  nMemH, int nEdgeSuppressionLev, int nBrightSuppressionLev);
		static void Cpp_CreateHistogramWeightMap(array<byte>^ pSrcImg, array<byte>^ pGoldenImg, array<float>^ pDstImg, int  nMemW, int  nMemH, int nWeightLev);

		// ********* Pattern Inspection ******** //
		static void Cpp_HistogramBaseTreshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nHistOffset, int  nMemW, int  nMemH, bool bDark);

		// Elementwise Operation
		static void Cpp_Multiply(array<byte>^ pSrcImg1, array<float>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_Multiply(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_Bitwise_NOT(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_Bitwise_AND(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_Bitwise_OR(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);

		// Filtering
		static void Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nSig);
		static void Cpp_AverageBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_MedianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz);
		static void Cpp_Morphology(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz, System::String^ strMethod, int nIter);

		// BackSide
		// Recipe 티칭시 원본 이미지 크기 대폭 줄여서 MapData 만들기 위한 용도 > 전체 버퍼가 들어가기 때문에 DownScale 넉넉하게 줄것! ( > 10)
		// TMA 같이 정밀한 픽셀 측정 및 반복성을 필요로하는 상황에서는 다른 함수 사용 (downScale = 1 느림...)
		static array<Cpp_Point^>^ Cpp_FindWaferEdge(byte* pSrcImg, float& inoutCenterX, float& inoutCenterY, float& inoutRadius, int  nMemW, int  nMemH, int downScale);
		// 1000 * 1000 Chip 크기로 Map Data 생성
		static array<int>^ Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);
		// Map Size 넘겨주면 N * M 크기로 Map Data 생성
		static array<int>^ Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);
		static int Cpp_CalcAdaptiveThresholdParam(array<byte>^ pSrcImg, array<byte>^ pMaskImg, int  nMemW, int  nMemH, int nGap_DominantGV2ParamGV);

		// Image(Feature/Defect Image) Load/Save
		static void Cpp_SaveBMP(System::String^ strFilePath, array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nByteCnt);
		static void Cpp_LoadBMP(System::String^ strFilePath, array<byte>^ pOutImg, int  nMemW, int  nMemH, int nByteCnt);
		static void Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int nMemW, int nMemH, Cpp_Rect^ DefectRect, int imageNum);
		static void Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int nMemW, int nMemH, array<Cpp_Rect^>^ DefectRect);
		static void Cpp_SaveDefectListBMP_Color(System::String^ strFilePath, BYTE* pRImg, BYTE* pGImg, BYTE* pBImg, int nMemW, int nMemH, Cpp_Rect^ DefectRect, int imageNum);
		static void Cpp_SaveDefectListBMP_Color(System::String^ strFilePath, BYTE* pRImg, BYTE* pGImg, BYTE* pBImg, int nMemW, int nMemH, array<Cpp_Rect^>^ DefectRect);

		// ETC.
		static void Cpp_SplitColorChannel(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample);
		static void Cpp_SplitColorChannel(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample);
		static void Cpp_SubSampling(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample);
		static void Cpp_SubSampling(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample);
		static void Cpp_ConvertRGB2H(array<byte>^ pRImg, array<byte>^ pGImg, array<byte>^ pBImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_DrawContourMap(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_CutOutROI(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB);
		static void Cpp_GoldenImageReview(array<array<byte>^>^ pSrcImg, array<byte>^ pDstImg, int imgNum, int nMemW, int nMemH);

		static void Cpp_SobelEdgeDetection(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta);
		static void Cpp_SobelEdgeDetection(BYTE* pSrcImg, BYTE* pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta);

		static void Cpp_Histogram(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int channels, int dims, int histSize, array<float>^ histRanges);
		static void Cpp_FilEllipse(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH);

		void Cpp_CreatInterpolationData(int i,double dXScale, double dXShift, int nWidth);
		void Cpp_ProcessInterpolation(int i,int thid, BYTE* pSrc, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE* ppTarget, int nXOffset, int nYOffset, int nDir, int nSy, int nEy);
		void Cpp_ProcessInterpolation(int i,int  thid, BYTE* pSrcImg, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE pTarget);

		static int Cpp_FindEdge(array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel);
		static int Cpp_FindEdge(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel);
		static int Cpp_FindEdge16bit(array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel);
		static int Cpp_FindEdge16bit(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel);
	};
}