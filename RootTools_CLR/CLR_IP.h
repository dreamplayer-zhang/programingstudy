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
		static void Cpp_Threshold(byte* pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark, int nThresh);

		static float Cpp_Average(array<byte>^ pSrcImg, int  nMemW, int  nMemH);
		static float Cpp_Average(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB);

		static array<Cpp_LabelParam^>^ Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, bool bDark);
		static array<Cpp_LabelParam^>^ Cpp_Labeling(byte* pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark);
		
		// Method는 3, 5 권장 (다른 Method들은 정규화 되지 않아 Score가 100 이상 나올 수 있음) - 정확성 3 < 5, 속도 3 > 5
		// OutPosX, OutPosY는 Matching Box의 좌상단. {CenterX, CenterY} = {OutPosX + nTempW / 2, OutPosY + nTempH / 2}
		static float Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& nOutPosX, int& nOutPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod);
		static float Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& nOutPosX, int& nOutPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod);

		//static void Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, bool bDark, [Out] array<Cpp_LabelParam^>^ outLabel);
		
		// Filtering
		static void Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nSig);
		static void Cpp_MedianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz);
		static void Cpp_Morphology(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz, System::String^ strMethod, int nIter);
	};
}

