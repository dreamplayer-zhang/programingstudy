#pragma once
#include "pch.h"
#include "TypeDefines.h"


typedef struct _SurfaceParam
{
	bool bDark;
	int nThreshold;
	int nDefectSize;
} SurfaceParam;


class InspSurface
{
private:
	BYTE* m_pBuffer;
	int m_nWidth;
	int m_nHeight;
	int m_nThreshold;
	std::vector<LabeledData> m_vtLabeledData;

	//
	bool SurfaceDark();
	bool SurfaceBright();

public:
	bool DoInsp(void* param);
	
};

