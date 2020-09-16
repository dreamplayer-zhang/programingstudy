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
		static void Cpp_Threshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemWidth, int  nMemHeight, bool bDark, int nThreshold);
		
		static float Cpp_Average(array<byte>^ pSrcImg, int  nMemWidth, int  nMemHeight);
		static float Cpp_Average(byte* pSrcImg, int  nMemWidth, int  nMemHeight, int nROILeft, int nROITop, int nROIRight, int nROIBot);

		static array<Cpp_LabelParam^>^ Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, bool bDark);
		static array<Cpp_LabelParam^>^ Cpp_Labeling(byte* pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, int nROILeft, int nROITop, int nROIRight, int nROIBot, bool bDark);
		
		static float Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pBinImg, int& nPosX, int& nPosY, int  nMemWidth, int  nMemHeight, int nTempWidth, int nTempHeight, int nMethod);
		static float Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pBinImg, int& nPosX, int& nPosY, int  nMemWidth, int  nMemHeight, int nTempWidth, int nTempHeight); // Managed Code에서는 Default Value 사용 불가
		//static void Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, bool bDark, [Out] array<Cpp_LabelParam^>^ outLabel);
		
		// Filtering
		static void Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemWidth, int  nMemHeight, int nSigma);
		static void Cpp_MedianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemWidth, int  nMemHeight, int nFilterSz);
		static void Cpp_Morphology(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemWidth, int  nMemHeight, int nFilterSz, System::String^ strMethod, int nIter);
	};
}

