#pragma once
#include "../RootTools_CLR/CCLRD2DStructure.h"
#include <math.h>

#pragma warning(disable: 4244)

namespace RootTools_CLR
{
	public ref class CCLRD2DModule
	{
	public:
		MyClrTest* test;
		CCLRD2DStructure strc;
		CCLRD2DModule()
		{
			test = new MyClrTest();

			strc.strc = new D2DChipStruc();
			strc.target = new D2DPtrStruc();
			strc.pChipTarget = new byte();
			strc.ref = new D2DPtrStruc();
			strc.ResultABS = new D2DPtrStruc();
			
		}
		~CCLRD2DModule()
		{
			delete test;
		}

	
		void SetDieInfo(int _TargetNumX, int _TargetNumY, int _RefNumX, int _RefNumY)
		{
			strc.strc->TargetNumX = _TargetNumX;
			strc.strc->TargetNumY = _TargetNumY;
			strc.strc->RefNumX= _RefNumX;
			strc.strc->RefNumY= _RefNumY;
		}

		void SetRectInfo(int _RectNumX, int _RectNumY, int _CurrentPosX, int _CurrentPosY)
		{
			strc.strc->RectNumX = _RectNumX;
			strc.strc->RectNumY = _RectNumY;
			strc.strc->CurrentPosX = _CurrentPosX;
			strc.strc->CurrentPosY = _CurrentPosY;
		}

		void SetTargetInfo( int _nPtrWidth, int _nHeight, int _nWidth)
		{
			strc.target->nPtrWidth = _nPtrWidth;
			strc.target->nHeight = _nHeight;
			strc.target->nWidth = _nWidth;
		}

		void SetRefInfo( int _nPtrWidth, int _nHeight, int _nWidth)
		{
			//strc.ref->pByte;
			strc.ref->nPtrWidth = _nPtrWidth;
			strc.ref->nHeight = _nHeight;
			strc.ref->nWidth = _nWidth;
		}

		void SetABSResultInfo(int _nPtrWidth, int _nHeight, int _nWidth)
		{
			//strc.ResultABS->pByte;
			strc.ResultABS->nPtrWidth = _nPtrWidth;
			strc.ResultABS->nHeight = _nHeight;
			strc.ResultABS->nWidth = _nWidth;
		}

		void SetWidth(int _TargetWidth, int _RefWidth, int _ABSresultWidth)
		{
			strc.target->nWidth = _TargetWidth;
			strc.ref->nWidth = _RefWidth;
			strc.ResultABS->nWidth = _ABSresultWidth;
		}

		void SetPtrInfo(byte* _target, byte* _ref, byte* _doubleSizeTarget, byte* _ABSresult)
		{
			strc.pChipTarget = _target;
			strc.target->pByte = _doubleSizeTarget;
			strc.ref->pByte = _ref;
			strc.ResultABS->pByte = _ABSresult;
		}
		void AddPtrInfo(int _target, int _ref, int _ABSresult)
		{
			strc.target->pByte += _target;
			strc.ref->pByte += _ref;
			strc.ResultABS->pByte += _ABSresult;
		}

		byte* GetChipTargetPtr()
		{
			return strc.pChipTarget;
		}

		byte* GetDoubleSizeTargetPtr()
		{
			return strc.target->pByte;
		}

		int GetChipWidth()
		{
		//chipwidth == ref width == chiptarget width
			return strc.ref->nWidth;
		}
		
		int GetTargetDieNumX()
		{
			return strc.strc->TargetNumX;
		}

		int GetTargetDieNumY()
		{
			return strc.strc->TargetNumY;
		}


		void D2DInspProto()
		{
			
		}



		void D2DInspChipProto()
		{
			int nTrigger = 3;

			int nIdxX, nIdxY;
			ULONGLONG llSum, llMin=0;
			llSum = test->SSE_GetDiffSum4DoubleSize_256(strc.target, strc.ref);
			int nThreshold = ceil(llSum / (256 * 256));

			for (int k = -2 * nTrigger; k <= 2 * nTrigger; k++)
			{
				for (int l = -2 * nTrigger; l <=2 * nTrigger; l++)
				{
					llSum = test->SSE_GetDiffSum4DoubleSize_256_threshold(strc.target, strc.ref, (byte)nThreshold);

					if (llSum < llMin)
					{
						nIdxX = l;
						nIdxY = k;
						llMin = llSum;
					}
				}
			}

			//부분 ABS이미지 만들기
			test->SSE_MakeABS_Proto(strc.target+nIdxX+nIdxY*256 , strc.ref, strc.ResultABS);

			//부분 defect 찾기


		}

		//ref target 넘버, rect 위치(x,y)를 기본 입력값으로 가져감.
		//가로 세로 256
		// ptr: ref, target , abs
		//
		void D2DInspChipRectProto()
		{
			test->SSE_GetDiffSum4DoubleSize_256(strc.target, strc.ref);
		








		}


		ULONGLONG SSE_GetDiffSum4DoubleSize_256(D2DPtrStruc ref, D2DPtrStruc target)
		{
			//return test->SSE_GetDiffSum4DoubleSize_256(ref, target);
			return 0;
		}
	};


}

