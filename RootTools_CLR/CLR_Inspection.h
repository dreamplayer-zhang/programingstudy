#pragma once

#include "..\RootTools_Cpp\\Cpp_Demo.h"
#include "..\RootTools_Cpp\\PitSizer.h"
#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
//#include "..\RootTools_Cpp\\Cpp_DB.h"
#include "CLR_InspConnector.h"
#include "..\RootTools_Cpp\\InspectionSurface.h"
#include "..\RootTools_Cpp\\InspectionReticle.h"
#include "DefectData.h"
//#include "InspSurface_Reticle.h"

//
//typedef struct _defectData
//{
//	int NONE = -1;
//	POINT ptPos; // Center 
//	double dArea; // Count of points 
//	POINT ptUnit; // chip die
//	POINT ptSize; // w h 
//	int nClusterID = NONE; // 이웃
//	RECT rtArea;  //외각
//	string sDefectName;
//	int sDCode;
//}DefectData;

namespace RootTools_CLR
{
	public ref class CLR_Inspection
	{
	protected:
		//Cpp_Demo* m_pDemo = nullptr; 
		PitSizer* m_PitSizer = nullptr;
		//Cpp_DB*  m_pDB = nullptr;
		//InspSurface_Reticle* m_Reticle = nullptr;
		CLR_InspConnector* m_InspConn = nullptr;
		CInspectionSurface* pInspSurface = nullptr;
		CInspectionReticle* pInspReticle = nullptr;
	public:
		CLR_Inspection()
		{
			//m_pDemo = new Cpp_Demo();
			m_PitSizer = new PitSizer(2048 * 2048, 1);
			//m_pDB = new Cpp_DB();
			//m_Reticle = new InspSurface_Reticle();
			m_InspConn = new CLR_InspConnector();
			pInspSurface = new CInspectionSurface();
			pInspReticle = new CInspectionReticle();
		}

		virtual ~CLR_Inspection()
		{
			//delete m_pDemo; 
			delete m_PitSizer;
			delete pInspSurface;
			delete pInspReticle;
		}

		/*int Add(int n0, int n1)
		{
			return m_pDemo->Add(n0, n1); 
		}*/

		/*void SetMemory(int nCount, int nByte, int xSize, int ySize, __int64 nAddress)
		{
			m_pDemo->SetMemory(nCount, nByte, xSize, ySize, nAddress); 
		}*/

		/*void SendDefectData(DefectData* pStruct)
		{

		}

		int Test()
		{

			return 1;
		}*/
		array<DefectData^>^ SurfaceInspection(int threadindex, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, bool bDark, bool bAbsolute)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			m_InspConn->GetImagePool("pool", memwidth, memHeight);
			int bufferwidth = memwidth;
			int bufferheight = memHeight;


			//PaintOutline(testrect.left, testrect.top, testrect.right, testrect.bottom, 10000, 5, m_InspConn->GetBuffer(), 10000);
			//pInspSurface->SetParams(m_InspConn->GetBuffer(), testrect, 1, 70, 10, true);
			pInspSurface->SetParams(m_InspConn->GetBuffer(), bufferwidth, bufferheight, targetRect, 1, GV, DefectSize, bDark, threadindex);
			
			//TODO 여기서 이벤트를 올리는 방식으로 변경한다
			//여기 들어올때 이미 한 블럭에 대한 정보가 통째로 넘어오는 것이므로 구조 자체를 변경하여 AddDefect이 발생하는 순간을 여기서 포착하도록 수정한다
			//pInspSurface->Inspection();

			pInspSurface->CheckConditions();

			pInspSurface->CopyImageToBuffer(bDark);//opencv pitsize 가져오기 전까지는 buffer copy가 필요함
			vTempResult = pInspSurface->SurfaceInspection(bAbsolute);//TODO : absolute GV 구현해야함
			
			bool bResultExist = vTempResult.size() > 0;
			array<DefectData^>^ local = gcnew array<DefectData^>(vTempResult.size());

			if (bResultExist)
			{
				for (int i = 0; i < vTempResult.size(); i++)
				{
					local[i] = gcnew DefectData();
					local[i]->nIdx = vTempResult[i].nIdx;
					local[i]->nClassifyCode = vTempResult[i].nClassifyCode;
					local[i]->fSize = vTempResult[i].fSize;
					local[i]->nLength = vTempResult[i].nLength;
					local[i]->nWidth = vTempResult[i].nWidth;
					local[i]->nHeight = vTempResult[i].nHeight;
					local[i]->nInspMode = vTempResult[i].nInspMode;
					local[i]->nFOV = vTempResult[i].nFOV;
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
				}
			}

			//return GetResult();

			//m_InspConn->PrepareRun();
			//m_InspConn->StripRun(testrect, 1);
			//m_InspConn->EndRun();

			return local;


			/*
			POINT m_ptCurrent;
			m_ptCurrent.x = -1;
			m_ptCurrent.y = 0;
			RECT testrect;
			testrect.left = 2000;
			testrect.right = 3000;
			testrect.top = 2000;
			testrect.bottom = 3000;

			//RECT rtR = rtMask + m_ptCurrent;
			RECT rtR;
			rtR.left = testrect.left + m_ptCurrent.x;
			rtR.right = testrect.right + m_ptCurrent.x;
			rtR.top = testrect.top + m_ptCurrent.y;
			rtR.bottom = testrect.bottom + m_ptCurrent.y;

			bool bIP = false;
			bool bIP_2nd = false; // m_pParam->m_bPatternUseInterpolation;			// Large Defect
			int nDarkPitSize = 1; // m_pParamSurface->m_HistogramBased_PatternInsp.m_nDarkPitSize;
			bool bLengthInsp = false; // m_pParamSurface->m_HistogramBased_PatternInsp.m_bLengthInsp;
			int nBandwidth = 10; // m_pParamSurface->m_nHP_Signal_Bandwidth;
			int nIntensity = 120; // m_pParamSurface->m_nHP_Signal_intensity;

			int nBandwidth_Shadow = 10; // m_pParamSurface->m_nHP_Signal_Bandwidth_Shadow;
			int nIntensity_Shadow = 120; // m_pParamSurface->m_nHP_Signal_intensity_Shadow;
			int nBandwidth_OutShadow = 10; // m_pParamSurface->m_nHP_Signal_Bandwidth_OutShadow;
			int nIntensity_OutShadow = 120; // m_pParamSurface->m_nHP_Signal_intensity_OutShadow;

			int nBandwidth_EdgeArea = 10;// m_pParamSurface->m_nHP_Signal_Bandwidth_Edge;
			int nIntensity_EdgeArea = 120; // m_pParamSurface->m_nHP_Signal_intensity_Edge;
			int m_nInspOffset = 10;

		

			int DefectPosX = rtR.left, DefectPosY = rtR.top;
			int nEndX = Width(rtR);
			int nEndY = Height(rtR);
			int nPL = 0;
			BOOL bRst = true;

			double m_nPatternInterpolationOffset = 2;
			if (bIP)
			{
				nEndX = (int)(nEndX * m_nPatternInterpolationOffset);
				nEndY = (int)(nEndY * m_nPatternInterpolationOffset);
			}


			std::string DBPath = "C:/sqlite/db/VSTEMP.sqlite";
			int a = m_pDB->DBCreateVSTemp(DBPath);

			std::string mmfName = "pool";
			wstring t;
			t.assign(mmfName.begin(), mmfName.end());
			HANDLE hMapping;
			hMapping = ::OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, t.c_str());
			LPVOID p = ::MapViewOfFile(hMapping, FILE_MAP_ALL_ACCESS, 0, 0, 10000 * 10000);

			byte* pByte = (byte*)p;

			int nDefectSum = 0;
			int nOutline = 10;
			int nX = 10000;
			int nY = 10000;
			int nGV = 180;
			int inspectindex = -1;
			int nsize = 10;



		
			//for (int i = rtR.left + nOutline; i < rtR.right - nOutline; i++)
			//{
			//	//byte* pPos = &pByte[i * nX + nOutline];
			//	byte* pPos = &pByte[i * nX + nOutline];
			//	for (int j = rtR.top + nOutline; j < rtR.bottom - nOutline; j++, pPos++)
			//	{
			//		pMask = pPos;
			//		pMask++;


			//	}
			//}

			PaintOutline(rtR.left, rtR.top, rtR.right ,rtR.bottom,nY, nOutline, pByte,nX );


			//m_Reticle->CopyImageToBuffer(pByte, nX, rtR, 255, false);
			//nPL = m_Reticle->CalHPPatternGV(nBandwidth, nIntensity, nEndX, nEndY);
			if (nPL > 0)
			{
				RECT insp;
				insp.left = m_nInspOffset;
				insp.top = m_nInspOffset;
				insp.right = nEndX + m_nInspOffset;
				insp.bottom = nEndY + m_nInspOffset;
				POINT pos;
				pos.x = DefectPosX;
				pos.y = DefectPosY;

			//	if (!m_Reticle->InspRect_Dark(1000 + 600, insp,
			//		true, nPL, nDarkPitSize, pos, bLengthInsp, bIP))
			//	{
			//		bRst = false;
			//	}
			}


			//PaintOutline(nY, nOutline, pByte, nX);

			m_pDB->OpenAndBegin(DBPath);
			//for (int i = nOutline; i < nY - nOutline; i++)
			//{
			//	//byte* pPos = &pByte[i * nX + nOutline];
			//	byte* pPos = &pByte[i * nX + nOutline];
			//	for (int j = nOutline; j < nX - nOutline; j++, pPos++)
			//	{

			//		m_Reticle->CopyImageToBuffer(pByte, j, i, nX, rtR, 255, false);
			//		if (nBandwidth > 0)
			//		{
			//			nPL = m_Reticle->CalHPPatternGV(nBandwidth, nIntensity, nEndX, nEndY);
			//			if (nPL > 0)
			//			{
			//				RECT insp;
			//				insp.left = m_nInspOffset;
			//				insp.top = m_nInspOffset;
			//				insp.right = nEndX + m_nInspOffset;
			//				insp.bottom = nEndY + m_nInspOffset;
			//				POINT pos;
			//				pos.x = DefectPosX;
			//				pos.y = DefectPosY;



			//				if (!m_Reticle->InspRect_Dark(1000 + 600, insp,
			//					true, nPL, nDarkPitSize, pos, bLengthInsp, bIP))
			//				{
			//					bRst = false;
			//				}
			//			}
			//		}
			//	}
			//}
			m_pDB->CommitAndClose();
			nDefectSum = 0;

			return a;
			*/

		}
		array<DefectData^>^ StripInspection(int threadindex, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, int nIntensity, bool nBandwidth)
		{
			RECT targetRect;
			std::vector<DefectDataStruct> vTempResult;

			targetRect.left = RoiLeft;
			targetRect.right = RoiRight;
			targetRect.top = RoiTop;
			targetRect.bottom = RoiBottom;

			m_InspConn->GetImagePool("pool", memwidth, memHeight);
			int bufferwidth = memwidth;
			int bufferheight = memHeight;

			pInspReticle->SetParams(m_InspConn->GetBuffer(), bufferwidth, bufferheight, targetRect, 1, threadindex, GV, DefectSize);
			pInspReticle->CheckConditions();

			pInspReticle->CopyImageToBuffer(true);//opencv pitsize 가져오기 전까지는 buffer copy가 필요함
			vTempResult = pInspReticle->StripInspection(nBandwidth, nIntensity);

			bool bResultExist = vTempResult.size() > 0;
			array<DefectData^>^ local = gcnew array<DefectData^>(vTempResult.size());

			return local;
		}
		void PaintOutline(int nY, int nOutline, byte* pByte, int nX)
		{
			for (int i = 0; i < nY; i++)
			{
				for (int j = 0; j < nOutline; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = 0; i < nY; i++)
			{
				for (int j = nX - nOutline; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = 0; i < nOutline; i++)
			{
				for (int j = 0; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = nY - nOutline; i < nY; i++)
			{
				for (int j = 0; j < nX; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
		}

		void PaintOutline(int startx, int starty, int endx, int endy, int nY, int nOutline, byte* pByte, int nX)
		{
			for (int i = starty; i < endy; i++)
			{
				for (int j = startx; j < startx + nOutline; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = starty; i < endy; i++)
			{
				for (int j = endx - nOutline; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = starty; i < starty + nOutline; i++)
			{
				for (int j = startx; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}
			for (int i = endy - nOutline; i < endy; i++)
			{
				for (int j = startx; j < endx; j++)
				{
					pByte[i * nX + j] = 255;
				}
			}

		}

		int Width(RECT rt)
		{
			int width = rt.right - rt.left;

			width = abs(width);

			return width;
		}
		int Height(RECT rt)
		{
			int Height = rt.bottom - rt.top;

			Height = abs(Height);

			return Height;
		}

		/*bool OpenImage(std::string FilePath)
		{
			return m_pDemo->OpenImage(FilePath);
		}*/
	};
}

