#include "pch.h"
#include "Raw3D_Calculation.h"
#include <thread>
using namespace std;
//#include "Raw3D_RawData.h"
/*
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif
*/

Raw3D_Calculation::Raw3D_Calculation()
{
	m_nThreadIndex = 1;
	m_sz3DImage = CSize(0, 0);
	m_szRawImage = CSize(0, 0);
	m_ppBuffHeight = NULL;
	m_ppBuffBright = NULL;
	m_pBuffRaw = NULL;


	m_bStop = false;

	RAWDATA = Raw3D_RawData::GetInstance();
}


Raw3D_Calculation::~Raw3D_Calculation()
{
	if (m_pBuffRaw != NULL)
	{
		delete[] m_pBuffRaw;
		m_pBuffRaw = NULL;
	}
}

void Raw3D_Calculation::Initialize(int nThreadIndex, int nThreadNum, CSize sz3DImage, CSize szRawImage
	, LPBYTE* ppMainImage, WORD** ppBuffHeight, short** ppBuffBright, LPBYTE* ppBuffFG, int nSnapFrameNum)
{
	m_nThreadIndex = nThreadIndex;
	m_nThreadNum = nThreadNum;

	m_sz3DImage = sz3DImage;
	m_szRawImage = szRawImage;

	m_ppMainImage = ppMainImage;

	m_ppBuffHeight = ppBuffHeight;
	m_ppBuffBright = ppBuffBright;

	m_ppBuffFG = ppBuffFG;

	m_nSnapFrameNum = nSnapFrameNum;
	//m_pnCurrFrameNum = pnCurrFrameNum;

	CreateCvtBuffer(m_szRawImage);

	SetCalcState(Calc3DState::ReadyCalc);
}
typedef unsigned char       BYTE;
void Raw3D_Calculation::CreateCvtBuffer(CSize szRawImage)
{
	if (m_pBuffRaw != NULL)
	{
		delete[] m_pBuffRaw;
		m_pBuffRaw = NULL;
	}
	m_pBuffRaw = new BYTE[szRawImage.cx * szRawImage.cy];
}

/*
void Raw3D_Calculation::SendLog(CString strMsg)
{
	if (m_hLogForm != NULL)
	{
		CString sLogMsg = strMsg;
	//	::SendMessage(m_hLogForm, WM_MESSAGE_LOG_DATA, LOG_INDEX_MAPPING, (LPARAM)&sLogMsg);
	}
}*/

UINT ThreadCalculation(LPVOID lParam)
{
	Raw3D_Calculation* pRaw3DCalc = ((Raw3D_Calculation*)lParam);
	pRaw3DCalc->CalculateImage();

	return 0;
}

//Make Converted Image & Height/Bright Image & Display Image
void Raw3D_Calculation::StartCalculation(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos
	, int nMinGV1, int nMinGV2, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param)
{
	DebugTxt("StartCalculation\n", 18);
	m_cvtMode = cvtMode;
	m_calcMode = calcMode;
	m_displayMode = displayMode;

	m_ptDataPos = ptDataPos;

	m_nMinGV1 = nMinGV1;
	m_nMinGV2 = nMinGV2;

	m_nOverlapStartPos = nOverlapStartPos;
	m_nOverlapSize = nOverlapSize;

	m_nDisplayOffsetX = nDisplayOffsetX;
	m_nDisplayOffsetY = nDisplayOffsetY;

	m_bRevScan = bRevScan;
	m_bUseMinGV2 = bUseMinGV2;

	m_pnCurrFrameNum = pnCurrFrameNum;

	m_param = param;

	SetCalcState(Calc3DState::Calculating);
	
	thread t1(ThreadCalculation,this);
	// AfxBeginThread(ThreadCalculation, this);
}

//디스플레이되는 이미지 컨버팅만
//void Raw3D_Calculation::StartConvertDisplay(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptStart, )
//{
//}

void Raw3D_Calculation::StopCalc()
{
	DebugTxt("StopCalc\n", 10);
	m_bStop = true;
}

UINT ThreadConvertDisplay(LPVOID lParam)
{
	Raw3D_Calculation* pRaw3DCalc = ((Raw3D_Calculation*)lParam);
	//	pRaw3DCalc->CalculateImage();

	return 0;
}

