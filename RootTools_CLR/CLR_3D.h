#pragma once

#include "windows.h"
#include "memoryapi.h"
#include <stdio.h>

#include <iostream>
#include <time.h>
#include <math.h>
#include <basetsd.h>
#include <atltypes.h>

#define BLOCK_SIZE 1000
#define MAX_SAME_FRAME_COUNT 10000
#include "..\RootTools_Cpp\\Raw3DManager.h"
#pragma warning(disable: 4267)
namespace RootTools_CLR
{
	public class CLR_3D
	{
		
	public:
		CLR_3D();
		virtual ~CLR_3D();

		byte** GetFGBuffer();
		byte** GetRawBuffer();
		void MakeImage(ConvertMode convertMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos
			, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param);

		byte** ConvertedImage;
		Raw3DManager* m_pRaw3DMgr;
	};
}
