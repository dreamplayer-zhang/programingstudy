#include "pch.h"
#include "InspectionBase.h"

void InspectionBase::CloseDB()
{
	char ch[500];

	sprintf_s(ch, "%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d",
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1);

	string str(ch);
	//pDataBase->InsertData(str);

	pDataBase->CommitAndClose();
	//pDataBase->Commit();
}


void InspectionBase::OpenDB(int threadidx)
{
	string temp;
	temp = DBFolderPath + "/VSTEMP" + to_string(threadidx) + ".sqlite";


	dbcount = 0;
	//int result = CreateDirectoryA(DBFolderPath.c_str(), NULL);
	//pDataBase->DBCreateVSTemp(temp);
	pDataBase->OpenAndBegin(temp);
}

void InspectionBase::WriteDB(RECT rt, float size)
{
	int PosX;//
	int PosY;//
	int Darea;//
	int UnitX;
	int UnitY;
	int Width;//
	int Height;//
	int ClusterID;
	int Dcode;

	Width = rt.right - rt.left;
	Height = rt.bottom - rt.top;
	PosX = rt.left + (int)(Width * 0.5);
	PosY = rt.top + (int)(Height * 0.5);
	Darea = size;
	UnitX = 0;
	UnitY = 0;
	ClusterID = 0;
	Dcode = GetDefectCode();

	char ch[500];
	sprintf_s(ch, /*"%d,*/"%d,%d,%d,%d,%d,%d,%d,%d,%d,%d",
		/*dbcount,*/ Dcode, Darea, Width, Height, PosX, PosY, rt.left + 10, rt.top + 10, rt.right + 10, rt.bottom + 10); //rt에 10 더해줘야 맞는게 m_inspoffset떄문인가? 확인필요
	

//	sprintf_s(ch, /*"%d,*/"%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d",
//		/*dbcount,*/ PosX, PosY, Darea, UnitX, UnitY, Width, Height, ClusterID, rt.left + 10, rt.top + 10, rt.right + 10, rt.bottom + 10, 1234, Dcode, 0, 0);

	string str(ch);
	pDataBase->InsertData(str);
	dbcount++;
}
void InspectionBase::EraseDB(int threadidx)
{
	string temp;
	temp = DBFolderPath + "/VSTEMP" + to_string(threadidx) + ".sqlite";
	pDataBase->EraseAndClose(temp);
}


void InspectionBase::AddDefect(RECT rt, POINT ptDPos, float fSize)
{
	rt.left = rt.left + ptDPos.x - m_ptCurrent.x;
	rt.right = rt.right + ptDPos.x - m_ptCurrent.x;
	rt.bottom = rt.bottom + ptDPos.y - m_ptCurrent.y;
	rt.top = rt.top + ptDPos.y - m_ptCurrent.y;

	int nX = rt.left + (int)(Functions::GetWidth(rt) * 0.5);
	int nY = rt.top + (int)(Functions::GetHeight(rt) * 0.5);

	return WriteDB(rt, fSize);
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
void InspectionBase::CopyImageToBuffer()//byte* mem, int nW, RECT rt, int nBackGround, BOOL bInterpolation)
{
	//opencv 전의 test용으로 interpolation 내용 미구현

	byte* mem = pBuffer;
	int nW = nBufferW;//80000;
	RECT rt = rtROI;
	int nBackGround = 255;

	
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
			int ytarget = i + rt.top + (nOffset - 5) * 2;
			int xtarget = j + rt.left + (nOffset - 5) * 2;

			inspbuffer[i + nOffset][nOffset + j] = mem[(ytarget)*nW + (xtarget)];
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
InspectionBase::InspectionBase()
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

	pPitSizer = new PitSizer(2048 * 2048, 1);
	pDataBase = new Cpp_DB();
	pBuffer = new byte(10000 * 10000);

	m_ptCurrent.x = -1;
	m_ptCurrent.y = 0;
}

InspectionBase::~InspectionBase()
{
	Functions::SafeDelete(pPitSizer);
	Functions::SafeDelete(pDataBase);
	Functions::SafeDeleteArray(pBuffer);
}
