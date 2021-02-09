#include "pch.h"
#include "ResizeSSE.h"

#pragma warning(disable: 4244)

ResizeSSEData::ResizeSSEData()
{
	memset(m_bimg, 0, sizeof(byte) * MAX_FOV);
	memset(m_aimg, 0, sizeof(byte) * MAX_FOV);
}
ResizeSSEData::~ResizeSSEData()
{

}

void ResizeSSE::CreatInterpolationData(double dXScale, double dXShift, int nWidth)
{
	double xx = 0;
	int xi = 0;
	double xa = 0;
	int ngugan = 0;
	int ngugansize = 0;
	memset(m_gugan, 0, sizeof(int) * nWidth);
	memset(m_gugansize, 0, sizeof(int) * nWidth);
	m_gugan[ngugan++] = 0;
	if(dXShift > 1)  dXShift - 1;//  -1 ~ +1 넘을때는 추가 구현 해야함. 
	if(dXShift < -1) dXShift + 1;

	for (int j = 0, k = 0; j < nWidth; j++, k++) //1. 스케일 별로 한번만 계산 
	{
		xx = (double)(j- dXShift) / dXScale;

		xa = xx - (int)xx;
		m_xa8[j] = xa * 32768; //2^7     <<7
		xi = (int)xx;
		if (k != xi)
		{
			k = xi;
			m_gugan[ngugan++] = xi;
			m_gugansize[ngugan - 1]++;
		}
		else
		{
			m_gugansize[ngugan - 1]++;
		}
	}

}