void Raw3D_Calculation::CalculateImage()
{
	DebugTxt("CalculateImage\n",16);
	//스레드 인덱스에 따른 n 계산 해야함
	//역방향 추가해야함
	int n = 0;
	if (m_cvtMode == ConvertMode::NoConvert)
	{
		n = 0;
	}
	else if (m_cvtMode == ConvertMode::Bump)
	{
		n = m_szRawImage.cy;
	}

	int nFrameIndex = 0;
	if (nFrameIndex < 0)
	{
		SetCalcState(Calc3DState::ErrorCalc);
		DebugTxt("Calc3DState::ErrorCalc\n", 24);
		return;
	}

	while (nFrameIndex < m_nSnapFrameNum)
	{		
		//DebugTxt("while\n", 7);
		nFrameIndex = n + m_nThreadIndex;
		if (*m_pnCurrFrameNum > nFrameIndex)
		{
			DebugTxt("m_nSnapFrameNum\n", 17);
			switch (m_cvtMode)
			{
			case ConvertMode::NoConvert:
				NoConvertImage(nFrameIndex);
				break;
			case ConvertMode::Bump:
				ConvertImageBump(nFrameIndex, m_bRevScan);
				//nFrameIndex -= m_szRawImage.cy;	//이거 없애야하겠지?..
				break;
			default:
				break;
			}

			int nOverlapNum = (m_ptDataPos.x - m_nOverlapStartPos) / (m_szRawImage.cx - m_nOverlapSize);

			if (m_nOverlapStartPos > 0 && m_ptDataPos.x > m_nOverlapStartPos)
				nOverlapNum += 2;

			CPoint ptOLDataPos = CPoint(m_ptDataPos.x, m_ptDataPos.y);
			ptOLDataPos.x += (nOverlapNum * m_nOverlapSize);

			if (ptOLDataPos.x + m_szRawImage.cx >= m_sz3DImage.cx)
				break;

			int nYPos = 0;
			if (m_bRevScan == false)
			{
				nYPos = nFrameIndex;
			}
			else
			{
				//nYPos = m_nSnapFrameNum - nFrameIndex - m_szRawImage.cy;
				nYPos = m_nSnapFrameNum - m_param.nSnapFrameRest - nFrameIndex;// + m_szRawImage.cy;
			}

			if (ptOLDataPos.y + nYPos < 0 || ptOLDataPos.y + nYPos >= m_sz3DImage.cy)
				break;

			switch (m_calcMode)
			{
			case Calc3DMode::Normal:
				DebugTxt("Calc3DMode::Normal\n", 17);
				CalcHBImage(nYPos, ptOLDataPos, m_nMinGV1, m_nMinGV2, m_bUseMinGV2);
				break;
			case Calc3DMode::FromTop:
				CalcHBImage_FromTop(nYPos, ptOLDataPos, m_nMinGV1, m_nMinGV2, m_bUseMinGV2);
				break;
			case Calc3DMode::SK_Under15:
				CalcHBImage_SKPAD_MinGV1(nYPos, ptOLDataPos, m_nMinGV1, m_nMinGV2, m_bUseMinGV2);
				break;
			case Calc3DMode::ConsiderZero:
				CalcHBImage_ConsiderZeroBump(nYPos, ptOLDataPos, m_nMinGV1, m_nMinGV2, m_bUseMinGV2);
				break;
			case Calc3DMode::ConsiderZeroAutoBeamThk:
				CalcHBImage_ConsiderZeroBump_AutoBeamThickness(nYPos, ptOLDataPos, m_nMinGV1, m_nMinGV2, m_bUseMinGV2);
				break;
			default:
				break;
			}

			CRect rtCvtROI = CRect(m_ptDataPos.x, nYPos + ptOLDataPos.y, m_szRawImage.cx, 1);
			ConvertDisplayImage(m_displayMode, rtCvtROI, m_nOverlapStartPos, m_nOverlapSize, m_nDisplayOffsetX, m_nDisplayOffsetY);

			n += m_nThreadNum;
		}

		if (m_bStop == true)
			break;
	}
	DebugTxt("while End\n", 11);
	SetCalcState(Calc3DState::Done);
}

void Raw3D_Calculation::NoConvertImage(int nFrameIndex)
{
	BYTE* pSrc, * pDst;
	for (int y = 0; y < m_szRawImage.cy; y++)
	{
		if (m_ppBuffFG[nFrameIndex][y * m_szRawImage.cx] != NULL)
		{
			pSrc = &m_ppBuffFG[nFrameIndex][y * m_szRawImage.cx];
			pDst = &m_pBuffRaw[y * m_szRawImage.cx];

			memcpy(pDst, pSrc, m_szRawImage.cx);
		}
	}
}

void Raw3D_Calculation::ConvertImageBump(int nFrameIndex, bool bReverseScan)
{
	BYTE* pSrc, * pDst;
	if ((nFrameIndex < 0) || (nFrameIndex >= m_sz3DImage.cy))
		return;

	if ((nFrameIndex < m_szRawImage.cy) || (nFrameIndex >= m_sz3DImage.cy))
		return;

	//TRACE("FrameIndex: %d // nFrameIdx: %d // LineNum: %d \n", nFrameIndex, nFrameIdx, nLineNum);
	//TRACE("FrameIndex: %d\n", nFrameIndex);

	if (bReverseScan == false)
	{
		for (long y = 0; y < m_szRawImage.cy; y++)
		{
			//pSrc=&m_pBufRaw[nLine+y][y*m_szRawImage.cx]; //데모룸 스캔방향(-방향)
			//int nFrameIdx = nFrameIndex + m_szRawImage.cy - y - 1;
			int nFrameIdx = nFrameIndex - y - 1;
			int nLineNum = y * m_szRawImage.cx;

			pSrc = m_ppBuffFG[nFrameIdx];
			//pSrc = m_ppBuffFG[50];
			pSrc += nLineNum;//이게 문제임

			pDst = &m_pBuffRaw[nLineNum];
			memcpy(pDst, pSrc, m_szRawImage.cx);
		}
	}
	else
	{
		for (long y = 0; y < m_szRawImage.cy; y++)
		{
			int nFrameIdx = nFrameIndex - y - 1;
			int nLineNum = (m_szRawImage.cy - y - 1) * m_szRawImage.cx;

			pSrc = m_ppBuffFG[nFrameIdx];
			pSrc += nLineNum;

			pDst = &m_pBuffRaw[nLineNum];
			memcpy(pDst, pSrc, m_szRawImage.cx);
		}
	}
}

