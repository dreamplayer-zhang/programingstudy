#include "pch.h"
#include "CLR_3D.h"
#include <msclr\marshal_cppstd.h>

#pragma warning(disable: 4244)
#pragma warning(disable: 4267)
#pragma warning(disable: 4793)

namespace RootTools_CLR
{
		CLR_3D::CLR_3D()
		{
			m_pRaw3DMgr = new Raw3DManager();
			m_pRaw3DMgr->GetRawData()->Initialize(CSize(200, 200), CSize(200, 200), 100, 1000);
			m_pRaw3DMgr->Initialize(NULL, ConvertedImage, 200, 200, CSize(200, 200), 100);
		}
		CLR_3D::~CLR_3D()
		{

		}
		byte** CLR_3D::GetFGBuffer()
		{
			return m_pRaw3DMgr->GetRawBuffFG();
		}
		byte** CLR_3D::GetRawBuffer()
		{
			return m_pRaw3DMgr->GetRawData()->GetRawBuffer();
		}
		void CLR_3D::MakeImage(ConvertMode convertMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos
			, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param)
		{
			m_pRaw3DMgr->MakeImage(convertMode, calcMode, displayMode, ptDataPos, nMinGV1, nMinGV2, nThreadNum, nSnapFrameNum, nOverlapStartPos, nOverlapSize
				, nDisplayOffsetX, nDisplayOffsetY, bRevScan, bUseMinGV2, pnCurrFrameNum, param);
		}
}