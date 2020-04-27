#pragma once
#include "InspectionBase.h"

class InspectionSurface : public InspectionBase
{
public:
	void SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi,  int defectCode, int grayLevel, int defectSize, bool bDarkInspection, int threadindex);
	std::vector<DefectDataStruct> Inspection(bool nAbsolute, bool bIsDarkInsp);
	InspectionSurface();
	~InspectionSurface();
};