//Height, Bright Image 만들기
void Raw3D_Calculation::CalcHBImage(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
{
	DebugTxt("CalcHBImage\n", 13);
	long nSum, nYSum, nBSum, nMax, yMax, yAve, nGV, y0, dy;
	long yHMax = 0;
	long yHMin = 255;
	long gvMax = 0;
	long gvMin = 255;
	long gvDelta = 0;

	BYTE* pBuf;
	bool bAveMax = true;
	bool bMinMax = true;

	bool bCheckMinGV2 = false;
	int nGVMin = nMinGV1;

	//		omp_set_num_threads(omp_get_max_threads());
	//#pragma omp parallel for reduction(+:n)

	for (int x = 0; x < m_szRawImage.cx; x++)
	{
		nGVMin = nMinGV1;
		bCheckMinGV2 = false;
		pBuf = m_pBuffRaw + x;
		nSum = nYSum = nBSum = 0;
		nMax = yMax = -1;

		for (int y = 0; y < m_szRawImage.cy; y++)
		{
			if (*pBuf > nMax)
			{
				nMax = *pBuf;
				yMax = y;
			}

			if (*pBuf > nMinGV1)
			{
				nGV = *pBuf - nMinGV1;
				nSum += nGV;
				nYSum += nGV * (m_szRawImage.cy - y - 1);
			}

			//nBSum += *pBuf;
			pBuf += m_szRawImage.cx;
		}

		if (nSum == 0)	//MinGV1 이상값이 하나도 없으면
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;

			if (bUseDualMinGV && nMinGV2 > 0)	//MinGV2값이 있으면 MinGV1로 구한것처럼 다시 구하기
			{
				nGVMin = nMinGV2;
				pBuf = m_pBuffRaw + x;
				nSum = nYSum = nBSum = 0;
				nMax = yMax = -1;
				for (int y = 0; y < m_szRawImage.cy; y++)
				{
					if (*pBuf > nMax)
					{
						nMax = *pBuf;
						yMax = y;
					}
					if (*pBuf > nGVMin)
					{
						nGV = *pBuf - nGVMin;
						nSum += nGV;
						nYSum += nGV * (m_szRawImage.cy - y - 1);
					}
					//nBSum += *pBuf;
					pBuf += m_szRawImage.cx;
				}
				bCheckMinGV2 = true;
			}
		}

		if (nSum != 0)
		{
			yAve = m_szRawImage.cy - nYSum / nSum - 1;	//기준 (가중평균)

			if (yAve > yMax)	//기준보다 위가 더 밝다
			{
				y0 = m_szRawImage.cy - 1;	//맨 아래부터
				dy = -1;

				pBuf = m_pBuffRaw + x + y0 * m_szRawImage.cx;
				yHMin = yAve;
				gvMin = 255;
				gvDelta = gvMax = 0;
				for (int y = y0; y >= yAve; y += dy)	//맨 아래부터 기준점까지
				{
					if (*pBuf > gvMax)
					{
						gvMax = *pBuf;
						yHMax = y;
					}
					else if ((*pBuf - gvMax) < gvDelta)	//현재값이 범위안에서 찾은 Max값 보다 크지않고 gvDelta보다 작을때
					{
						gvMin = *pBuf;	//Min값을 구함
						gvDelta = *pBuf - gvMax;	//-값이 들어감
						yHMin = y;
					}
					pBuf += (dy * m_szRawImage.cx);

					//현재 y위치-기준 과의 거리보다 현재 y위치-MaxGV위치 거리가 더 가깝고 Max위치보다 위에 있음
					//MinGV 위치와는 2픽셀 이상 떨어져있으면서 Min위치가 현재 지점보다 더 아래에 있음
					if (((yHMax - y) > (y - yAve)) && ((yHMin - y) > 2))
						break;
					//y = yAve - 1;	//끝
				}
				//Min보다 Max가 위에 있음 bMinMax = true / Min이 더 위에 있음 bMinMax = false
				if (yHMin > yHMax)
					bMinMax = true;
				else
					bMinMax = false;

				bAveMax = true;
			}
			else //기준보다 아래가 더 밝다
			{
				y0 = 0;
				dy = 1;

				pBuf = m_pBuffRaw + x + y0 * m_szRawImage.cx;
				yHMin = yAve;
				gvMin = 255;
				gvDelta = gvMax = 0;
				for (int y = y0; y <= yAve; y += dy)	// 맨 위부터 기준점까지
				{
					if (*pBuf > gvMax)
					{
						gvMax = *pBuf;
						yHMax = y;
					}
					else if ((*pBuf - gvMax) < gvDelta)
					{
						gvMin = *pBuf;
						gvDelta = *pBuf - gvMax;
						yHMin = y;
					}
					pBuf += (dy * m_szRawImage.cx);

					if (((y - yHMax) > (yAve - y)) && ((y - yHMin) > 2))
						break;
					//y = yAve + 1;
				}
				//Max보다 Min이 위에 있음 bMinMax = true / Max가 더 위에 있음 bMinMax = false	//true가 정상적임
				if (yHMin <= yHMax)
					bMinMax = true;
				else
					bMinMax = false;

				bAveMax = false;
			}

			(bAveMax) ? bMinMax = (yHMin > yHMax) : bMinMax = (yHMin <= yHMax);

			if (((gvMax - gvMin) < (nGVMin / 2)) || bMinMax)	//Max가 더 위에 있거나 / Mingv값의 반보다 gvMaxMin 차이가 작을 때(이건 진짜 이해 안감)
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);

				if (bCheckMinGV2 == true)	//MinGV2값이면 (-)
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nSum);
				else
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nSum);
			}
			else
			{ // 왜 다시하는거?  Min이 더 위에 있거나 / 
				pBuf = m_pBuffRaw + x;
				nSum = nYSum = 0;

				if (gvMin < nGVMin)
					gvMin = nGVMin;

				for (int y = 0; y < yHMin; y++)
				{
					if (*pBuf > gvMin)
					{
						nGV = *pBuf - gvMin;
						nSum += nGV;
						nYSum += nGV * (m_szRawImage.cy - y - 1);
					}
					pBuf += m_szRawImage.cx;
				}
				if (nSum == 0)
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
				}
				else
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);

					if (bCheckMinGV2 == true)	//MinGV2값이면(-)
						m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nSum);
					else
						m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nSum);
				}
			}
		}
	}
	DebugTxt("CalcHBImage End\n", 17);
}

