#pragma once
#include <Windows.h>
#include <stdio.h>
#include <string>

namespace RootTools_CLR
{
	//Box ������ ���� surface �˻� ���� �� db����ȭ
	ref class CLR_InspConnector
	{
		public:
			CLR_InspConnector(int processornum);//�˻� ��Ƽ������� ���� �� �� �����庰 �Ҵ�� ��ȣ(visionworksó��)
			virtual ~CLR_InspConnector();

			void GetImagePool(std::string memoryname, long offset , int pool_w, int pool_h);

			int Width(RECT rect);
			int Height(RECT rect);

			byte* GetBuffer();
	private:
		void SetInitData();
		int ProcessorNum;

		LPVOID ImgPool;
		int ImgPool_Width;
		int ImgPool_Height;
	};
}
