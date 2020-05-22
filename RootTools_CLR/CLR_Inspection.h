#pragma once
#include "..\RootTools_Cpp\\PitSizer.h"
#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>
#include <list>
#include "CLR_InspConnector.h"
#include "..\RootTools_Cpp\\InspectionSurface.h"
#include "..\RootTools_Cpp\\InspectionReticle.h"
#include "DefectData.h"

namespace RootTools_CLR
{
	public ref class CLR_Inspection
	{
	protected:
		CLR_InspConnector^ m_InspConn = nullptr;
		CInspectionSurface* pInspSurface = nullptr;
		CInspectionReticle* pInspReticle = nullptr;
	public:
		CLR_Inspection(int nThreadNum, int nROIWidth, int nROIHeight)
		{
			m_InspConn = gcnew CLR_InspConnector(nThreadNum);
			pInspSurface = new CInspectionSurface(nROIWidth, nROIHeight);
			pInspReticle = new CInspectionReticle(nROIWidth, nROIHeight);
		}

		virtual ~CLR_Inspection()
		{
			delete pInspSurface;
			delete pInspReticle;
		}

		array<DefectData^>^ SurfaceInspection(int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, bool bDark, bool bAbsolute)
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
					local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
					local[i]->fSize = vTempResult[i].fSize;
					local[i]->nLength = vTempResult[i].nLength;
					local[i]->nWidth = vTempResult[i].nWidth;
					local[i]->nHeight = vTempResult[i].nHeight;
					local[i]->nFOV = vTempResult[i].nFOV;
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
				}
			}

			return local;
		}
		array<DefectData^>^ StripInspection(int threadindex, int nDefectCode, int RoiLeft, int RoiTop, int RoiRight, int RoiBottom, int  memwidth, int  memHeight, int GV, int DefectSize, int nIntensity, int nBandwidth)
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

			if (bResultExist)
			{
				for (int i = 0; i < vTempResult.size(); i++)
				{
					local[i] = gcnew DefectData();
					local[i]->nIdx = vTempResult[i].nIdx;
					local[i]->nClassifyCode = nDefectCode;//vTempResult[i].nClassifyCode;
					local[i]->fSize = vTempResult[i].fSize;
					local[i]->nLength = vTempResult[i].nLength;
					local[i]->nWidth = vTempResult[i].nWidth;
					local[i]->nHeight = vTempResult[i].nHeight;
					local[i]->nFOV = vTempResult[i].nFOV;
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//데이터를 던져주기 직전에 rect의 top/left 정보를 더해서 던져준다
				}
			}

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
	};
}