void Raw3D_Calculation::CalcHBImage_FromTop(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
{
	long nSum, nYSum, nBSum, nGV;

	//BYTE *pBuf;


	int nCnt = 0;
	//		omp_set_num_threads(omp_get_max_threads());
	//#pragma omp parallel for reduction(+:n)

	for (int x = 0; x < m_szRawImage.cx; x++)
	{
		bool bCheck = false;
		bool bCheckMinGV2 = false;
		BYTE* pBuf = m_pBuffRaw + x;
		nSum = nYSum = nBSum = 0;
		nCnt = 0;

		for (int y = 0; y < m_szRawImage.cy; y++)
		{
			if (*pBuf >= nMinGV1)
			{
				nGV = *pBuf - nMinGV1;
				nSum += nGV;
				nYSum += nGV * (m_szRawImage.cy - y - 1);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck)
				{
					bCheck = true;
				}
			}
			else if (bCheck)
			{
				if (nCnt < 3)
				{
					nSum = 0;
					nYSum = 0;
					bCheck = false;
				}
				else
				{
					break;
				}
			}

			//nBSum += *pBuf;
			pBuf += m_szRawImage.cx;
		}


		//MinGV2
		if (bUseDualMinGV && nMinGV2 > 0
			&& nSum == 0)
		{
			pBuf = m_pBuffRaw + x;
			nSum = nYSum = nBSum = 0;
			bCheck = false;
			nCnt = 0;

			for (int y = 0; y < m_szRawImage.cy; y++)
			{
				if (*pBuf >= nMinGV2)
				{
					nGV = *pBuf - nMinGV2;
					nSum += nGV;
					nYSum += nGV * (m_szRawImage.cy - y - 1);
					nBSum += *pBuf;
					nCnt++;

					if (!bCheck)
					{
						bCheck = true;
					}
				}
				else if (bCheck)
				{
					if (nCnt < 3)
					{
						nSum = 0;
						nYSum = 0;
						bCheck = false;
					}
					else
					{
						break;
					}
				}
			}
			bCheckMinGV2 = true;
		}

		if (nSum == 0)
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
		}
		else
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);

			/*if (nSum > m_szRawImage.cy * 2)
			{*/
			if (bCheckMinGV2 == true)	//MinGV2값이면 (-)
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nBSum);
			else
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nBSum);
			/*}
			else
			{
				if (bCheckMinGV2 == true)
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -1;
				else
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 1;
			}*/
		}
	}
}

