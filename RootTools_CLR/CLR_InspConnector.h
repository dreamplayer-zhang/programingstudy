#pragma once
#include <Windows.h>
#include <stdio.h>
#include <string>
#include "..\RootTools_Cpp\\InspSurface_Reticle.h"
#include "..\RootTools_Cpp\Cpp_DB.h"

using namespace std;

namespace RootTools_CLR
{
	//Box 단위로 들어온 surface 검사 구현 및 db연동화
	class CLR_InspConnector
	{
		public:
			CLR_InspConnector(int processornum = 0);//검사 멀티쓰레드로 돌릴 시 각 쓰레드별 할당된 번호(visionworks처럼)
			virtual ~CLR_InspConnector();

			InspSurface_Reticle* m_InspReticle;
			Cpp_DB* m_DBmgr;

			void GetImagePool(string memoryname,int pool_w, int pool_h);


			//현재 검사에 대한 parameter set
			//parameter set 방식에 따라 아래 함수 미사용시 제거
			bool SetParam_Common();
			bool SetParam_Strip(bool bip, bool bip_2nd, double bipoffset, int darkpitlevel, int darkpitsize, bool lengthinsp, int bandwidth, int intensity, int bandwidth_shadow, int intensity_shadow, int bandwidth_outshadow, int intensity_outshadow,
				int bandwidth_edge, int intensity_edge, int bandwidth_darkpit, int inspoffset);
			bool SetParam_Normal();

			//편의상 기존 sun에서 구분하는 방식인 strip 검사(bandwidth, pit level 둘 다 사용 검사)
			//normal 검사(pit level 방식만 사용)로 구분
			bool StripRun(RECT insparea, int areainfo);
			bool NormalRun();
			bool SurfaceInsp();

			bool PrepareRun();
			bool EndRun();

			int Width(RECT rect);
			int Height(RECT rect);

			byte* GetBuffer();
	private:
		void SetInitData();
		int ProcessorNum;

		string dbpath = "C:/sqlite/db/VSTEMP";


		LPVOID ImgPool;
		int ImgPool_Width;
		int ImgPool_Height;

		//Common
		POINT m_ptCurrent;

		//Surface Strip Inspection
		//검사에서 쓰는 parameter들 별도 class로 선언해서 inspsurface_reticle에 넘겨주는 식으로 구현
		int dDcode_strip = 10000;

		bool bIP;
		bool bIP_2nd;
		double nPatternInterpolationOffset;

		int nDarkPitLevel;
		int nDarkPitSize;
		bool bLengthInsp;

		int nBandwidth; 
		int nIntensity;
		int nBandwidth_Shadow;
		int nIntensity_Shadow;
		int nBandwidth_OutShadow;
		int nIntensity_OutShadow;
		int nBandwidth_EdgeArea;
		int nIntensity_EdgeArea;
		int nDarkBandwidthPitSize;
		int nInspOffset;
		//여기에도 pit level size구현해야함


		//Surface Noraml Inspection





	};
}
