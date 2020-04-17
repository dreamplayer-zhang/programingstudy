#include "pch.h"
#include "InspectionSurface.h"

bool InspectionSurface::Inspection()
{
	CheckConditions();
	
	if (GetDarkInspection())
	{
		CopyImageToBuffer();//opencv pitsize 가져오기 전까지는 buffer copy가 필요함
		InspectionDark();
	}
	else
	{
		InspectionBright();
	}

	return GetResult();
}
void InspectionSurface::EndInspection(int threadidx)
{
	EraseDB(threadidx);


}

void InspectionSurface::SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi, int defectCode, int grayLevel, int defectSize, bool bDarkInspection,int threadindex)
{
	SetBuffer(buffer);
	SetBufferWidthHeight(bufferwidth, bufferheight);
	SetROI(roi);
	SetDefectCode(defectCode);
	SetGrayLevel(grayLevel);
	SetDefectSize(defectSize);
	SetIsDarkInspection(bDarkInspection);
	OpenDB(threadindex);
}
void InspectionSurface::InspectionDark()
{
	bool bDarkResut = true;
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
			if (*pPos < nGrayLevel)
			{
				//ret = GetPitsizer()->GetPitSize(pPos, x, y, 10000, nGrayLevel, nGrayLevel, 1, false);
				int col = sizeof(inspbuffer[0]) / sizeof(byte);
				ret = GetPitsizer()->GetPitSize(*inspbuffer, x, y, col, nGrayLevel, nGrayLevel, 1, false);
				
				if (GetLengthInspection())
				{
					rt = GetPitsizer()->GetPitRect();
					if (Functions::GetWidth(rt) >= GetDefectSize() || Functions::GetHeight(rt) >= GetDefectSize())
					{
						AddDefect(rt, ptDefectPos, ret);
					}
				}
				else if (ret >= GetDefectSize())
				{
					rt = GetPitsizer()->GetPitRect();

					AddDefect(rt, ptDefectPos, ret);
				}
			}
		}
	}

	// 임시
	CloseDB();
	SetResult(bDarkResut);
}

void InspectionSurface::InspectionBright()
{
	bool bBrightResult = false;

	SetResult(bBrightResult);
}

InspectionSurface::InspectionSurface()
{
	SetDefectCode(1);
}

InspectionSurface::~InspectionSurface()
{
}