// Beam 두께 parameter 사용
void Raw3D_Calculation::CalcHBImage_SKPAD_MinGV1(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
{
	BYTE* pBuf;
	BYTE* pBufX;

	int nLinenum = m_param.n3DScanParam;
	bool bCheckMinGV2 = false;
	///////////////////////////
	// Beam 중심위치 구하기

	memset(m_SumX, 0, MAX_RAW_Y * sizeof(int));
	memset(m_SumX2, 0, MAX_RAW_Y * sizeof(int));

	for (int y = 0; y < m_szRawImage.cy; y++)
	{
		pBuf = m_pBuffRaw + y * m_szRawImage.cx;
		for (int x = 0; x < m_szRawImage.cx; x++)
		{
			int a = *pBuf - nMinGV1;
			int b = *pBuf - nMinGV2;
			if (a < 0)
				a = 0;
			if (b < 0)
				b = 0;

			m_SumX[y] += a;
			m_SumX2[y] += b;
			pBuf++;
		}
	}

	int BeamCenter = 0;  // Beam 중심위치
	int BeamCenter2 = 0;  // Beam 중심위치
	int Max = -1;
	int Max2 = -1;
	for (int y = 0; y < m_szRawImage.cy; y++)
	{
		if (Max < m_SumX[y])
		{
			BeamCenter = y;
			Max = m_SumX[y];
		}

		if (Max2 < m_SumX2[y])
		{
			BeamCenter2 = y;
			Max2 = m_SumX2[y];
		}
	}

	//Beam 두께 구하기
	int BeamThk = 0;
	int BeamThk2 = 0;
	for (int y = m_szRawImage.cy - 1; y >= BeamCenter; y--)
	{
		if (0 < m_SumX[y] - m_szRawImage.cx)
		{
			BeamThk++;
		}
	}
	BeamThk = BeamThk * 2 - 1;
	BeamThk2 = BeamThk2 * 2 - 1;

	/////////////////////////////////////////
	// Height Buffer, Bright Buffer 구하기

	long x = 0, y = 0;
	long nGV;

	long nBSum; // Bright Buffer 용
	long nBSum2; // Bright Buffer 용 Mingv2
	long nYSum; // Height Buffer 용
	long nSum; // Height Buffer 용

	bool bCheck;
	int nCnt;
	int nFirst_y;

	for (x = 0; x < m_szRawImage.cx; x++)
	{
		bCheckMinGV2 = false;
		pBufX = m_pBuffRaw + x;
		nSum = nYSum = nBSum = 0;
		bCheck = false;
		nCnt = 0;
		nFirst_y = 0; // 상단 최초점
		bool bBumpAndBeam = false;
		bool bNoException = true; // false이면 예외처리 없이 처리할 것

		for (y = 0; y < m_szRawImage.cy; y++)
		{
			pBuf = pBufX + y * m_szRawImage.cx;

			int nGV = *pBuf - nMinGV1;
			if (nGV < 0) nGV = 0;

			if (nGV > 0) // nGVMin 이상의 값만 Bright Buffer로 합산 , Height Buffer 로 합산
			{
				nSum += nGV;
				nYSum += nGV * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // 최초에 한번만
				{
					bCheck = true;
					nFirst_y = y;  // 상단 최초점

					if (nFirst_y < BeamCenter - nLinenum)
						bBumpAndBeam = true; // 예외처리용
				}
			}
			else if (bCheck)  // 0이 나왔을 때 
			{
				if (nCnt < 2)  // 1칸 짜리는 예외처리 
				{
					nSum = 0;
					nYSum = 0;
					nBSum = 0;
					bCheck = false;
					nCnt = 0;
					nFirst_y = 0;
					bBumpAndBeam = false;
					bNoException = true;
				}
				else
				{
					if (y < BeamCenter)		//얘가 범프 떨어진 경우인데
						bNoException = false; // 예외처리에 안들어감

					break;
				}
			}
		}

		int nZeroOffset = 3;
		if (nSum > 0)
		{
			if ((int)((double)nYSum / nSum) + nZeroOffset < m_szRawImage.cy - BeamCenter - 1)
			{
				nSum = 0;
				nBSum = 0;
				bBumpAndBeam = false;
			}
		}

		int nMinGV = nMinGV1;
		if (nMinGV2 > 0 && bUseDualMinGV && nSum == 0)	//Mingv2
		{
			nMinGV = nMinGV2;
			//BeamThk = BeamThk2;
			bNoException = false;  // 예외처리 안들어감
			bCheckMinGV2 = true;
			bCheck = false;
			nCnt = 0;
			nYSum = 0;
			nBSum2 = 0;
			nBSum = 0;

			for (y = 0; y < m_szRawImage.cy; y++)
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				int nGV = *pBuf - nMinGV;
				if (nGV < 0)
					nGV = 0;

				if (nGV > 0) // nGVMin 이상의 값만 Bright Buffer로 합산 , Height Buffer 로 합산
				{
					nSum += nGV;
					nYSum += nGV * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nBSum += *pBuf;
					nCnt++;

					if (!bCheck) // 최초에 한번만
					{
						bCheck = true;
						nFirst_y = y;  // 상단 최초점

						if (nFirst_y < BeamCenter - nLinenum)
							bBumpAndBeam = true; // 예외처리용
					}
				}
				else if (bCheck)  // 0이 나왔을 때 
				{
					if (nCnt < 2)  // 1칸 짜리는 예외처리 
					{
						nSum = 0;
						nYSum = 0;
						nBSum = 0;
						nBSum2 = 0;
						bCheck = false;
						nCnt = 0;
						nFirst_y = 0;
						bBumpAndBeam = false;
						bNoException = true;
					}
					else
					{
						if (y < BeamCenter)		//얘가 범프 떨어진 경우인데
							bNoException = false; // 예외처리에 안들어감

						break;
					}
				}
			}
		}

		//// --> nYSum, nSum 과 nFirst_y (상단최초점)

		////////////////////////////////////////////////
		//   예외처리 - Beam 과 Bump가 붙는 경우를 구분
		////////////////////////////////////////////////

		int nLast_y = 0;
		if (bBumpAndBeam && bNoException)
		{
			// 최대값 구하기 - 대략적인 최대값을 구함
			int nMax = 0;
			int yMax = 0;
			int nMin = 255;
			int yMin = MAX_RAW_Y;

			//for (int y = nFirst_y; y < nFirst_y + m_n3DScanMode; y++)  
			//for (int y = nFirst_y; y < nFirst_y + 5; y++)
			for (int y = nFirst_y; y < BeamCenter - BeamThk / 2; y++)
				//for (int y = nFirst_y; y < BeamCenter - nLinenum / 2; y++)
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				int nGV = *pBuf - nMinGV;

				if (nGV < 0)
					nGV = 0;

				if (nGV > nMax)
				{
					nMax = nGV;
					yMax = y;
				}
			}

			// 최소값 구하기, 자르는 위치
			bool IsMin = false;
			nMin = nMax;

			for (int y = yMax; y < nFirst_y + nLinenum; y++)
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				int nGV = *pBuf - nMinGV;

				if (nGV < 0)
					nGV = 0;

				if (nGV < nMin)
				{
					nMin = nGV;
					yMin = y;

					if (nMin + 5 < nMax)
						IsMin = true;
				}
			}
			nLast_y = yMin;
			//// --> nLast_y 구함
			// 계산

			nSum = nYSum = 0;
			if (bCheckMinGV2 == false)
				nBSum = 0;

			if (IsMin == false)
			{
				nLast_y = m_szRawImage.cy - 1; // 에외처리 - Bump없이 Beam이 매우 높게 올라온 경우
			}

			for (y = nFirst_y; y < nLast_y; y++)
			{
				pBuf = pBufX + y * m_szRawImage.cx;
				int nGV = *pBuf - nMinGV;
				if (nGV < 0)
					nGV = 0;
				nSum += nGV;
				nYSum += nGV * (m_szRawImage.cy - 1 - y);

				if (bCheckMinGV2 == false)
					nBSum += *pBuf;
			}
		}

		if (nSum == 0)
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
		}
		else
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);

			if (bCheckMinGV2 == true)	//MinGV2값이면(-)
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nBSum);
			else
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nBSum);
		}
	}

}


