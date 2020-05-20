#include "pch.h"
#include "CLR_InspConnector.h"

namespace RootTools_CLR
{
	CLR_InspConnector::CLR_InspConnector(int processornum)
	{
		ProcessorNum = processornum;
		SetInitData();
	}
	void CLR_InspConnector::SetInitData()
	{
		ImgPool_Width = 40000;
		ImgPool_Height = 40000;
	}
	CLR_InspConnector::~CLR_InspConnector()
	{
	}

	byte* CLR_InspConnector::GetBuffer()
	{
		return (byte*)ImgPool;
	}

	void CLR_InspConnector::GetImagePool(std::string memoryname, int pool_w, int pool_h)
	{
		ImgPool_Width = pool_w;
		ImgPool_Height = pool_h;
		std::string mmfName = memoryname;
		std::wstring t;
		t.assign(mmfName.begin(), mmfName.end());
		HANDLE hMapping;
		hMapping = ::OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, t.c_str());
		ImgPool = ::MapViewOfFile(hMapping, FILE_MAP_ALL_ACCESS, 0, 0, ImgPool_Width * ImgPool_Height);


	}
	/// <summary>
	/// 실제 suface 검사를 진행
	/// InspArea : 검사 진행할 block의 좌표 정보
	/// areainfo : 위 block의 area info (예 : shadow area)
	/// areainfo : 1 normal, 2 shadow, 3 outshadow, 4 edge
	/// </summary>
	/*bool CLR_InspConnector::StripRun(RECT insparea, int areainfo)
	{
		byte* pByte = (byte*)ImgPool;

		RECT rtR;
		rtR.left = insparea.left + m_ptCurrent.x;
		rtR.right = insparea.right + m_ptCurrent.x;
		rtR.top = insparea.top + m_ptCurrent.y;
		rtR.bottom = insparea.bottom + m_ptCurrent.y;

		int DefectPosX = rtR.left, DefectPosY = rtR.top;
		int nEndX = Width(rtR);
		int nEndY = Height(rtR);
		int nPL = 0;
		BOOL bRst = true;

		//2nd interpolation visionworks 참조해서 구현
		if (bIP)
		{
			nEndX = (int)(nEndX * nPatternInterpolationOffset);
			nEndY = (int)(nEndY * nPatternInterpolationOffset);
		}

		m_InspReticle->CopyImageToBuffer(pByte, ImgPool_Width, rtR, 255, bIP);

		RECT insp;
		insp.left = nInspOffset;
		insp.top = nInspOffset;
		insp.right = nEndX + nInspOffset;
		insp.bottom = nEndY + nInspOffset;
		POINT pos;
		pos.x = DefectPosX;
		pos.y = DefectPosY;

		if (areainfo == 1)
		{
			nPL = m_InspReticle->CalHPPatternGV(nBandwidth, nIntensity, nEndX, nEndY);
			if (nPL > 0)
			{
				if (!m_InspReticle->InspRect_Dark(dDcode_strip + 1000, insp,
					true, nPL, nDarkBandwidthPitSize, pos, bLengthInsp, bIP))
				{
					bRst = false;
				}
			}

			nPL = nDarkPitLevel;
			if (nPL > 0)
			{
				m_InspReticle->CopyImageToBuffer(pByte, ImgPool_Width, rtR, 255, bIP);	//이전에 copy된 것은 defect부분이 255로 들어가서 새로 복사해줘야함
				if (!m_InspReticle->InspRect_Dark(dDcode_strip + 500, insp,
					true, nPL, nDarkPitSize, pos, bLengthInsp, bIP))
				{
					bRst = false;
				}
			}
		}



		return bRst;
	}
	bool CLR_InspConnector::SurfaceInsp()
	{
		bool result;




		return true;
	}*/
	int CLR_InspConnector::Width(RECT rect)
	{
		int w;

		w = rect.right - rect.left;
		w = abs(w);

		return w;
	}
	int CLR_InspConnector::Height(RECT rect)
	{
		int h;

		h = rect.bottom - rect.top;
		h = abs(h);

		return h;
	}



}