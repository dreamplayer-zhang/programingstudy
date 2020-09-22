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

	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, array<byte>^ pTempImg, int& nOutPosX, int& nOutPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod);

		nOutPosX = Pos.x;
		nOutPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}
	float CLR_IP::Cpp_TemplateMatching(byte* pSrcImg, byte* pTempImg, int& nOutPosX, int& nOutPosY, int  nMemW, int  nMemH, int nTempW, int nTempH, int nROIL, int nROIT, int nROIR, int nROIB, int nMethod)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0];
		pin_ptr<byte> pTemp = &pTempImg[0];
		Point Pos;

		float score = IP::TemplateMatching(pSrc, pTemp, Pos, nMemW, nMemH, nTempW, nTempH, Point(nROIL, nROIT), Point(nROIR, nROIB), nMethod);

		nOutPosX = Pos.x;
		nOutPosY = Pos.y;

		pSrc = nullptr;
		pTemp = nullptr;

		return score;
	}
	/*
	//void CLR_IP::Cpp_Labeling(array<byte>^ pSrcImg, array<byte>^ pBinImg, int  nMemWidth, int  nMemHeight, bool bDark, [Out] array<Cpp_LabelParam^>^ outLabel)
	//{
	//	pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
	//	pin_ptr<byte> pBin = &pBinImg[0];

	//	std::vector<LabeledData> vtLabeled;

	//	IP::Labeling(pSrc, pBin, vtLabeled, nMemWidth, nMemHeight, bDark);

	//	array<Cpp_LabelParam^>^ local = gcnew array<Cpp_LabelParam^>(vtLabeled.size());

	//	pSrc = nullptr;  // unpin
	//	pBin = nullptr;

	//	bool bResultExist = vtLabeled.size() > 0;

	//	if (bResultExist)
	//	{
	//		for (int i = 0; i < vtLabeled.size(); i++)
	//		{
	//			local[i] = gcnew Cpp_LabelParam();
	//			local[i]->centerX = vtLabeled[i].center.x;
	//			local[i]->centerY = vtLabeled[i].center.y;

	//			local[i]->boundTop = vtLabeled[i].bound.top;
	//			local[i]->boundBottom = vtLabeled[i].bound.bottom;
	//			local[i]->boundLeft = vtLabeled[i].bound.left;
	//			local[i]->boundRight = vtLabeled[i].bound.right;

	//			local[i]->area = vtLabeled[i].area;
	//			local[i]->value = vtLabeled[i].value;
	//		}
	//	}
	//	outLabel = local;
	//}
	*/
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
}