void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200520 이종현 Merge
{
	BYTE* pBuf;
	BYTE* pBufX;

	int nLinenum = m_param.n3DScanParam;
	bool bCheckMinGV2 = false;

	int nStrongestBeamLine = 0;
	int nMaxRowBrightSum = 0;

	for (int y = 0; y < m_szRawImage.cy; y++)
	{
		pBuf = m_pBuffRaw + y * m_szRawImage.cx;
		int nRowBrightSum = 0;
		for (int x = 0; x < m_szRawImage.cx; x++)
		{
			nRowBrightSum += *pBuf;
			/*if (*pBuf>nMinGV1)
			{
				nRowBrightSum += *pBuf ;
			}*/
			pBuf++;
		}
		if (nRowBrightSum >= nMaxRowBrightSum)
		{
			nStrongestBeamLine = y;

			nMaxRowBrightSum = nRowBrightSum;
		}
	}

	int BeamThk = nLinenum / 2; //빔두께 구하는부분이 먼가 잘못됨

	//위에서 빔 굵기, 빔 센터 다 찾고나서

	long x = 0;
	long y = 0;

	long nBSum;
	long nYSum;
	long nSum;
	long nBSum2;
	long nYSum2;
	long nSum2;

	bool bCheck;
	int nCnt;
	int nFirst_y;
	int nLast_y;

	for (x = 0; x < m_szRawImage.cx; x++) //매 x마다
	{
		bCheckMinGV2 = false;
		pBufX = m_pBuffRaw + x;
		nSum = 0;
		nYSum = 0;
		nBSum = 0;
		bCheck = false;
		nCnt = 0;
		nFirst_y = 0;
		nLast_y = 0;

		for (y = 0; y < m_szRawImage.cy; y++) //매 y를 확인함
		{

			pBuf = pBufX + y * m_szRawImage.cx;

			if (*pBuf > nMinGV1) // y의 어떤 점이 mingv1 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
			{
				nSum += *pBuf - nMinGV1;
				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
				{
					bCheck = true;
					nFirst_y = y;
				}
			}
			else if (bCheck) //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
			{
				if (nCnt == 1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다.
				{
					nSum = 0;
					nYSum = 0;
					nBSum = 0;
					bCheck = false;
					nCnt = 0;
					nFirst_y = 0;
				}
				else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다.
				{
					nLast_y = nFirst_y + nCnt - 1;
					break;
				}
			}
		}

		if ((nSum == 0 || nFirst_y >= nStrongestBeamLine - BeamThk) && bUseDualMinGV) //근데 만약에 nSum이 0이거나 최초지점이 빔라인보다 낮은경우, mingv2를 사용한다. beamthk 숫자가 너무 크면 바닥이 안나옴 왜인지모르겟음;
		{

			nSum2 = 0;
			nYSum2 = 0;
			nBSum2 = 0;

			bCheck = false;
			int nCnt2 = 0;
			int nFirst_y2 = 0;
			int nLast_y2 = 0;

			for (long y = 0; y < nStrongestBeamLine - 2 * BeamThk; y++) //mingv2를 사용해서 찾는다. all m_szRawImage.cy 지금 이거로하면 안나옴, cut nStrongestBeamLine-2*BeamThk
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				if (*pBuf > nMinGV2) // y의 어떤 점이 mingv2 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
				{
					nSum2 += *pBuf - nMinGV2;
					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nCnt2++;

					if (!bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
					{
						bCheck = true;
						nFirst_y2 = y;
						bCheckMinGV2 = true;
					}
				}
				else if (bCheck)  //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
				{
					if (nCnt2 == 1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다. -> (bCheckMinGV2 = false)구문으로 mingv1에서 찾은값을 그냥 쓴다. 
					{
						nSum2 = 0;
						nYSum2 = 0;
						nBSum2 = 0;
						bCheck = false;
						nCnt2 = 0;
						nFirst_y2 = 0;
						bCheckMinGV2 = false;
					}
					else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다. (사실 마지막부분 기록할필요없다 필요할줄알고 해놓음)
					{
						nLast_y2 = nFirst_y2 + nCnt2 - 1;
						break;
					}
				}
			}
		}
		if (!bCheckMinGV2) //mingv1 값으로 계산한결과를 반환
		{
			if (nSum == 0)
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			}

			else
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
			}
		}
		else //(mingv2값으로 계산한 결과를 반환
		{
			if (nSum2 == 0)
			{
				if (nSum == 0)
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
				}
				else
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
				}
			}
			else
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum2 / nSum2);
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)nBSum2;
			}
		}
	}
}


