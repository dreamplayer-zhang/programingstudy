
#include "pch.h"
#include "IP.h"
#include "InspSurface.h"


bool InspSurface::DoInsp(void* _param)
{
	SurfaceParam* param = static_cast<SurfaceParam*>(_param);

	bool bRst = true;

	bRst = SurfaceDark();

	bRst = bRst & SurfaceBright();

	return bRst;
}

bool InspSurface::SurfaceDark()
{
	BYTE *pBin = nullptr, *pDst = nullptr;

	IP::Threshold(m_pBuffer, m_nWidth, m_nHeight, m_nThreshold, true, pBin);
	IP::Labeling(m_pBuffer, pBin, m_nWidth, m_nHeight, true, pDst, m_vtLabeledData);

	return (m_vtLabeledData.size() == 0);
}

bool InspSurface::SurfaceBright()
{
	BYTE* pBin = nullptr, * pDst = nullptr;

	IP::Threshold(m_pBuffer, m_nWidth, m_nHeight, m_nThreshold, false, pBin);
	IP::Labeling(m_pBuffer, pBin, m_nWidth, m_nHeight, false, pDst, m_vtLabeledData);

	return (m_vtLabeledData.size() == 0);
}