#include "pch.h"

#include "CLR_IP.h"
#include <msclr\marshal_cppstd.h>

#pragma warning(disable: 4244)
#pragma warning(disable: 4267)
#pragma warning(disable: 4793)

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
				local[i]->centerX = vtLabeled[i].centerX;
				local[i]->centerY = vtLabeled[i].centerY;

				local[i]->width = vtLabeled[i].width;
				local[i]->height = vtLabeled[i].height;

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

		IP::Labeling(pSrc, pBin, vtLabeled, nMemW, nMemH, true);

		array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

		pSrc = nullptr;  // unpin
		pBin = nullptr;

		bool bResultExist = vtLabeled.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtLabeled.size(); i++)
			{
				local[i] = gcnew Cpp_LabelParam();
				local[i]->centerX = vtLabeled[i].centerX;
				local[i]->centerY = vtLabeled[i].centerY;

				local[i]->width = vtLabeled[i].width;
				local[i]->height = vtLabeled[i].height;

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
	array<Cpp_LabelParam^>^ CLR_IP::Cpp_Labeling_SubPix(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemW, int  nMemH, bool bDark, int thresh, float Scale)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pBin = &pBinImg[0];

		std::vector<LabeledData> vtLabeled;

		IP::Labeling_SubPix(pSrc, pBin, vtLabeled, nMemW, nMemH, bDark, thresh, Scale);

		array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

		pSrc = nullptr;  // unpin
		pBin = nullptr;

		bool bResultExist = vtLabeled.size() > 0;

		if (bResultExist)
		{
			for (int i = 0; i < vtLabeled.size(); i++)
			{
				local[i] = gcnew Cpp_LabelParam();
				local[i]->centerX = (float)vtLabeled[i].centerX;
				local[i]->centerY = vtLabeled[i].centerY;

				local[i]->width = vtLabeled[i].width;
				local[i]->height = vtLabeled[i].height;

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

	void CLR_IP::Cpp_Masking(array<byte>^ pSrcImg, array<byte>^ pDstImg, array<Cpp_Point^>^ startPoint, array<int>^ length, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		std::vector<Point> vtStartPoint;
		std::vector<int> vtLength;

		for (int i = 0; i < startPoint->Length; i++)
		{
			vtStartPoint.push_back(Point(startPoint[i]->x, startPoint[i]->y));
			int nLen = length[i];
			vtLength.push_back(nLen);
		}
		IP::Masking(pSrc, pDst, vtStartPoint, vtLength, nMemW, nMemH);

		pSrc = nullptr;
		pDst = nullptr;
	}

	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod, nByteCnt, nChIdx);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	float CLR_IP::Cpp_TemplateMatching(array<byte>^ pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching_VRS(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod, nByteCnt);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod, nByteCnt, nChIdx);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	float CLR_IP::Cpp_TemplateMatching_LargeTrigger(byte* pSrcImg, array<byte>^ pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching_LargeTrigger(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod, nByteCnt, nChIdx);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	float CLR_IP::Cpp_TemplateMatching_LargeTrigger(byte* pSrcImg, byte* pTempImg, int& outPosX, int& outPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod, int nByteCnt, int nChIdx)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching_LargeTrigger(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod, nByteCnt, nChIdx);

		outPosX = Pos.x;
		outPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}

	// ********* D2D ******** //
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
	void CLR_IP::Cpp_SelectMinDiffinArea(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ RefROILT, Cpp_Point^ CurROILT, int stride, int nROIW, int nROIH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		std::vector<Point> vtPoint;

		for (int i = 0; i < RefROILT->Count; i++)
			vtPoint.push_back(Point(RefROILT[i]->x, RefROILT[i]->y));

		IP::SelectMinDiffinArea(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, Point(CurROILT->x, CurROILT->y), stride, nROIW, nROIH);

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_CreateGoldenImage_Avg(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		std::vector<Point> vtPoint;

		for (int i = 0; i < ROILT->Count; i++)
			vtPoint.push_back(Point(ROILT[i]->x, ROILT[i]->y));

		IP::CreateGoldenImage_Avg(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, nROIW, nROIH);

		pSrc = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_CreateGoldenImage_MedianAvg(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		std::vector<Point> vtPoint;

		for (int i = 0; i < ROILT->Count; i++)
			vtPoint.push_back(Point(ROILT[i]->x, ROILT[i]->y));

		if (imgNum < 4)
			IP::CreateGoldenImage_Avg(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, nROIW, nROIH);
		else
			IP::CreateGoldenImage_MedianAvg(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, nROIW, nROIH);

		pSrc = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_CreateGoldenImage_Median(byte* pSrcImg, array<byte>^ pDstImg, int imgNum, int  nMemW, int  nMemH, List<Cpp_Point^>^ ROILT, int nROIW, int nROIH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		std::vector<Point> vtPoint;

		for (int i = 0; i < ROILT->Count; i++)
			vtPoint.push_back(Point(ROILT[i]->x, ROILT[i]->y));

		if (imgNum < 4)
			IP::CreateGoldenImage_Avg(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, nROIW, nROIH);
		else
			IP::CreateGoldenImage_Median(pSrc, pDst, imgNum, nMemW, nMemH, vtPoint, nROIW, nROIH);

		pSrc = nullptr;
		pDst = nullptr;
	}

	// D2D 3.0
	void CLR_IP::Cpp_CreateHistogramWeightMap(array<byte>^ pSrcImg, array<byte>^ pGoldenImg, array<float>^ pDstImg, int  nMemW, int  nMemH, int nWeightLev)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pGolden = &pGoldenImg[0];
		pin_ptr<float> pDst = &pDstImg[0];

		IP::CreateHistogramWeightMap(pSrc, pGolden, pDst, nMemW, nMemH, nWeightLev);

		pSrc = nullptr;
		pGolden = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_CreateDiffScaleMap(array<byte>^ pSrcImg, array<float>^ pDstImg, int  nMemW, int  nMemH, int nEdgeSuppressionLev, int nBrightSuppressionLev)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<float> pDst = &pDstImg[0];

		IP::CreateDiffScaleMap(pSrc, pDst, nMemW, nMemH, nEdgeSuppressionLev, nBrightSuppressionLev);

		pSrc = nullptr;
		pDst = nullptr;
	}

	// ********* Pattern Inspection ******** //
	void CLR_IP::Cpp_HistogramBaseTreshold(array<byte>^ pSrcImg, array<byte>^ pDstImg, int nHistOffset, int  nMemW, int  nMemH, bool bDark)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::HistogramBaseTreshold(pSrc, pDst, nHistOffset, nMemW, nMemH, bDark);

		pSrc = nullptr;  // unpin
		pDst = nullptr;
	}

	// Elementwise Operation
	void CLR_IP::Cpp_Multiply(array<byte>^ pSrcImg1, array<float>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc1 = &pSrcImg1[0];
		pin_ptr<float> pSrc2 = &pSrcImg2[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Multiply(pSrc1, pSrc2, pDst, nMemW, nMemH);

		pSrc1 = nullptr;
		pSrc2 = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Multiply(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc1 = &pSrcImg1[0];
		pin_ptr<byte> pSrc2 = &pSrcImg2[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Multiply(pSrc1, pSrc2, pDst, nMemW, nMemH);

		pSrc1 = nullptr;
		pSrc2 = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Bitwise_NOT(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Bitwise_NOT(pSrc, pDst, nMemW, nMemH);

		pSrc = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Bitwise_AND(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc1 = &pSrcImg1[0];
		pin_ptr<byte> pSrc2 = &pSrcImg2[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Bitwise_AND(pSrc1, pSrc2, pDst, nMemW, nMemH);

		pSrc1 = nullptr;
		pSrc2 = nullptr;
		pDst = nullptr;
	}

	void CLR_IP::Cpp_Bitwise_OR(array<byte>^ pSrcImg1, array<byte>^ pSrcImg2, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc1 = &pSrcImg1[0];
		pin_ptr<byte> pSrc2 = &pSrcImg2[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::Bitwise_OR(pSrc1, pSrc2, pDst, nMemW, nMemH);

		pSrc1 = nullptr;
		pSrc2 = nullptr;
		pDst = nullptr;
	}

	// Filtering
	void CLR_IP::Cpp_GaussianBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH, int nSig)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::GaussianBlur(pSrc, pDst, nMemW, nMemH, nSig);

		pSrc = nullptr;
		pDst = nullptr;
	}
	void CLR_IP::Cpp_AverageBlur(array<byte>^ pSrcImg, array<byte>^ pDstImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::AverageBlur(pSrc, pDst, nMemW, nMemH);

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
	
	// BackSide
	array<int>^ CLR_IP::Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
	{
		std::vector<Point> vtPoint;

		for (int i = 0; i < Contour->Length; i++)
			vtPoint.push_back(Point(Contour[i]->x / downScale, Contour[i]->y / downScale));

		std::vector<byte> vtMap;
		vtMap = IP::GenerateMapData(vtPoint, outOriginX, outOriginY, outMapX, outMapY, nW, nH, downScale, isIncludeMode);

		array<int>^ map = gcnew array<int>(vtMap.size());
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
	array<int>^ CLR_IP::Cpp_GenerateMapData(array<Cpp_Point^>^ Contour, float& outOriginX, float& outOriginY, float& outChipSzX, float& outChipSzY, int& outMapX, int& outMapY, int nW, int nH, int downScale, bool isIncludeMode)
	{
		std::vector<Point> vtPoint;

		for (int i = 0; i < Contour->Length; i++)
			vtPoint.push_back(Point(Contour[i]->x / downScale, Contour[i]->y / downScale));

		std::vector<byte> vtMap;
		vtMap = IP::GenerateMapData(vtPoint, outOriginX, outOriginY, outChipSzX, outChipSzY, outMapX, outMapY, nW, nH, downScale, isIncludeMode);

		array<int>^ map = gcnew array<int>(outMapX * outMapY);
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
	
	int CLR_IP::Cpp_FindDominantIntensity(array<byte>^ pSrcImg, array<byte>^ pMaskImg, int  nMemW, int  nMemH)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pMask = &pMaskImg[0];
	
		int nThreshold = IP::FindDominantIntensity(pSrc, pMask, nMemW, nMemH);

		pSrc = nullptr;
		pMask = nullptr;

		return nThreshold;
	}

	// Image(Feature/Defect Image) Load/Save
	void CLR_IP::Cpp_SaveBMP(System::String^ strFilePath, array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nByteCnt)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		IP::SaveBMP(sFilePath, pSrc, nMemW, nMemH, nByteCnt);
		pSrc = nullptr;
	}
	void CLR_IP::Cpp_LoadBMP(System::String^ strFilePath, array<byte>^ pOutImg, int  nMemW, int  nMemH, int nByteCnt)
	{
		pin_ptr<byte> pOut = &pOutImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		IP::LoadBMP(sFilePath, pOut, nMemW, nMemH, nByteCnt);
		pOut = nullptr;
	}

	void CLR_IP::Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int nMemW, int nMemH, Cpp_Rect^ DefectRect, int imageNum)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		sFilePath += std::to_string(imageNum) + ".BMP";

		Rect defectRect = Rect(DefectRect->x, DefectRect->y, DefectRect->w, DefectRect->h);
		IP::SaveDefectListBMP(sFilePath, pSrc, nMemW, nMemH, defectRect);

		pSrc = nullptr;
	}
	void CLR_IP::Cpp_SaveDefectListBMP(System::String^ strFilePath, byte* pSrcImg, int nMemW, int nMemH, array<Cpp_Rect^>^ DefectRect)
	{
		if (DefectRect->Length == 0)
			return;

		pin_ptr<byte> pSrc = &pSrcImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);

		std::vector<Rect> defectRect;
		for (int i = 0; i < DefectRect->Length; i++)
			defectRect.push_back(Rect(DefectRect[i]->x, DefectRect[i]->y, DefectRect[i]->w, DefectRect[i]->h));

		IP::SaveDefectListBMP(sFilePath, pSrc, nMemW, nMemH, defectRect);

		pSrc = nullptr;
	}
	void CLR_IP::Cpp_SaveDefectListBMP_Color(System::String^ strFilePath, BYTE* pRImg, BYTE* pGImg, BYTE* pBImg, int nMemW, int nMemH, Cpp_Rect^ DefectRect, int imageNum)
	{
		pin_ptr<byte> pR = &pRImg[0];
		pin_ptr<byte> pG = &pGImg[0];
		pin_ptr<byte> pB = &pBImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);
		sFilePath += std::to_string(imageNum) + ".BMP";

		Rect defectRect = Rect(DefectRect->x, DefectRect->y, DefectRect->w, DefectRect->h);
		IP::SaveDefectListBMP_Color(sFilePath, pR, pG, pB, nMemW, nMemH, defectRect);

		pR = nullptr;
		pG = nullptr;
		pB = nullptr;
	}
	void CLR_IP::Cpp_SaveDefectListBMP_Color(System::String^ strFilePath, BYTE* pRImg, BYTE* pGImg, BYTE* pBImg, int nMemW, int nMemH, array<Cpp_Rect^>^ DefectRect)
	{
		if (DefectRect->Length == 0)
			return;

		pin_ptr<byte> pR = &pRImg[0];
		pin_ptr<byte> pG = &pGImg[0];
		pin_ptr<byte> pB = &pBImg[0];
		std::string sFilePath = msclr::interop::marshal_as<std::string>(strFilePath);

		std::vector<Rect> defectRect;
		for (int i = 0; i < DefectRect->Length; i++)
			defectRect.push_back(Rect(DefectRect[i]->x, DefectRect[i]->y, DefectRect[i]->w, DefectRect[i]->h));

		IP::SaveDefectListBMP_Color(sFilePath, pR, pG, pB, nMemW, nMemH, defectRect);

		pR = nullptr;
		pG = nullptr;
		pB = nullptr;
	}

	// ETC.
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
	void CLR_IP::Cpp_GoldenImageReview(array<array<byte>^>^ pSrcImg, array<byte>^ pDstImg, int imgNum, int nMemW, int nMemH)
	{
		using System::Runtime::InteropServices::GCHandle;
		using System::Runtime::InteropServices::GCHandleType;

		pin_ptr<byte> pDst = &pDstImg[0];
		// pin each contained array<int>^
		array<GCHandle>^ pins = gcnew array<GCHandle>(pSrcImg->Length);
		for (int i = 0, i_max = pins->Length; i != i_max; ++i)
			pins[i] = GCHandle::Alloc(pSrcImg[i], GCHandleType::Pinned);

		try
		{
			// get int*s for each contained pinned array<int>^
			array<byte*>^ arrays = gcnew array<byte*>(pins->Length);
			for (int i = 0, i_max = arrays->Length; i != i_max; ++i)
				arrays[i] = static_cast<byte*>(pins[i].AddrOfPinnedObject().ToPointer());

			// pin outer array<int*>^
			pin_ptr<byte*> pin = &arrays[0];

			// pass outer pinned array<int*> to UNumeric::ChangeArray as an int**
			// (note that no casts are necessary in correct code)
			IP::GoldenImageReview(pin, pDst, imgNum, nMemW, nMemH);
		}
		finally
		{
			// unpin each contained array<int>^
			for each (GCHandle pin in pins)
				pin.Free();
			pDst = nullptr;
		}
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
	ResizeSSE m_ResizeSSE[3];
	void CLR_IP::Cpp_CreatInterpolationData(int i, double dXScale, double dXShift, int nWidth)
	{
		m_ResizeSSE[i].CreatInterpolationData(dXScale, dXShift, nWidth);
	}
	void CLR_IP::Cpp_ProcessInterpolation(int i,int  thid, BYTE* pSrcImg, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE* ppTarget, int nXOffset, int nYOffset, int nDir, int nSy, int nEy)
	{
		m_ResizeSSE[i].ProcessInterpolation(thid, pSrcImg, nSrcHeight, nSrcWidth, nFovWidth, ppTarget, nXOffset, nYOffset, nDir, nSy, nEy);
	}
	void CLR_IP::Cpp_ProcessInterpolation(int i,int  thid, BYTE* pSrcImg, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE pTarget)
	{
		m_ResizeSSE[i].ProcessInterpolation(thid, pSrcImg, nSrcHeight, nSrcWidth, nFovWidth, pTarget);
	}

	int CLR_IP::Cpp_FindEdge(array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];

		int nEdge = IP::FindEdge(pSrc, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDir, nSearchLevel);

		pSrc = nullptr;

		return nEdge;
	}

	int CLR_IP::Cpp_FindEdge(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel)
	{
		int nEdge = IP::FindEdge(pSrcImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDir, nSearchLevel);

		return nEdge;
	}

	int CLR_IP::Cpp_FindEdge16bit(array<byte>^ pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];

		int nEdge = IP::FindEdge(pSrc, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDir, nSearchLevel, 2);

		pSrc = nullptr;

		return nEdge;
	}

	int CLR_IP::Cpp_FindEdge16bit(byte* pSrcImg, int  nMemW, int  nMemH, int nROIL, int nROIT, int nROIR, int nROIB, int nDir, int nSearchLevel)
	{
		int nEdge = IP::FindEdge(pSrcImg, nMemW, nMemH, Point(nROIL, nROIT), Point(nROIR, nROIB), nDir, nSearchLevel, 2);

		return nEdge;
	}
}