#pragma once
#include "InspectionBase.h"

class InspectionSurface : public InspectionBase
{
public:
	//virtual bool Inspection() override;
	void EndInspection(int threadidx);
	void SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi,  int defectCode, int grayLevel, int defectSize, bool bDarkInspection, int threadindex);
	vector<DefectInfo> Inspection(bool nAbsolute, bool bIsDartInsp);
	InspectionSurface();
	~InspectionSurface();
};