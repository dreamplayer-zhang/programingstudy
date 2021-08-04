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
			
			
		}
		CLR_3D::~CLR_3D()
		{

		}
		void CLR_3D::Init3D(LPBYTE ppConvertedImage,WORD* ppBuffHeight, short* ppBuffBright,int szImageBufferX, int szImageBufferY, LPBYTE ppBuffRaw, int szMaxRawImageX, int szMaxRawImageY, int nMaxOverlapSize, int nMaxFrameNum)
		{
			m_pRaw3DMgr->GetRawData()->Initialize(ppBuffHeight, ppBuffBright,CSize( szImageBufferX,szImageBufferY), ppBuffRaw,CSize( szMaxRawImageX, szMaxRawImageY), nMaxOverlapSize, nMaxFrameNum);
			m_pRaw3DMgr->Initialize(ppConvertedImage, szImageBufferX, szImageBufferY, CSize(szMaxRawImageX, szMaxRawImageY), nMaxOverlapSize);
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
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, Parameter3D param)
		{
			m_pRaw3DMgr->MakeImage(convertMode, calcMode, displayMode, ptDataPos, nMinGV1, nMinGV2, nThreadNum, nSnapFrameNum, nOverlapStartPos, nOverlapSize
				, nDisplayOffsetX, nDisplayOffsetY, bRevScan, bUseMinGV2, param);
		}
		void CLR_3D::MakeImageSimple(int ptDataPosX,int ptDataPosY,
			 int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
			, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int n3DScanParam,	int nSnapFrameRest)
		{		
			Parameter3D p;
			p.n3DScanParam = n3DScanParam;
			p.nSnapFrameRest = nSnapFrameRest;
			m_pRaw3DMgr->MakeImage(ConvertMode::Bump, Calc3DMode::Normal, DisplayMode::HeightImage, CPoint(ptDataPosX, ptDataPosY), nMinGV1, nMinGV2, nThreadNum, nSnapFrameNum, nOverlapStartPos, nOverlapSize
				, nDisplayOffsetX, nDisplayOffsetY, bRevScan, bUseMinGV2, p);
		}
		void CLR_3D::SetFrameNum(int n)
		{
			m_pRaw3DMgr->SetFrameNum(n);
		}
}