////#1. BeamThk 쓰는 방법
void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump_AutoBeamThickness(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200623 이종현 빔두께
{
	BYTE* pBuf;
	BYTE* pBufX;

	int nLinenum = m_param.n3DScanParam;
	bool bCheckMinGV2 = false;

	int nStrongestBeamLine = 0;
	int nMaxRowBrightSum = 0;
	int nBeamSum = 0;
	for (int y = 0; y < m_szRawImage.cy; y++)
	{
		pBuf = m_pBuffRaw + y * m_szRawImage.cx;
		int nRowBrightSum = 0;
		for (int x = 0; x < m_szRawImage.cx; x++)
		{
			nRowBrightSum += *pBuf;
			if (*pBuf > nMinGV1)
			{
				nBeamSum++;
			}
			pBuf++;
		}
		if (nRowBrightSum >= nMaxRowBrightSum)
		{
			nStrongestBeamLine = y;

			nMaxRowBrightSum = nRowBrightSum;
		}
	}

	int nCount = 0;

	pBuf = m_pBuffRaw + nStrongestBeamLine * m_szRawImage.cx;
	for (int x = 0; x < m_szRawImage.cx; x++)
	{
		if (*pBuf > nMinGV1)
		{
			nCount++;
		}
		pBuf++;
	}

	int BeamThk = 0;
	if (nCount == 0)
	{
		BeamThk = 0;
	}
	else
	{
		BeamThk = nBeamSum / nCount + 1;
	}

	//위에서 빔 굵기, 빔 센터 다 찾고나서

	long x = 0;
	long y = 0;

	long nBSum;
	long nYSum;
	long nSum;
	long nBSum2;
	long nYSum2;
	long nSum2;

	bool bCheck;
	int nCnt;
	int nFirst_y;
	int nLast_y;

	for (x = 0; x < m_szRawImage.cx; x++) //매 x마다
	{
		bCheckMinGV2 = false;
		pBufX = m_pBuffRaw + x;
		nSum = 0;
		nYSum = 0;
		nBSum = 0;
		bCheck = false;
		nCnt = 0;
		nFirst_y = 0;
		nLast_y = 0;

		for (y = 0; y < m_szRawImage.cy; y++) //매 y를 확인함
		{

			pBuf = pBufX + y * m_szRawImage.cx;

			if (*pBuf > nMinGV1) // y의 어떤 점이 mingv1 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
			{
				nSum += *pBuf - nMinGV1;
				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
				{
					bCheck = true;
					nFirst_y = y;
				}
			}
			else if (bCheck) //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
			{
				if (nCnt == 1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다.
				{
					nSum = 0;
					nYSum = 0;
					nBSum = 0;
					bCheck = false;
					nCnt = 0;
					nFirst_y = 0;
				}
				else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다.
				{
					nLast_y = nFirst_y + nCnt - 1;
					break;
				}
			}
		}

		if ((nSum == 0 || nFirst_y >= nStrongestBeamLine - BeamThk) && bUseDualMinGV) //근데 만약에 nSum이 0이거나 최초지점이 빔라인보다 낮은경우, mingv2를 사용한다. beamthk 숫자가 너무 크면 바닥이 안나옴 왜인지모르겟음;
		{

			nSum2 = 0;
			nYSum2 = 0;
			nBSum2 = 0;

			bCheck = false;
			int nCnt2 = 0;
			int nFirst_y2 = 0;
			int nLast_y2 = 0;

			for (long y = 0; y < nStrongestBeamLine - 2 * BeamThk; y++) //mingv2를 사용해서 찾는다. all m_szRawImage.cy 지금 이거로하면 안나옴, cut nStrongestBeamLine-2*BeamThk
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				if (*pBuf > nMinGV2) // y의 어떤 점이 mingv2 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
				{
					nSum2 += *pBuf - nMinGV2;
					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nCnt2++;

					if (!bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
					{
						bCheck = true;
						nFirst_y2 = y;
						bCheckMinGV2 = true;
					}
				}
				else if (bCheck)  //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
				{
					if (nCnt2 == 1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다. -> (bCheckMinGV2 = false)구문으로 mingv1에서 찾은값을 그냥 쓴다. 
					{
						nSum2 = 0;
						nYSum2 = 0;
						nBSum2 = 0;
						bCheck = false;
						nCnt2 = 0;
						nFirst_y2 = 0;
						bCheckMinGV2 = false;
					}
					else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다. (사실 마지막부분 기록할필요없다 필요할줄알고 해놓음)
					{
						nLast_y2 = nFirst_y2 + nCnt2 - 1;
						break;
					}
				}
			}
		}
		if (!bCheckMinGV2) //mingv1 값으로 계산한결과를 반환
		{
			if (nSum == 0)
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			}

			else
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
			}
		}
		else //(mingv2값으로 계산한 결과를 반환
		{
			if (nSum2 == 0)
			{
				if (nSum == 0)
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
				}
				else
				{
					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
				}
			}
			else
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum2 / nSum2);
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)nBSum2;
			}
		}
	}
}





