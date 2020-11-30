#pragma once

#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
#include "CLR_IP_ParamStruct.h"
#include "..\RootTools_Cpp\\IP.h"
#include <iostream>

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
		static array<Cpp_LabelParam^>^ Cpp_Labeling(byte* pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark);

		static void Cpp_SubtractAbs(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_FindMinDiffLoc(array<byte>^ pSrcImg, array<byte>^ pInOutTarget, int nTransX, int nTransY, int nTargetW, int nTargetH, int nTrigger);
		// Method�� 3, 5 ���� (�ٸ� Method���� ����ȭ ���� �ʾ� Score�� 100 �̻� ���� �� ����) - ��Ȯ�� 3 < 5, �ӵ� 3 > 5
		// OutPosX, OutPosY�� Matching Box�� �»��. {CenterX, CenterY} = {OutPosX + nTempW / 2, OutPosY + nTempH / 2}
		static float Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod);
		static float Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod);

		//static void Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, bool bDark, [Out] array<Cpp_LabelParam^>^ outLabel);

		// Filtering
		static void Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nSig);
		static void Cpp_MedianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz);
		static void Cpp_Morphology(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz, System::String^ strMethod, int nIter);

		static void Cpp_SplitColorChannel(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample);
		static void Cpp_SplitColorChannel(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample);
		static void Cpp_SubSampling(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample);
		static void Cpp_SubSampling(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample);
		// BackSide

		// Recipe ƼĪ�� ���� �̹��� ũ�� ���� �ٿ��� MapData ����� ���� �뵵 > ��ü ���۰� ���� ������ DownScale �˳��ϰ� �ٰ�! ( > 10)
		// TMA ���� ������ �ȼ� ���� �� �ݺ����� �ʿ���ϴ� ��Ȳ������ �ٸ� �Լ� ��� (downScale = 1 ����...)
		static array<Cpp_Point^>^ Cpp_FindWaferEdge(byte* pSrcImg, float& inoutCenterX, float& inoutCenterY, float& inoutRadius, int  nMemW, int  nMemH, int downScale);
		// 1000 * 1000 Chip ũ��� Map Data ����
		static array<byte>^ Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);
		// Map Size �Ѱ��ָ� N * M ũ��� Map Data ����
		static array<byte>^ Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode);

		static void Cpp_SaveBMP(System::String^ strFilePath, array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nByteCnt);
		static void Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int  nMemW, int  nMemH, array<Cpp_Rect^>^ DefectRect, int nByteCnt);
		static void Cpp_LoadBMP(System::String^ strFilePath, array<byte>^ pOutImg, int  nMemW, int  nMemH);
	
		static void Cpp_SobelEdgeDetection(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta);
		static void Cpp_SobelEdgeDetection(BYTE* pSrcImg, BYTE* pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta);
	
		static void Cpp_Histogram(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int channels, int dims, int histSize, array<float>^ histRanges);
		static void Cpp_FilEllipse(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH);
		
		static void Cpp_ConvertRGB2H(array<byte>^ pRImg, array<byte>^ pGImg, array<byte>^ pBImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_DrawContourMap(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH);
		static void Cpp_CutOutROI(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB);
	};
}