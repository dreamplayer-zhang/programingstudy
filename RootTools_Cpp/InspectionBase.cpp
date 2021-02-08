#include "pch.h"
#include "InspectionBase.h"

#pragma warning(disable: 4244)
#pragma warning(disable: 6385)
#pragma warning(disable: 26451)
#pragma warning(disable: 26495)

DefectDataStruct InspectionBase::GetDefectData(RECT rt, POINT ptDPos, float nArea)
{
	DefectDataStruct data;

	data.nWidth= rt.right - rt.left;
	data.nHeight = rt.bottom - rt.top;
	data.fPosX = rt.left + (data.nWidth * (double)0.5);//중앙값을 구하기 위한 width 더하기
	data.fPosY = rt.top + (data.nHeight * (double)0.5);//중앙값을 구하기 위한 height 더하기
	data.nLength = data.nWidth;
	if (data.nHeight > data.nWidth)
	{
		data.nLength = data.nHeight;
	}
	data.fAreaSize = nArea;
	data.nClassifyCode = GetDefectCode();

	return data;
}
void InspectionBase::SetParams()
{
}
void InspectionBase::CheckConditions() const
{
	//assert(!IsRectEmpty(&rtROI));
	assert(nDefectCode != -1);
	assert(nGrayLevel != -1);
	assert(nDefectSize != -1);
}

void InspectionBase::SetIsDarkInspection(bool bValue)
{
	bDarkInspection = bValue;
}

void InspectionBase::SetResult(bool bValue)
{
	bResult = bValue;
}

byte* InspectionBase::GetBuffer() const
{
	return pBuffer;
}

byte* InspectionBase::GetBuffer(const int x, const int y, const int width) const
{
	return &pBuffer[y * width + x];
}

PitSizer* InspectionBase::GetPitsizer() const
{
	return pPitSizer;
}

RECT InspectionBase::GetROI() const
{
	return rtROI;
}
bool InspectionBase::GetAbsoulteGrayLevel() const
{
	return bAbsoulteGrayLevel;
}
bool InspectionBase::GetLengthInspection() const
{
	return bLengthInspection;
}
bool InspectionBase::GetDarkInspection() const
{
	return bDarkInspection;
}
int InspectionBase::GetDefectCode() const
{
	return nDefectCode;
}
int InspectionBase::GetGrayLevel() const
{
	return nGrayLevel;
}
int InspectionBase::GetDefectSize() const
{
	return nDefectSize;
}
int InspectionBase::GetDefectSizeMax() const
{
	return nDefectSizeMax;
}
int InspectionBase::GetDefectSizeMin() const
{
	return nDefectSizeMin;
}
bool InspectionBase::GetResult() const
{
	return bResult;
}
RECT InspectionBase::GetInspbufferROI() const
{
	return inspbufferROI;
}
void InspectionBase::CopyImageToBuffer(bool bDark)//byte* mem, int nW, RECT rt, int nBackGround, BOOL bInterpolation)
{
	//opencv 전의 test용으로 interpolation 내용 미구현
	byte* mem = pBuffer;
	INT64 nW = nBufferW;//80000;
	RECT rt = rtROI;
	int nBackGround = 255;
	if (!bDark)
	{
		nBackGround = 0;
	}

	
	int nWidth = (int)(Functions::GetWidth(rt) + m_nInspOffset);
	int nHeight = (int)(Functions::GetHeight(rt) + m_nInspOffset);

	inspbufferROI.left = m_nInspOffset;
	inspbufferROI.top = m_nInspOffset;
	inspbufferROI.right = nWidth;
	inspbufferROI.bottom = nHeight;

	INT64 nOffset = m_nInspOffset;

		// Copy Area
	int nStart = 0;
	int nEndX = Functions::GetWidth(rt);
	int nEndY = Functions::GetHeight(rt);

	////버퍼 복사를 이상하게 하고있음. 문제 확인 후 조치 필요함
	//for (INT64 i = nStart; i < nEndY; i++)
	//{
	//	for (INT64 j = nStart; j < nEndX; j++)
	//	{
	//		INT64 xtarget = j + rt.left + (nOffset - 5) * 2;
	//		INT64 ytarget = i + rt.top + (nOffset - 5) * 2;
	//		INT64 iIndex = (ytarget)*nW + (xtarget);
	//		inspbuffer[i + nOffset][nOffset + j] = mem[iIndex];
	//		//mem[iIndex] = 255;//테스트용 코드
	//		//inspbuffer2[(i+nOffset)* nWidth + (nOffset + j)] = mem[(ytarget)*nW + (xtarget)];
	//	}
	//} 

	// 201130 ESCHO
	for (INT64 i = nStart; i < nEndY; i++)
	{
		INT64 ytarget = i + rt.top + (nOffset - 5) * 2;
		INT64 iIndex = (ytarget)*nW + rt.left;
		memcpy(inspbuffer[i + nOffset], &mem[iIndex], nEndX - nStart);
	}


	for (int i = 0; i < m_nInspOffset; i++) {//Jerry
		memset(inspbuffer[i], nBackGround, nWidth);
		memset(inspbuffer[nHeight - 1 - i], nBackGround, nWidth);
		for (int j = 0; j < nHeight; j++) {
			inspbuffer[j][i] = nBackGround;
			inspbuffer[j][nWidth - 1 - i] = nBackGround;
		}
	}

}
InspectionBase::InspectionBase(INT64 nWidth, INT64 nHeight)
{
	bResult = false;

	rtROI = RECT();

	nDefectCode = -1;
	nGrayLevel = -1;
	nDefectSize = -1;
	nDefectSizeMin = -1;
	nDefectSizeMax = -1;

	bAbsoulteGrayLevel = false;
	bLengthInspection = false;
	bDarkInspection = false;
	INT64 sizerSize = nWidth * nHeight;
	INT64 bufSize = nWidth * nHeight;

	pPitSizer = new PitSizer(sizerSize, 1);
	pBuffer = new byte(bufSize);
}

InspectionBase::~InspectionBase()
{
	Functions::SafeDelete(pPitSizer);
	//Functions::SafeDelete(pDataBase);
	//Functions::SafeDeleteArray(pBuffer);
	//pPitSizer = nullptr;
	//pBuffer = nullptr;
}
