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
	public ref class CLR_3D
	{
		
	public:
		CLR_3D();
		virtual ~CLR_3D();
		void Init3D(LPBYTE ppConvertedImage, WORD* ppBuffHeight, short* ppBuffBright, int szImageBufferX, int szImageBufferY, LPBYTE ppBuffRaw, int szMaxRawImageX, int szMaxRawImageY, int nMaxOverlapSize, int nMaxFrameNum);

		byte** GetFGBuffer();
		byte** GetRawBuffer();
		void MakeImage(ConvertMode convertMode, Calc3DMode calcMode, DisplayMode displayMode, CCPoint ptDataPos
			, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, Parameter3D param);
		void MakeImageSimple(int ptDataPosX, int ptDataPosY
			, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int n3DScanParam, int nSnapFrameRest);
		byte** ConvertedImage;
		Raw3DManager* m_pRaw3DMgr;
		void SetFrameNum(int n);
	};
}
