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
			
			//TODO ���⼭ �̺�Ʈ�� �ø��� ������� �����Ѵ�
			//���� ���ö� �̹� �� ���� ���� ������ ��°�� �Ѿ���� ���̹Ƿ� ���� ��ü�� �����Ͽ� AddDefect�� �߻��ϴ� ������ ���⼭ �����ϵ��� �����Ѵ�
			//pInspSurface->Inspection();

			pInspSurface->CheckConditions();

			pInspSurface->CopyImageToBuffer(bDark);//opencv pitsize �������� �������� buffer copy�� �ʿ���
			vTempResult = pInspSurface->SurfaceInspection(bAbsolute);//TODO : absolute GV �����ؾ���
			
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
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//�����͸� �����ֱ� ������ rect�� top/left ������ ���ؼ� �����ش�
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//�����͸� �����ֱ� ������ rect�� top/left ������ ���ؼ� �����ش�
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

			pInspReticle->CopyImageToBuffer(true);//opencv pitsize �������� �������� buffer copy�� �ʿ���
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
					local[i]->fPosX = vTempResult[i].fPosX + targetRect.left;//�����͸� �����ֱ� ������ rect�� top/left ������ ���ؼ� �����ش�
					local[i]->fPosY = vTempResult[i].fPosY + targetRect.top;//�����͸� �����ֱ� ������ rect�� top/left ������ ���ؼ� �����ش�
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

