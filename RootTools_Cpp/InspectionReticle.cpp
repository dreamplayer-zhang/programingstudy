#include "pch.h"
#include "InspectionReticle.h"

void CInspectionReticle::SetParams(byte* buffer, int bufferwidth, int bufferheight, RECT roi, int defectCode, int threadindex, int nGV, int nSize)
{
	SetBuffer(buffer);
	SetBufferWidthHeight(bufferwidth, bufferheight);
	SetROI(roi);
	SetDefectCode(defectCode);
	SetIsDarkInspection(true);
	SetGrayLevel(nGV);
	SetDefectSize(nSize);
}

std::vector<DefectDataStruct> CInspectionReticle::StripInspection(int nBandwidth, int nIntensity, int nClassifyCode)
{
	std::vector<DefectDataStruct> vResult;
	bool bInspResult = false;

	//RECT rt;
	RECT rtROI = GetROI();
	RECT rtinspROI = GetInspbufferROI();
	//float ret;
	//byte* pPos;
	POINT ptDefectPos;
	ptDefectPos.x = rtROI.left;
	ptDefectPos.y = rtROI.top;

	int nWidth = Functions::GetWidth(rtROI);
	int nGrayLevel = GetGrayLevel();

	////2nd interpolation visionworks 참조해서 구현
	//if (bIP)
	//{
	//	nEndX = (int)(nEndX * nPatternInterpolationOffset);
	//	nEndY = (int)(nEndY * nPatternInterpolationOffset);
	//}
	/*int nEndX = rtROI.right;
	int nEndY = rtROI.bottom;*/
	int nEndX = rtROI.right - rtROI.left;
	int nEndY = rtROI.bottom - rtROI.top;


	if (nGrayLevel > 0)//절대검사 먼저
	{
		std::vector<DefectDataStruct> tempResult = SurfaceInspection(true, nClassifyCode);
		for (int i = 0; i < tempResult.size(); i++)
		{
			vResult.push_back(tempResult[i]);
		}
	}

	int nPL = CalHPPatternGV(nBandwidth, nIntensity, nEndX, nEndY);
	SetGrayLevel(nPL);//Pattern Target GV 계산 후 GV Level 재설정

	if (nPL > 0)
	{
		std::vector<DefectDataStruct> tempResult = SurfaceInspection(true, nClassifyCode);
		for (int i = 0; i < tempResult.size(); i++)
		{
			vResult.push_back(tempResult[i]);
		}
	}

	SetResult(bInspResult);
	return vResult;
}

int CInspectionReticle::CalHPPatternGV(int nBandwidth, int nIntensity, int nW, int nH)
{
	memset(m_Histogram, 0, 256 * sizeof(int));
	int nCnt = 0;
	int nPL = 0;
	LPBYTE p;

	for (int y = m_nInspOffset; y < nH + m_nInspOffset; y++) {
		p = &inspbuffer[y][m_nInspOffset];
		for (int x = m_nInspOffset; x < nW + m_nInspOffset; x++, p++) {
			m_Histogram[*p]++;
		}
	}

	for (int j = 0; j < nBandwidth; j++) {
		nCnt += m_Histogram[j];
	}


	for (int i = 0; i < 254 - nBandwidth; i++) {
		if (nCnt > nIntensity) {
			nPL = i;
			i = 256;
		}
		nCnt = nCnt - m_Histogram[i];
		//nCnt -= m_Histogram[i];
		if (i + nBandwidth < 256)//Joseph 2019-01-31 예외 방지
			nCnt += m_Histogram[i + nBandwidth];
	}

	return nPL;
}

CInspectionReticle::CInspectionReticle(int nWidth, int nHeight) : CInspectionSurface(nWidth, nHeight)
{
}
