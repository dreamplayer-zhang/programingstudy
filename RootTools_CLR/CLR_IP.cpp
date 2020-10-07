#include "pch.h"

#include "CLR_IP.h"
#include <msclr\marshal_cppstd.h>

namespace RootTools_CLR
{
	void CLR_IP::ContourFitEllipse(array<byte>^ pSrcImg, int nW, int nH, array<byte>^ pDstImg)
	{
		pin_ptr<byte> pSrc = &pSrcImg[0]; // pin : 주소값 고정
		pin_ptr<byte> pDst = &pDstImg[0];

		IP::ContourFitEllipse(pSrc, nW, nH, pDst);

		pSrc = nullptr;  // unpin
		pDst = nullptr;
	}
}