//#2. RowSum 사용하는방법
//void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200623 이종현 RowSum
//{
//	BYTE *pBuf;
//	BYTE *pBufX;
//
//	int nLinenum = m_param.n3DScanParam;
//	bool bCheckMinGV2 = false;
//
//	int nStrongestBeamLine = 0;
//	int nMaxRowBrightSum = 0 ;
//	int nCutoffBrightSum = 0;
//	int* arrCutoffBrightSum = new int[m_szRawImage.cy];
//
//	for (int y = 0; y < m_szRawImage.cy; y++)
//	{
//		pBuf = m_pBuffRaw + y * m_szRawImage.cx;
//		int nRowBrightSum = 0 ;
//		for (int x = 0; x < m_szRawImage.cx; x++)
//		{
//			nRowBrightSum += *pBuf ;
//			if (*pBuf>nMinGV1)
//			{
//				nCutoffBrightSum = *pBuf-nMinGV1;
//			}
//			pBuf++;
//		}
//		arrCutoffBrightSum[y] = nCutoffBrightSum;
//		if (nRowBrightSum >= nMaxRowBrightSum)
//		{
//			nStrongestBeamLine = y;
//
//			nMaxRowBrightSum = nRowBrightSum;
//		}
//	}
//	
//	//int BeamThk = nLinenum/2; //빔두께 구하는부분이 먼가 잘못됨
//	if (nStrongestBeamLine == 0)
//    {
//        nStrongestBeamLine = 1;
//    }
//    int nCutOffRow = 0;
//    for (int y = nStrongestBeamLine; y >0; y--)
//    {
//        if (arrCutoffBrightSum[y-1]==0 || arrCutoffBrightSum[y]<arrCutoffBrightSum[y-1])
//        {
//            nCutOffRow = y-1;
//            break;
//        }
//    }
//
//	//위에서 빔 굵기, 빔 센터 다 찾고나서
//
//	long x =0;
//	long y =0;
//
//	long nBSum;
//	long nYSum;
//	long nSum;
//	long nBSum2;
//	long nYSum2;
//	long nSum2;
//
//	bool bCheck;
//	int nCnt;
//	int nFirst_y;
//	int nLast_y;
//
//	for ( x=0; x< m_szRawImage.cx; x++) //매 x마다
//	{
//		bCheckMinGV2 = false;
//		pBufX=m_pBuffRaw + x;
//		nSum = 0;
//		nYSum = 0;
//		nBSum = 0;
//		bCheck = false;
//		nCnt =0;
//		nFirst_y=0;
//		nLast_y=0;
//		
//		for( y = 0; y < m_szRawImage.cy ; y++) //매 y를 확인함
//		{
//			
//			pBuf = pBufX + y * m_szRawImage.cx;
//			
//			if (*pBuf>nMinGV1) // y의 어떤 점이 mingv1 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
//			{
//				nSum += *pBuf - nMinGV1;
//				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy -1 -y);
//				nBSum += *pBuf;
//				nCnt++;
//				
//				if ( !bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
//				{
//					bCheck = true;
//					nFirst_y = y;
//				}
//			}
//			else if (bCheck) //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
//			{
//				if(nCnt==1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다.
//				{
//					nSum = 0;
//					nYSum = 0;
//					nBSum = 0;
//					bCheck = false;
//					nCnt = 0;
//					nFirst_y = 0;
//				}
//				else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다.
//				{
//					nLast_y = nFirst_y + nCnt -1;
//					break;
//				}
//			}
//		}
//
//		if((nSum ==0 || nFirst_y > nCutOffRow-2) && bUseDualMinGV) //근데 만약에 nSum이 0이거나 최초지점이 빔라인보다 낮은경우, mingv2를 사용한다. beamthk 숫자가 너무 크면 바닥이 안나옴 왜인지모르겟음;
//		{
//
//			nSum2=0;
//			nYSum2 = 0;
//			nBSum2 = 0;
//
//			bCheck = false;
//			int nCnt2 = 0;
//			int nFirst_y2 = 0;
//			int nLast_y2 = 0;
//			
//			for(long y = 0; y < nCutOffRow-2; y++) //mingv2를 사용해서 찾는다. all m_szRawImage.cy 지금 이거로하면 안나옴, cut nStrongestBeamLine-2*BeamThk
//			{
//				pBuf = pBufX + y * m_szRawImage.cx;
//				
//				if (*pBuf>nMinGV2) // y의 어떤 점이 mingv2 보다 밝으면 -> 표시하고 나서 다시 확인하는데,
//				{
//					nSum2 += *pBuf - nMinGV2;
//					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy -1 -y);
//					nBSum2 += *pBuf;
//					nCnt2++;
//					
//					if ( !bCheck) // 그 위치를 최초 1회 기록하고 Check 됨을 표시한다. 
//					{
//						bCheck = true;
//						nFirst_y2 = y;
//						bCheckMinGV2 = true;
//					}
//				}
//				else if (bCheck)  //Check가 되어있는 상태에서  mingv1보다 밝지 않은 점이 나타나면 여기로 들어온다.
//				{
//					if(nCnt2 == 1) // 근데 연속하지않고 1픽셀만 지속된경우 노이즈로 판명한다. -> (bCheckMinGV2 = false)구문으로 mingv1에서 찾은값을 그냥 쓴다. 
//					{
//						nSum2 = 0;
//						nYSum2 = 0;
//						nBSum2 = 0;
//						bCheck = false;
//						nCnt2 = 0;
//						nFirst_y2 = 0;
//						bCheckMinGV2=false;
//					}
//					else //1픽셀보다 큰경우 그 check의 마지막부분을 기록한다. (사실 마지막부분 기록할필요없다 필요할줄알고 해놓음)
//					{
//						nLast_y2 = nFirst_y2 + nCnt2 -1;
//						break;				
//					}
//				}	
//			}
//		}
//		if (!bCheckMinGV2) //mingv1 값으로 계산한결과를 반환
//		{
//			if ( nSum ==0 )
//			{
//				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
//				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
//			}
//
//			else
//			{
//				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
//				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
//			}
//		}
//		else //(mingv2값으로 계산한 결과를 반환
//		{
//			if ( nSum2 ==0 )
//			{	
//				if ( nSum ==0 )
//				{	
//					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
//					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
//				}
//				else
//				{
//					m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);
//					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)nBSum;
//				}
//			}
//			else
//			{
//				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum2 / nSum2);
//				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)nBSum2;
//			}
//		}
//	}
//}
//
//

void Raw3D_Calculation::ConvertDisplayImage(DisplayMode displayMode, CRect rtROI, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY)
{
	//BYTE *pBYTE; WORD *pWord; long x, x0;

	//int n0 = (m_cp0.x - m_nOverlapStartPos) / m_szRawImage.cx;	//몇번째 라인인가?

	//if (m_nOverlapStartPos > 0 && m_cp0.x > m_nOverlapStartPos)
	//	n0++;

	//x0 = m_cp0.x - (m_nOverlap * n0);
	//pBYTE = &(m_pBufI[n][x0]);
	//pWord = &(m_pBufData[n][m_cpStart.x]);

	//for (x = 0; x < m_szRawImage.cx; x++)
	//{
	//	*pBYTE = (BYTE)(*pWord / m_szRawImage.cy);
	//	pBYTE++;
	//	pWord++;
	//} //forget

	int nOverlapNum = (rtROI.x - m_nOverlapStartPos) / (m_szRawImage.cx - m_nOverlapSize);

	if (m_nOverlapStartPos > 0 && rtROI.x > m_nOverlapStartPos)
		nOverlapNum++;

	int nOffsetX = m_nOverlapSize * nOverlapNum;

	switch (displayMode)
	{
	case DisplayMode::HeightImage:
		for (int y = rtROI.y; y < rtROI.y + rtROI.cy; y++)
		{
			for (int x = rtROI.x; x < rtROI.x + rtROI.cx; x++)
			{

				if (IsMinGV2(x + nOffsetX, y) == true)
				{
					m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = 0;
				}
				else
				{
					m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = (BYTE)(m_ppBuffHeight[y][x + nOffsetX] / m_szRawImage.cy);
				}
			}
		}
		break;
	case DisplayMode::BrightImage:
		for (int y = rtROI.y; y < rtROI.y + rtROI.cy; y++)
		{
			for (int x = rtROI.x; x < rtROI.x + rtROI.cx; x++)
			{
				if (IsMinGV2(x + nOffsetX, y) == true)
				{
					m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = 0;
				}
				else
				{
					m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = (BYTE)(m_ppBuffBright[y][x + nOffsetX] / m_szRawImage.cy);
				}
			}
		}
		break;
	default:
		break;
	}
}

ConvertMode Raw3D_Calculation::GetConvertMode()
{
	return m_cvtMode;
}

Calc3DMode Raw3D_Calculation::GetCalculationMode()
{
	return m_calcMode;
}