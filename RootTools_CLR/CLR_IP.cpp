#include "pch.h"

#include "CLR_IP.h"
#include <msclr\marshal_cppstd.h>

namespace RootTools_CLR
{
	void CLR_IP::Cpp_Threshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, bool bDark, int nThresh)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Threshold(pSrc, pDst, nMemW, nMemH, bDark, nThresh);

		pSrc = nullptr;  // unpin
		pDst = nullptr;
	}
	void CLR_IP::Cpp_Threshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nThresh)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Threshold(pSrc, pDst, nMemW, nMemH, false, nThresh);

		pSrc = nullptr;  // unpin
		pDst = nullptr;
	}
	void CLR_IP::Cpp_Threshold(byte* pSrcImg, array<byte>^ pDstImg, int nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark, int nThresh)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Threshold(pSrc, pDst, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), bDark, nThresh);

		pSrc = nullptr;  // unpin
		pDst = nullptr;
	}

	float CLR_IP::Cpp_Average(array<byte>^ pSrcImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pImg = &pSrcImg[0]; // pin : 주소값 고정

		return IP::Average(pImg, nMemW, nMemH);

		pImg = nullptr; // unpin
	}
	float CLR_IP::Cpp_Average(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB)
	{
		pin_ptr<byte> pImg = &pSrcImg[0]; // pin : 주소값 고정

		return IP::Average(pImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB));

		pImg = nullptr; // unpin
	}

	array<Cpp_LabelParam^>^ CLR_IP::Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, bool bDark)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pBin = &pBinImg[0];

		std::vector<LabeledData> vtLabeled;

		IP::Labeling(pSrc, pBin, vtLabeled, nMemW, nMemH, bDark);

		array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

		pSrc = nullptr;  // unpin
		pBin = nullptr;

		bool bResultExist = vtLabeled.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtLabeled.size(); i++)
			{
				local[i] = gcnew Cpp_LabelParam();
				local[i]->centerX = vtLabeled[i].center.x;
				local[i]->centerY = vtLabeled[i].center.y;

				local[i]->boundTop = vtLabeled[i].bound.top;
				local[i]->boundBottom = vtLabeled[i].bound.bottom;
				local[i]->boundLeft = vtLabeled[i].bound.left;
				local[i]->boundRight = vtLabeled[i].bound.right;

				local[i]->area = vtLabeled[i].area;
				local[i]->value = vtLabeled[i].value;
			}
		}
		return local;
	}
	array<Cpp_LabelParam^>^ CLR_IP::Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pBin = &pBinImg[0];

		std::vector<LabeledData> vtLabeled;

		IP::Labeling(pSrc, pBin, vtLabeled, nMemW, nMemH, false);

		array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

		pSrc = nullptr;  // unpin
		pBin = nullptr;

		bool bResultExist = vtLabeled.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtLabeled.size(); i++)
			{
				local[i] = gcnew Cpp_LabelParam();
				local[i]->centerX = vtLabeled[i].center.x;
				local[i]->centerY = vtLabeled[i].center.y;

				local[i]->boundTop = vtLabeled[i].bound.top;
				local[i]->boundBottom = vtLabeled[i].bound.bottom;
				local[i]->boundLeft = vtLabeled[i].bound.left;
				local[i]->boundRight = vtLabeled[i].bound.right;

				local[i]->area = vtLabeled[i].area;
				local[i]->value = vtLabeled[i].value;
			}
		}
		return local;
	}
	array<Cpp_LabelParam^>^ CLR_IP::Cpp_Labeling(byte* pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, bool bDark)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pBin = &pBinImg[0];

		std::vector<LabeledData> vtLabeled;

		IP::Labeling(pSrc, pBin, vtLabeled, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), bDark);

		array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

		pSrc = nullptr;  // unpin
		pBin = nullptr;

		bool bResultExist = vtLabeled.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtLabeled.size(); i++)
			{
				local[i] = gcnew Cpp_LabelParam();
				local[i]->centerX = vtLabeled[i].center.x;
				local[i]->centerY = vtLabeled[i].center.y;

				local[i]->boundTop = vtLabeled[i].bound.top;
				local[i]->boundBottom = vtLabeled[i].bound.bottom;
				local[i]->boundLeft = vtLabeled[i].bound.left;
				local[i]->boundRight = vtLabeled[i].bound.right;

				local[i]->area = vtLabeled[i].area;
				local[i]->value = vtLabeled[i].value;
			}
		}
		return local;
	}

	void CLR_IP::Cpp_SubtractAbs(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc1 = &pSrcImg1[0];
		pin_ptr<byte> pSrc2 = &pSrcImg2[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::SubtractAbs(pSrc1, pSrc2, pDst, nMemW, nMemH);

		pSrc1 = nullptr;
		pSrc2 = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_FindMinDiffLoc(array<byte>^ pSrcImg, array<byte>^ pInOutTarget, int nTransX, int nTransY, int nTargetW, int nTargetH, int nTrigger)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTarget = &pInOutTarget[0];

		Point Trans = IP::FindMinDiffLoc(pSrc, pTarget, nTargetW, nTargetH, nTrigger);
		nTransX = Trans.x;
		nTransY = Trans.y;
		pSrc = nullptr;
		pTarget = nullptr;
	}

	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}
	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	void CLR_IP::Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nSig)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; 
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::GaussianBlur(pSrc, pDst, nMemW, nMemH, nSig);

		pSrc = nullptr;  
		pDst = nullptr;
	}

	void CLR_IP::Cpp_MedianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; 
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::MedianBlur(pSrc, pDst, nMemW, nMemH, nFiltSz);

		pSrc = nullptr; 
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Morphology(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nFiltSz, System::String^ strMethod, int nIter)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		std::string MorpOp = msclr::interop::marshal_as<std::string>(strMethod);

		IP::Morphology(pSrc, pDst, nMemW, nMemH, nFiltSz, MorpOp, nIter);

		pSrc = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_SplitColorChannel(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pSplitImg = &pOutImg[0];

		IP::SplitColorChannel(pSrc, pSplitImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nChIndex, nDownSample);

		pSrc = nullptr;
		pSplitImg = nullptr;
	}
	void CLR_IP::Cpp_SplitColorChannel(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nChIndex, int nDownSample)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pSplitImg = &pOutImg[0];

		IP::SplitColorChannel(pSrc, pSplitImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nChIndex, nDownSample);

		pSrc = nullptr;
		pSplitImg = nullptr;
	}
	void CLR_IP::Cpp_SubSampling(BYTE* pSrcImg, BYTE* pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDownImg = &pOutImg[0];

		IP::SubSampling(pSrc, pDownImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDownSample);

		pSrc = nullptr;
		pDownImg = nullptr;
	}
	void CLR_IP::Cpp_SubSampling(BYTE* pSrcImg, array<byte>^ pOutImg, int nMemW, int nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDownSample)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDownImg = &pOutImg[0];

		IP::SubSampling(pSrc, pDownImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDownSample);

		pSrc = nullptr;
		pDownImg = nullptr;
	}
	array<Cpp_Point^>^ CLR_IP::Cpp_FindWaferEdge(byte* pSrcImg, float& inoutCenterX, float& inoutCenterY, float& inoutRadius, int nMemW, int nMemH, int downScale)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];

		std::vector<Point> vtWaferEdge;
		vtWaferEdge = IP::FindWaferEdge(pSrc, inoutCenterX, inoutCenterY, inoutRadius, nMemW, nMemH, downScale);

		pSrc = nullptr;	

		bool bResultExist = vtWaferEdge.size() > 0;
		array<Cpp_Point^>^ local = gcnew array<Cpp_Point^>(vtWaferEdge.size());

		if (bResultExist)
		{
			for (int i = 0; i < vtWaferEdge.size(); i++)
			{
				local[i] = gcnew Cpp_Point();

				local[i]->x = vtWaferEdge[i].x;
				local[i]->y = vtWaferEdge[i].y;
			}
		}
		return local;
	}

	array<byte>^ CLR_IP::Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
	{
		std::vector<Point> vtPoint;
		
		for (int i = 0; i < Contour->Length; i++)
			vtPoint.push_back(Point(Contour[i]->x / downScale, Contour[i]->y / downScale));

		std::vector<byte> vtMap;
		vtMap = IP::GenerateMapData(vtPoint, outOriginX, outOriginY, outMapX, outMapY, nW, nH, downScale, isIncludeMode);

		array<byte>^ map = gcnew array<byte>(vtMap.size());
		bool bResultExist = vtMap.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtMap.size(); i++)
			{
				map[i] = vtMap[i];
			}
		}
		return map;
	}
	array<byte>^ CLR_IP::Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
	{
		std::vector<Point> vtPoint;

		for (int i = 0; i < Contour->Length; i++)
			vtPoint.push_back(Point(Contour[i]->x / downScale, Contour[i]->y / downScale));

		std::vector<byte> vtMap;
		vtMap = IP::GenerateMapData(vtPoint, outOriginX, outOriginY, outChipSzX, outChipSzY, outMapX, outMapY, nW, nH, downScale, isIncludeMode);

		array<byte>^ map = gcnew array<byte>(outMapX * outMapY);
		bool bResultExist = vtMap.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtMap.size(); i++)
			{
				map[i] = vtMap[i];
			}
		}
		return map;
	}

	void CLR_IP::Cpp_SaveBMP(System::String^ strFilePath, array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nByteCnt)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		IP::SaveBMP(sFilePath, pSrc, nMemW, nMemH, nByteCnt);
		pSrc = nullptr;
	}

	void CLR_IP::Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int  nMemW, int  nMemH, array<Cpp_Rect^>^ DefectRect, int nByteCnt)
	{
		if (DefectRect->Length == 0)
			return;

		pin_ptr<byte> pSrc = &pSrcImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);

		std::vector<Rect> defectRect;
		for(int i = 0 ; i < DefectRect->Length; i++)
			defectRect.push_back(Rect(DefectRect[i]->x, DefectRect[i]->y, DefectRect[i]->w, DefectRect[i]->h));

		IP::SaveDefectListBMP(sFilePath, pSrc, nMemW, nMemH, defectRect, nByteCnt);

		pSrc = nullptr;
	}

	void CLR_IP::Cpp_LoadBMP(System::String^ strFilePath, array<byte>^ pOutImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pOut = &pOutImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		IP::LoadBMP(sFilePath, pOut, nMemW, nMemH);
		pOut = nullptr;
	}

	void CLR_IP::Cpp_ConvertRGB2H(array<byte>^ pRImg, array<byte>^ pGImg, array<byte>^ pBImg, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pR = &pRImg[0];
		pin_ptr<byte> pG = &pGImg[0];
		pin_ptr<byte> pB = &pBImg[0];
		pin_ptr<byte> pH = &pDstImg[0];

		IP::ConvertRGB2H(pR, pG, pB, pH, nMemW, nMemH);

		pR = nullptr;
		pG = nullptr;
		pB = nullptr;
		pH = nullptr;
	}
	void CLR_IP::Cpp_DrawContourMap(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::DrawContourMap(pSrc, pDst, nMemW, nMemH);

		pSrc = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_CutOutROI(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::CutOutROI(pSrc, pDst, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB));

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_SobelEdgeDetection(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::SobelEdgeDetection(pSrc, pDst, nW, nH, nDerivativeX, nDerivativeY, nKernelSize, nScale, nDelta);

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_SobelEdgeDetection(BYTE* pSrcImg, BYTE* pDstImg, int nW, int nH, int nDerivativeX, int nDerivativeY, int nKernelSize, int nScale, int nDelta)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::SobelEdgeDetection(pSrc, pDst, nW, nH, nDerivativeX, nDerivativeY, nKernelSize, nScale, nDelta);

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Histogram(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH, int channels, int dims, int histSize, array<float>^ histRanges)	
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];
		pin_ptr<float> pHistRanges = &histRanges[0];

		IP::Histogram(pSrc, pDst, nW, nH, channels, dims, histSize, pHistRanges);

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_FilEllipse(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nW, int nH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::FitEllipse(pSrc, pDst, nW, nH);

		pSrc = nullptr;
		pDst = nullptr;
	}

}