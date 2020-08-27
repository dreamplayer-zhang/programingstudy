#include "pch.h"
#include "InspectionBase.h"

DefectDataStruct InspectionBase::GetDefectData(RECT rt, POINT ptDPos, float nArea)
{
	DefectDataStruct data;

	data.nWidth= rt.right - rt.left;
	data.nHeight = rt.bottom - rt.top;
	data.fPosX = rt.left + (data.nWidth * (double)0.5);//중앙값을 구하기 위한 width 더하기
	data.fPosY = rt.top + (data.nHeight * (double)0.5);//중앙값을 구하기 위한 height 더하기
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

	int nOffset = m_nInspOffset;
	//	LPBYTE* ppBuffer = m_ppImageBuffer2;

		// Copy Area
	int nStart = 0, nEndX = Functions::GetWidth(rt), nEndY = Functions::GetHeight(rt);

	//inspbuffer2 = new byte[nEndX * nEndY*6];

	//for (int i = 0; i < nHeight; i++)
	//{
	//	for (int j = 0; j < nWidth; j++)
	//	{
	//		inspbuffer2[i * nW + j] = nBackGround;
	//	}
	//}

	
	
	for (int i = nStart; i < nEndY; i++)
	{
		for (int j = nStart; j < nEndX; j++)
		{
			INT64 ytarget = i + rt.top + (nOffset - 5) * 2;
			INT64 xtarget = j + rt.left + (nOffset - 5) * 2;

			INT64 iIndex = (ytarget)*nW + (xtarget);
			byte* ptr = mem + iIndex;
			inspbuffer[i + nOffset][nOffset + j] = *ptr;	// <-- Index Overflow...?? -ESCHO
			//inspbuffer[i + nOffset][nOffset + j] = mem[iIndex];	// <-- Index Overflow...?? -ESCHO
			//inspbuffer2[(i+nOffset)* nWidth + (nOffset + j)] = mem[(ytarget)*nW + (xtarget)];
		}
	}
	

	//for (int i = 0; i < m_nInspOffset; i++) {//Jerry
	//	memset(&inspbuffer2[i], nBackGround, nWidth);
	//	memset(&inspbuffer2[nHeight - 1 - i], nBackGround, nWidth);
	//	for (int j = 0; j < nHeight; j++) {
	//		inspbuffer2[j*nWidth + i] = nBackGround;
	//		inspbuffer2[j*nWidth + (nWidth - 1 - i)] = nBackGround;
	//		//inspbuffer2[j][i] = nBackGround;
	//		//inspbuffer2[j][nWidth - 1 - i] = nBackGround;
	//	}
	//}


	for (int i = 0; i < m_nInspOffset; i++) {//Jerry
		memset(inspbuffer[i], nBackGround, nWidth);
		memset(inspbuffer[nHeight - 1 - i], nBackGround, nWidth);
		for (int j = 0; j < nHeight; j++) {
			inspbuffer[j][i] = nBackGround;
			inspbuffer[j][nWidth - 1 - i] = nBackGround;
		}
	}

}
InspectionBase::InspectionBase(int nWidth, int nHeight)
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
	int sizerSize = nHeight * nHeight;
	int bufSize = nWidth * nHeight;

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
