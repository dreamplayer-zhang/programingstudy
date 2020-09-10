#pragma once
#include "../RootTools_Cpp/CD2DTest.h"

namespace RootTools_CLR
{
	public ref class CCLRD2DStructure
	{
	public:
		D2DChipStruc* strc;
		D2DPtrStruc* target;
		byte* pChipTarget;
		D2DPtrStruc* ref;
		D2DPtrStruc* ResultABS;
		void CCLRD2DStrcTest()
		{
			//strc->RefNumX = 10;
		}
	};
}