#pragma once
#include <Windows.h>
#include <stdio.h>
#include <string>

namespace RootTools_CLR
{
	//Box 단위로 들어온 surface 검사 구현 및 db연동화
	ref class CLR_InspConnector
	{
		public:
			CLR_InspConnector(int processornum);//검사 멀티쓰레드로 돌릴 시 각 쓰레드별 할당된 번호(visionworks처럼)
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
