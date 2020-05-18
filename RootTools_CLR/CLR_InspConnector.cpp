#include "pch.h"
#include "CLR_InspConnector.h"

namespace RootTools_CLR
{
	//CLR_InspConnector::CLR_InspConnector()
	//{
	//	ProcessorNum = 0;
	//	m_InspReticle = new InspSurface_Reticle();
	//	m_DBmgr = new Cpp_DB();
	//	

	//	SetInitData();
	//}
	CLR_InspConnector::CLR_InspConnector(int processornum)
	{
		ProcessorNum = processornum;
		//m_InspReticle = new InspSurface_Reticle(ProcessorNum);
		//m_DBmgr = new Cpp_DB();


		SetInitData();
	}
	void CLR_InspConnector::SetInitData()
	{
		ImgPool_Width = 40000;
		ImgPool_Height = 40000;


		m_ptCurrent.x = -1;
		m_ptCurrent.y = 0;

		/*bIP = false;
		bIP = false;
		nPatternInterpolationOffset = 2.0;

		nDarkPitLevel = 60;
		nDarkPitSize = 5;
		bLengthInsp = false;

		nBandwidth = 10;
		nIntensity = 120;

		nBandwidth_Shadow = 10;
		nIntensity_Shadow = 120;
		nBandwidth_OutShadow = 10;
		nIntensity_OutShadow = 120;

		nBandwidth_EdgeArea = 10;
		nIntensity_EdgeArea = 120;

		nDarkBandwidthPitSize = 5;

		nInspOffset = 10;*/
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
		//memory 이름 하드코딩
		//std::string mmfName = "pool";
		std::string mmfName = memoryname;
		std::wstring t;
		t.assign(mmfName.begin(), mmfName.end());
		HANDLE hMapping;
		hMapping = ::OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, t.c_str());
		ImgPool = ::MapViewOfFile(hMapping, FILE_MAP_ALL_ACCESS, 0, 0, ImgPool_Width * ImgPool_Height);


	}
	/*bool CLR_InspConnector::SetParam_Common()
	{


		return true;
	}*/
	/*bool CLR_InspConnector::SetParam_Strip(bool bip, bool bip_2nd, double bipoffset, int darkpitlevel, int darkpitsize, bool lengthinsp, 
		int bandwidth, int intensity, int bandwidth_shadow, int intensity_shadow, int bandwidth_outshadow, int intensity_outshadow,
		int bandwidth_edge, int intensity_edge, int bandwidth_darkpit, int inspoffset)
	{
		m_ptCurrent.x = -1;
		m_ptCurrent.y = 0;

		bIP = bip;
		bIP = bip_2nd;
		nPatternInterpolationOffset = bipoffset;

		nDarkPitLevel = darkpitlevel;
		nDarkPitSize = darkpitsize;
		bLengthInsp = lengthinsp;
		
		nBandwidth = bandwidth;
		nIntensity = intensity;

		nBandwidth_Shadow = bandwidth_shadow;
		nIntensity_Shadow = intensity_shadow;
		nBandwidth_OutShadow = bandwidth_outshadow;
		nIntensity_OutShadow = intensity_outshadow;

		nBandwidth_EdgeArea = bandwidth_edge;
		nIntensity_EdgeArea = intensity_edge;

		nDarkBandwidthPitSize = bandwidth_darkpit;

		nInspOffset = inspoffset;

		return true;
	}
	bool CLR_InspConnector::SetParam_Normal()
	{


		return true;
	}*/
	/*bool CLR_InspConnector::NormalRun()
	{


		return true;
	}*/
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