void ResizeSSE::ProcessInterpolation(int thid, LPBYTE pSrc, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE pTarget)
{
	int blockEndWidth = nFovWidth / 16;
	int WidthEnd = nFovWidth % 16;
	ResizeSSEData* sid = &m_SID[thid];
	
	int idx = 0;
	for (int j = 0; m_gugansize[j] != 0; j++)
	{
		memcpy(&sid->m_aimg[idx], &pSrc[m_gugan[j]], m_gugansize[j]);
		memcpy(&sid->m_bimg[idx], &pSrc[m_gugan[j] + 1], m_gugansize[j]);
		idx += m_gugansize[j];
	}

	sid->pRst = (__m128i*)(pTarget);
	sid->pa = (__m128i*) & sid->m_aimg[0];
	sid->pb = (__m128i*) & sid->m_bimg[0];
	sid->pX = (__m128i*)(&m_xa8[0]);
	sid->pX2 = (__m128i*)(&m_xa8[8]);
	for (int j = 0; j < blockEndWidth; j++, sid->pa++, sid->pb++, sid->pX += 2, sid->pX2 += 2, sid->pRst++)
	{
		sid->a = _mm_loadu_si128(sid->pa);
		sid->b = _mm_loadu_si128(sid->pb);

		sid->ah = _mm_unpackhi_epi8(sid->a, ZeroData);// 16비트로 업
		sid->al = _mm_unpacklo_epi8(sid->a, ZeroData);
		sid->bh = _mm_unpackhi_epi8(sid->b, ZeroData);
		sid->bl = _mm_unpacklo_epi8(sid->b, ZeroData);

		sid->subhi = _mm_sub_epi16(sid->bh, sid->ah); // b-a
		sid->sublo = _mm_sub_epi16(sid->bl, sid->al);
		sid->shifth = _mm_slli_epi16(sid->subhi, 1); //*2
		sid->shiftl = _mm_slli_epi16(sid->sublo, 1);

		sid->X = _mm_loadu_si128(sid->pX); // 미리만든 곱하기 인자
		sid->X2 = _mm_loadu_si128(sid->pX2);

		sid->mulhi = _mm_mulhi_epi16(sid->shifth, sid->X2); // 곱하기하면서 상위 16비트만 가져옴		
		sid->mullo = _mm_mulhi_epi16(sid->shiftl, sid->X);

		sid->addh = _mm_add_epi16(sid->mulhi, sid->ah); // + a
		sid->addl = _mm_add_epi16(sid->mullo, sid->al);
		sid->add = _mm_packus_epi16(sid->addl, sid->addh);

		_mm_storeu_si128(sid->pRst, sid->add);

	}
	if (WidthEnd != 0)
	{
		sid->a = _mm_loadu_si128(sid->pa);
		sid->b = _mm_loadu_si128(sid->pb);

		sid->ah = _mm_unpackhi_epi8(sid->a, ZeroData);// 16비트로 업
		sid->al = _mm_unpacklo_epi8(sid->a, ZeroData);
		sid->bh = _mm_unpackhi_epi8(sid->b, ZeroData);
		sid->bl = _mm_unpacklo_epi8(sid->b, ZeroData);

		sid->subhi = _mm_sub_epi16(sid->bh, sid->ah); // b-a
		sid->sublo = _mm_sub_epi16(sid->bl, sid->al);
		sid->shifth = _mm_slli_epi16(sid->subhi, 1); //*2
		sid->shiftl = _mm_slli_epi16(sid->sublo, 1);

		sid->X = _mm_loadu_si128(sid->pX); // 미리만든 곱하기 인자
		sid->X2 = _mm_loadu_si128(sid->pX2);

		sid->mulhi = _mm_mulhi_epi16(sid->shifth, sid->X2); // 곱하기하면서 상위 16비트만 가져옴		
		sid->mullo = _mm_mulhi_epi16(sid->shiftl, sid->X);

		sid->addh = _mm_add_epi16(sid->mulhi, sid->ah); // + a
		sid->addl = _mm_add_epi16(sid->mullo, sid->al);
		sid->add = _mm_packus_epi16(sid->addl, sid->addh);

		for (int j = 0; j < WidthEnd; j++)
			sid->pRst->m128i_i8[j] = sid->add.m128i_i8[j];
	}

}
void ResizeSSE::ProcessInterpolation(int thid, LPBYTE pSrc, int nSrcHeight, int nSrcWidth, int nFovWidth, LPBYTE* ppTarget, int nXOffset, int nYOffset, int nDir, int nSy, int nEy)
{
	int blockEndWidth = nFovWidth / 16;
	int WidthEnd = nFovWidth % 16;
	ResizeSSEData* sid = &m_SID[thid];
	for (int i = nSy; i < nEy; i++)
	{
		if (nYOffset + i * nDir < 0)
		{
			continue;
		}
		int idx = 0;
		for (int j = 0; m_gugansize[j] != 0; j++)
		{
			memcpy(&sid->m_aimg[idx], &pSrc[m_gugan[j] + i * nSrcWidth], m_gugansize[j]);
			memcpy(&sid->m_bimg[idx], &pSrc[m_gugan[j] + 1 + i * nSrcWidth], m_gugansize[j]);
			idx += m_gugansize[j];
		}

		sid->pRst = (__m128i*)(ppTarget[nYOffset + i * nDir] + nXOffset);// m_ppTargetMemAddress[nIndex + i*m_nDirection] + m_nImagePos
		sid->pa = (__m128i*) & sid->m_aimg[0];
		sid->pb = (__m128i*) & sid->m_bimg[0];
		sid->pX = (__m128i*)(&m_xa8[0]);
		sid->pX2 = (__m128i*)(&m_xa8[8]);
		for (int j = 0; j < blockEndWidth; j++, sid->pa++, sid->pb++, sid->pX += 2, sid->pX2 += 2, sid->pRst++)
		{
			sid->a = _mm_loadu_si128(sid->pa);
			sid->b = _mm_loadu_si128(sid->pb);

			sid->ah = _mm_unpackhi_epi8(sid->a, ZeroData);// 16비트로 업
			sid->al = _mm_unpacklo_epi8(sid->a, ZeroData);
			sid->bh = _mm_unpackhi_epi8(sid->b, ZeroData);
			sid->bl = _mm_unpacklo_epi8(sid->b, ZeroData);

			sid->subhi = _mm_sub_epi16(sid->bh, sid->ah); // b-a
			sid->sublo = _mm_sub_epi16(sid->bl, sid->al);
			sid->shifth = _mm_slli_epi16(sid->subhi, 1); //*2
			sid->shiftl = _mm_slli_epi16(sid->sublo, 1);

			sid->X = _mm_loadu_si128(sid->pX); // 미리만든 곱하기 인자
			sid->X2 = _mm_loadu_si128(sid->pX2);

			sid->mulhi = _mm_mulhi_epi16(sid->shifth, sid->X2); // 곱하기하면서 상위 16비트만 가져옴		
			sid->mullo = _mm_mulhi_epi16(sid->shiftl, sid->X);

			sid->addh = _mm_add_epi16(sid->mulhi, sid->ah); // + a
			sid->addl = _mm_add_epi16(sid->mullo, sid->al);
			sid->add = _mm_packus_epi16(sid->addl, sid->addh);

			_mm_storeu_si128(sid->pRst, sid->add);

		}
		if (WidthEnd != 0)
		{
			sid->a = _mm_loadu_si128(sid->pa);
			sid->b = _mm_loadu_si128(sid->pb);

			sid->ah = _mm_unpackhi_epi8(sid->a, ZeroData);// 16비트로 업
			sid->al = _mm_unpacklo_epi8(sid->a, ZeroData);
			sid->bh = _mm_unpackhi_epi8(sid->b, ZeroData);
			sid->bl = _mm_unpacklo_epi8(sid->b, ZeroData);

			sid->subhi = _mm_sub_epi16(sid->bh, sid->ah); // b-a
			sid->sublo = _mm_sub_epi16(sid->bl, sid->al);
			sid->shifth = _mm_slli_epi16(sid->subhi, 1); //*2
			sid->shiftl = _mm_slli_epi16(sid->sublo, 1);

			sid->X = _mm_loadu_si128(sid->pX); // 미리만든 곱하기 인자
			sid->X2 = _mm_loadu_si128(sid->pX2);

			sid->mulhi = _mm_mulhi_epi16(sid->shifth, sid->X2); // 곱하기하면서 상위 16비트만 가져옴		
			sid->mullo = _mm_mulhi_epi16(sid->shiftl, sid->X);

			sid->addh = _mm_add_epi16(sid->mulhi, sid->ah); // + a
			sid->addl = _mm_add_epi16(sid->mullo, sid->al);
			sid->add = _mm_packus_epi16(sid->addl, sid->addh);

			for (int j = 0; j < WidthEnd; j++)
				sid->pRst->m128i_i8[j] = sid->add.m128i_i8[j];
		}
	}
}