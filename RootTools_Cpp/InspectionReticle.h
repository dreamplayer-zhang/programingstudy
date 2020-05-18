#pragma once
#include "InspectionSurface.h"

class CInspectionReticle : public CInspectionSurface
{
public:
	void SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi, int defectCode, int threadindex, int nGV, int nSize);
	std::vector<DefectDataStruct> StripInspection(int nBandwidth, int nIntensity);
	CInspectionReticle();
	~CInspectionReticle();
	int CalHPPatternGV(int nBandwidth, int nIntensity, int nW, int nH);
private:
	int m_Histogram[256];
};

