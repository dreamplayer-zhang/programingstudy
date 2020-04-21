#include "pch.h"
#include "InspectionSurface.h"

void InspectionSurface::SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi, int defectCode, int grayLevel, int defectSize, bool bDarkInspection,int threadindex)
{
	SetBuffer(buffer);
	SetBufferWidthHeight(bufferwidth, bufferheight);
	SetROI(roi);
	SetDefectCode(defectCode);
	SetGrayLevel(grayLevel);
	SetDefectSize(defectSize);
	SetIsDarkInspection(bDarkInspection);
}
std::vector<DefectDataStruct> InspectionSurface::Inspection(bool nAbsolute, bool bIsDartInsp)
{
	std::vector<DefectDataStruct> vResult;

	bool bDarkResut = bIsDartInsp;
	bool bInspResult = false;
	RECT rt;
	RECT rtROI = GetROI();
	RECT rtinspROI = GetInspbufferROI();
	float ret;
	byte* pPos;
	POINT ptDefectPos;
	ptDefectPos.x = rtROI.left;
	ptDefectPos.y = rtROI.top;

	int nWidth = Functions::GetWidth(rtROI);
	int nGrayLevel = GetGrayLevel();

	//opencv 새 알고리즘 전에는 inspoffset line background 255로 칠한 buffer 가지고 해야함
	//for (int y = rtROI.top; y < rtROI.bottom; y++)
	//{
	//	pPos = GetBuffer(rtROI.left, y, 10000);
	for (int y = rtinspROI.top; y < rtinspROI.bottom; y++)
	{
		
		pPos = &inspbuffer[y][rtinspROI.left];
		//pPos = &inspbuffer2[y * col + rtinspROI.left];
		//for (int x = rtROI.left; x < rtROI.right; x++, pPos++)
		for (int x = rtinspROI.left; x < rtinspROI.right; x++, pPos++)
		{
			bool bCheckPoint = *pPos < nGrayLevel;
			if (!bDarkResut)
			{
				bCheckPoint = *pPos > nGrayLevel;
			}

			if (bCheckPoint)
			{
				//ret = GetPitsizer()->GetPitSize(pPos, x, y, 10000, nGrayLevel, nGrayLevel, 1, false);
				int col = sizeof(inspbuffer[0]) / sizeof(byte);
				ret = GetPitsizer()->GetPitSize(*inspbuffer, x, y, col, nGrayLevel, nGrayLevel, 1, false);
				
				if (GetLengthInspection())
				{
					rt = GetPitsizer()->GetPitRect();
					if (Functions::GetWidth(rt) >= GetDefectSize() || Functions::GetHeight(rt) >= GetDefectSize())
					{
						vResult.push_back(GetDefectData(rt, ptDefectPos, ret));
						bInspResult = true;
					}
				}
				else if (ret >= GetDefectSize())
				{
					rt = GetPitsizer()->GetPitRect();

					vResult.push_back(GetDefectData(rt, ptDefectPos, ret));
					bInspResult = true;
				}
			}
		}
	}

	SetResult(bInspResult);
	return vResult;
}

InspectionSurface::InspectionSurface()
{
	SetDefectCode(1);
}

InspectionSurface::~InspectionSurface()
{
}