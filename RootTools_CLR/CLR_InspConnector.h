#pragma once
#include <Windows.h>
#include <stdio.h>
#include <string>
#include "..\RootTools_Cpp\\InspSurface_Reticle.h"
#include "..\RootTools_Cpp\Cpp_DB.h"

using namespace std;

namespace RootTools_CLR
{
	//Box ������ ���� surface �˻� ���� �� db����ȭ
	class CLR_InspConnector
	{
		public:
			CLR_InspConnector(int processornum = 0);//�˻� ��Ƽ������� ���� �� �� �����庰 �Ҵ�� ��ȣ(visionworksó��)
			virtual ~CLR_InspConnector();

			InspSurface_Reticle* m_InspReticle;
			Cpp_DB* m_DBmgr;

			void GetImagePool(string memoryname,int pool_w, int pool_h);


			//���� �˻翡 ���� parameter set
			//parameter set ��Ŀ� ���� �Ʒ� �Լ� �̻��� ����
			bool SetParam_Common();
			bool SetParam_Strip(bool bip, bool bip_2nd, double bipoffset, int darkpitlevel, int darkpitsize, bool lengthinsp, int bandwidth, int intensity, int bandwidth_shadow, int intensity_shadow, int bandwidth_outshadow, int intensity_outshadow,
				int bandwidth_edge, int intensity_edge, int bandwidth_darkpit, int inspoffset);
			bool SetParam_Normal();

			//���ǻ� ���� sun���� �����ϴ� ����� strip �˻�(bandwidth, pit level �� �� ��� �˻�)
			//normal �˻�(pit level ��ĸ� ���)�� ����
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
		//�˻翡�� ���� parameter�� ���� class�� �����ؼ� inspsurface_reticle�� �Ѱ��ִ� ������ ����
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
		//���⿡�� pit level size�����ؾ���


		//Surface Noraml Inspection





	};
}
