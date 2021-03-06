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
	m_sz3DImage = CCSize(0, 0);
	m_szRawImage = CCSize(0, 0);
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

void Raw3D_Calculation::Initialize(int nThreadIndex, int nThreadNum, CCSize sz3DImage, CCSize szRawImage
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
void Raw3D_Calculation::CreateCvtBuffer(CCSize szRawImage)
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
void Raw3D_Calculation::SetFrameNum(int n)
{
	m_nCurrFrameNum = n;
}
thread* t1;
//Make Converted Image & Height/Bright Image & Display Image
void Raw3D_Calculation::StartCalculation(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CCPoint ptDataPos
	, int nMinGV1, int nMinGV2, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param)
{	
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

	//m_pnCurrFrameNum = pnCurrFrameNum;

	m_param = param;

	SetCalcState(Calc3DState::Calculating);
	
	t1 = new thread(ThreadCalculation,this);
	// AfxBeginThread(ThreadCalculation, this);
}

//?????????????? ?????? ????????
//void Raw3D_Calculation::StartConvertDisplay(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptStart, )
//{
//}

void Raw3D_Calculation::StopCalc()
{	
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
	//?????? ???????? ???? n ???? ??????
	//?????? ??????????
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
		return;
	}

	while (nFrameIndex < m_nSnapFrameNum)
	{		
		//Sleep(1);
		//DebugTxt("while\n", 7);
		nFrameIndex = n + m_nThreadIndex;
		if (m_nCurrFrameNum > nFrameIndex)
		{
			switch (m_cvtMode)
			{
			case ConvertMode::NoConvert:
				NoConvertImage(nFrameIndex);
				break;
			case ConvertMode::Bump:
				ConvertImageBump(nFrameIndex, m_bRevScan);
				//nFrameIndex -= m_szRawImage.cy;	//???? ?????????????..
				break;
			default:
				break;
			}

			int nOverlapNum = (m_ptDataPos.x - m_nOverlapStartPos) / (m_szRawImage.cx - m_nOverlapSize);

			if (m_nOverlapStartPos > 0 && m_ptDataPos.x > m_nOverlapStartPos)
				nOverlapNum += 2;

			CCPoint ptOLDataPos = CCPoint(m_ptDataPos.x, m_ptDataPos.y);
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

			CCRect rtCvtROI = CCRect(m_ptDataPos.x, nYPos + ptOLDataPos.y, m_szRawImage.cx, 1);
			ConvertDisplayImage(m_displayMode, rtCvtROI, m_nOverlapStartPos, m_nOverlapSize, m_nDisplayOffsetX, m_nDisplayOffsetY);

			n += m_nThreadNum;
		}

		if (m_bStop == true)
			break;
	}
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
			//pSrc=&m_pBufRaw[nLine+y][y*m_szRawImage.cx]; //?????? ????????(-????)
			//int nFrameIdx = nFrameIndex + m_szRawImage.cy - y - 1;
			int nFrameIdx = nFrameIndex - y - 1;
			int nLineNum = y * m_szRawImage.cx;

			pSrc = m_ppBuffFG[nFrameIdx];
			//pSrc = m_ppBuffFG[50];
			pSrc += nLineNum;//???? ??????

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

//Height, Bright Image ??????
void Raw3D_Calculation::CalcHBImage(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
{
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

		if (nSum == 0)	//MinGV1 ???????? ?????? ??????
		{
			m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;
			m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = 0;

			if (bUseDualMinGV && nMinGV2 > 0)	//MinGV2???? ?????? MinGV1?? ?????????? ???? ??????
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
			yAve = m_szRawImage.cy - nYSum / nSum - 1;	//???? (????????)

			if (yAve > yMax)	//???????? ???? ?? ????
			{
				y0 = m_szRawImage.cy - 1;	//?? ????????
				dy = -1;

				pBuf = m_pBuffRaw + x + y0 * m_szRawImage.cx;
				yHMin = yAve;
				gvMin = 255;
				gvDelta = gvMax = 0;
				for (int y = y0; y >= yAve; y += dy)	//?? ???????? ??????????
				{
					if (*pBuf > gvMax)
					{
						gvMax = *pBuf;
						yHMax = y;
					}
					else if ((*pBuf - gvMax) < gvDelta)	//???????? ?????????? ???? Max?? ???? ???????? gvDelta???? ??????
					{
						gvMin = *pBuf;	//Min???? ????
						gvDelta = *pBuf - gvMax;	//-???? ??????
						yHMin = y;
					}
					pBuf += (dy * m_szRawImage.cx);

					//???? y????-???? ???? ???????? ???? y????-MaxGV???? ?????? ?? ?????? Max???????? ???? ????
					//MinGV ???????? 2???? ???? ?????????????? Min?????? ???? ???????? ?? ?????? ????
					if (((yHMax - y) > (y - yAve)) && ((yHMin - y) > 2))
						break;
					//y = yAve - 1;	//??
				}
				//Min???? Max?? ???? ???? bMinMax = true / Min?? ?? ???? ???? bMinMax = false
				if (yHMin > yHMax)
					bMinMax = true;
				else
					bMinMax = false;

				bAveMax = true;
			}
			else //???????? ?????? ?? ????
			{
				y0 = 0;
				dy = 1;

				pBuf = m_pBuffRaw + x + y0 * m_szRawImage.cx;
				yHMin = yAve;
				gvMin = 255;
				gvDelta = gvMax = 0;
				for (int y = y0; y <= yAve; y += dy)	// ?? ?????? ??????????
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
				//Max???? Min?? ???? ???? bMinMax = true / Max?? ?? ???? ???? bMinMax = false	//true?? ????????
				if (yHMin <= yHMax)
					bMinMax = true;
				else
					bMinMax = false;

				bAveMax = false;
			}

			(bAveMax) ? bMinMax = (yHMin > yHMax) : bMinMax = (yHMin <= yHMax);

			if (((gvMax - gvMin) < (nGVMin / 2)) || bMinMax)	//Max?? ?? ???? ?????? / Mingv???? ?????? gvMaxMin ?????? ???? ??(???? ???? ???? ????)
			{
				m_ppBuffHeight[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (WORD)(255 * nYSum / nSum);

				if (bCheckMinGV2 == true)	//MinGV2?????? (-)
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nSum);
				else
					m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nSum);
			}
			else
			{ // ?? ???????????  Min?? ?? ???? ?????? / 
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

					if (bCheckMinGV2 == true)	//MinGV2??????(-)
						m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nSum);
					else
						m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nSum);
				}
			}
		}
	}
}

void Raw3D_Calculation::CalcHBImage_FromTop(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
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
			if (bCheckMinGV2 == true)	//MinGV2?????? (-)
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

// Beam ???? parameter ????
void Raw3D_Calculation::CalcHBImage_SKPAD_MinGV1(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)
{
	BYTE* pBuf;
	BYTE* pBufX;

	int nLinenum = m_param.n3DScanParam;
	bool bCheckMinGV2 = false;
	///////////////////////////
	// Beam ???????? ??????

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

	int BeamCenter = 0;  // Beam ????????
	int BeamCenter2 = 0;  // Beam ????????
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

	//Beam ???? ??????
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
	// Height Buffer, Bright Buffer ??????

	long x = 0, y = 0;
	long nGV;

	long nBSum; // Bright Buffer ??
	long nBSum2; // Bright Buffer ?? Mingv2
	long nYSum; // Height Buffer ??
	long nSum; // Height Buffer ??

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
		nFirst_y = 0; // ???? ??????
		bool bBumpAndBeam = false;
		bool bNoException = true; // false???? ???????? ???? ?????? ??

		for (y = 0; y < m_szRawImage.cy; y++)
		{
			pBuf = pBufX + y * m_szRawImage.cx;

			int nGV = *pBuf - nMinGV1;
			if (nGV < 0) nGV = 0;

			if (nGV > 0) // nGVMin ?????? ???? Bright Buffer?? ???? , Height Buffer ?? ????
			{
				nSum += nGV;
				nYSum += nGV * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // ?????? ??????
				{
					bCheck = true;
					nFirst_y = y;  // ???? ??????

					if (nFirst_y < BeamCenter - nLinenum)
						bBumpAndBeam = true; // ??????????
				}
			}
			else if (bCheck)  // 0?? ?????? ?? 
			{
				if (nCnt < 2)  // 1?? ?????? ???????? 
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
					if (y < BeamCenter)		//???? ???? ?????? ????????
						bNoException = false; // ?????????? ????????

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
			bNoException = false;  // ???????? ????????
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

				if (nGV > 0) // nGVMin ?????? ???? Bright Buffer?? ???? , Height Buffer ?? ????
				{
					nSum += nGV;
					nYSum += nGV * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nBSum += *pBuf;
					nCnt++;

					if (!bCheck) // ?????? ??????
					{
						bCheck = true;
						nFirst_y = y;  // ???? ??????

						if (nFirst_y < BeamCenter - nLinenum)
							bBumpAndBeam = true; // ??????????
					}
				}
				else if (bCheck)  // 0?? ?????? ?? 
				{
					if (nCnt < 2)  // 1?? ?????? ???????? 
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
						if (y < BeamCenter)		//???? ???? ?????? ????????
							bNoException = false; // ?????????? ????????

						break;
					}
				}
			}
		}

		//// --> nYSum, nSum ?? nFirst_y (??????????)

		////////////////////////////////////////////////
		//   ???????? - Beam ?? Bump?? ???? ?????? ????
		////////////////////////////////////////////////

		int nLast_y = 0;
		if (bBumpAndBeam && bNoException)
		{
			// ?????? ?????? - ???????? ???????? ????
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

			// ?????? ??????, ?????? ????
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
			//// --> nLast_y ????
			// ????

			nSum = nYSum = 0;
			if (bCheckMinGV2 == false)
				nBSum = 0;

			if (IsMin == false)
			{
				nLast_y = m_szRawImage.cy - 1; // ???????? - Bump???? Beam?? ???? ???? ?????? ????
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

			if (bCheckMinGV2 == true)	//MinGV2??????(-)
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = -(short)(nBSum);
			else
				m_ppBuffBright[nFrameIndex + ptDataPos.y][x + ptDataPos.x] = (short)(nBSum);
		}
	}

}


void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200520 ?????? Merge
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

	int BeamThk = nLinenum / 2; //?????? ???????????? ???? ??????

	//?????? ?? ????, ?? ???? ?? ????????

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

	for (x = 0; x < m_szRawImage.cx; x++) //?? x????
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

		for (y = 0; y < m_szRawImage.cy; y++) //?? y?? ??????
		{

			pBuf = pBufX + y * m_szRawImage.cx;

			if (*pBuf > nMinGV1) // y?? ???? ???? mingv1 ???? ?????? -> ???????? ???? ???? ??????????,
			{
				nSum += *pBuf - nMinGV1;
				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
				{
					bCheck = true;
					nFirst_y = y;
				}
			}
			else if (bCheck) //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
			{
				if (nCnt == 1) // ???? ???????????? 1?????? ?????????? ???????? ????????.
				{
					nSum = 0;
					nYSum = 0;
					nBSum = 0;
					bCheck = false;
					nCnt = 0;
					nFirst_y = 0;
				}
				else //1???????? ?????? ?? check?? ???????????? ????????.
				{
					nLast_y = nFirst_y + nCnt - 1;
					break;
				}
			}
		}

		if ((nSum == 0 || nFirst_y >= nStrongestBeamLine - BeamThk) && bUseDualMinGV) //???? ?????? nSum?? 0?????? ?????????? ?????????? ????????, mingv2?? ????????. beamthk ?????? ???? ???? ?????? ?????? ??????????????;
		{

			nSum2 = 0;
			nYSum2 = 0;
			nBSum2 = 0;

			bCheck = false;
			int nCnt2 = 0;
			int nFirst_y2 = 0;
			int nLast_y2 = 0;

			for (long y = 0; y < nStrongestBeamLine - 2 * BeamThk; y++) //mingv2?? ???????? ??????. all m_szRawImage.cy ???? ?????????? ??????, cut nStrongestBeamLine-2*BeamThk
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				if (*pBuf > nMinGV2) // y?? ???? ???? mingv2 ???? ?????? -> ???????? ???? ???? ??????????,
				{
					nSum2 += *pBuf - nMinGV2;
					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nCnt2++;

					if (!bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
					{
						bCheck = true;
						nFirst_y2 = y;
						bCheckMinGV2 = true;
					}
				}
				else if (bCheck)  //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
				{
					if (nCnt2 == 1) // ???? ???????????? 1?????? ?????????? ???????? ????????. -> (bCheckMinGV2 = false)???????? mingv1???? ???????? ???? ????. 
					{
						nSum2 = 0;
						nYSum2 = 0;
						nBSum2 = 0;
						bCheck = false;
						nCnt2 = 0;
						nFirst_y2 = 0;
						bCheckMinGV2 = false;
					}
					else //1???????? ?????? ?? check?? ???????????? ????????. (???? ?????????? ?????????????? ???????????? ??????)
					{
						nLast_y2 = nFirst_y2 + nCnt2 - 1;
						break;
					}
				}
			}
		}
		if (!bCheckMinGV2) //mingv1 ?????? ???????????? ????
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
		else //(mingv2?????? ?????? ?????? ????
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


////#1. BeamThk ???? ????
void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump_AutoBeamThickness(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200623 ?????? ??????
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

	//?????? ?? ????, ?? ???? ?? ????????

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

	for (x = 0; x < m_szRawImage.cx; x++) //?? x????
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

		for (y = 0; y < m_szRawImage.cy; y++) //?? y?? ??????
		{

			pBuf = pBufX + y * m_szRawImage.cx;

			if (*pBuf > nMinGV1) // y?? ???? ???? mingv1 ???? ?????? -> ???????? ???? ???? ??????????,
			{
				nSum += *pBuf - nMinGV1;
				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy - 1 - y);
				nBSum += *pBuf;
				nCnt++;

				if (!bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
				{
					bCheck = true;
					nFirst_y = y;
				}
			}
			else if (bCheck) //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
			{
				if (nCnt == 1) // ???? ???????????? 1?????? ?????????? ???????? ????????.
				{
					nSum = 0;
					nYSum = 0;
					nBSum = 0;
					bCheck = false;
					nCnt = 0;
					nFirst_y = 0;
				}
				else //1???????? ?????? ?? check?? ???????????? ????????.
				{
					nLast_y = nFirst_y + nCnt - 1;
					break;
				}
			}
		}

		if ((nSum == 0 || nFirst_y >= nStrongestBeamLine - BeamThk) && bUseDualMinGV) //???? ?????? nSum?? 0?????? ?????????? ?????????? ????????, mingv2?? ????????. beamthk ?????? ???? ???? ?????? ?????? ??????????????;
		{

			nSum2 = 0;
			nYSum2 = 0;
			nBSum2 = 0;

			bCheck = false;
			int nCnt2 = 0;
			int nFirst_y2 = 0;
			int nLast_y2 = 0;

			for (long y = 0; y < nStrongestBeamLine - 2 * BeamThk; y++) //mingv2?? ???????? ??????. all m_szRawImage.cy ???? ?????????? ??????, cut nStrongestBeamLine-2*BeamThk
			{
				pBuf = pBufX + y * m_szRawImage.cx;

				if (*pBuf > nMinGV2) // y?? ???? ???? mingv2 ???? ?????? -> ???????? ???? ???? ??????????,
				{
					nSum2 += *pBuf - nMinGV2;
					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy - 1 - y);
					nBSum2 += *pBuf;
					nCnt2++;

					if (!bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
					{
						bCheck = true;
						nFirst_y2 = y;
						bCheckMinGV2 = true;
					}
				}
				else if (bCheck)  //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
				{
					if (nCnt2 == 1) // ???? ???????????? 1?????? ?????????? ???????? ????????. -> (bCheckMinGV2 = false)???????? mingv1???? ???????? ???? ????. 
					{
						nSum2 = 0;
						nYSum2 = 0;
						nBSum2 = 0;
						bCheck = false;
						nCnt2 = 0;
						nFirst_y2 = 0;
						bCheckMinGV2 = false;
					}
					else //1???????? ?????? ?? check?? ???????????? ????????. (???? ?????????? ?????????????? ???????????? ??????)
					{
						nLast_y2 = nFirst_y2 + nCnt2 - 1;
						break;
					}
				}
			}
		}
		if (!bCheckMinGV2) //mingv1 ?????? ???????????? ????
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
		else //(mingv2?????? ?????? ?????? ????
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





//#2. RowSum ????????????
//void Raw3D_Calculation::CalcHBImage_ConsiderZeroBump(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV)//200623 ?????? RowSum
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
//	//int BeamThk = nLinenum/2; //?????? ???????????? ???? ??????
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
//	//?????? ?? ????, ?? ???? ?? ????????
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
//	for ( x=0; x< m_szRawImage.cx; x++) //?? x????
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
//		for( y = 0; y < m_szRawImage.cy ; y++) //?? y?? ??????
//		{
//			
//			pBuf = pBufX + y * m_szRawImage.cx;
//			
//			if (*pBuf>nMinGV1) // y?? ???? ???? mingv1 ???? ?????? -> ???????? ???? ???? ??????????,
//			{
//				nSum += *pBuf - nMinGV1;
//				nYSum += (*pBuf - nMinGV1) * (m_szRawImage.cy -1 -y);
//				nBSum += *pBuf;
//				nCnt++;
//				
//				if ( !bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
//				{
//					bCheck = true;
//					nFirst_y = y;
//				}
//			}
//			else if (bCheck) //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
//			{
//				if(nCnt==1) // ???? ???????????? 1?????? ?????????? ???????? ????????.
//				{
//					nSum = 0;
//					nYSum = 0;
//					nBSum = 0;
//					bCheck = false;
//					nCnt = 0;
//					nFirst_y = 0;
//				}
//				else //1???????? ?????? ?? check?? ???????????? ????????.
//				{
//					nLast_y = nFirst_y + nCnt -1;
//					break;
//				}
//			}
//		}
//
//		if((nSum ==0 || nFirst_y > nCutOffRow-2) && bUseDualMinGV) //???? ?????? nSum?? 0?????? ?????????? ?????????? ????????, mingv2?? ????????. beamthk ?????? ???? ???? ?????? ?????? ??????????????;
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
//			for(long y = 0; y < nCutOffRow-2; y++) //mingv2?? ???????? ??????. all m_szRawImage.cy ???? ?????????? ??????, cut nStrongestBeamLine-2*BeamThk
//			{
//				pBuf = pBufX + y * m_szRawImage.cx;
//				
//				if (*pBuf>nMinGV2) // y?? ???? ???? mingv2 ???? ?????? -> ???????? ???? ???? ??????????,
//				{
//					nSum2 += *pBuf - nMinGV2;
//					nYSum2 += (*pBuf - nMinGV2) * (m_szRawImage.cy -1 -y);
//					nBSum2 += *pBuf;
//					nCnt2++;
//					
//					if ( !bCheck) // ?? ?????? ???? 1?? ???????? Check ???? ????????. 
//					{
//						bCheck = true;
//						nFirst_y2 = y;
//						bCheckMinGV2 = true;
//					}
//				}
//				else if (bCheck)  //Check?? ???????? ????????  mingv1???? ???? ???? ???? ???????? ?????? ????????.
//				{
//					if(nCnt2 == 1) // ???? ???????????? 1?????? ?????????? ???????? ????????. -> (bCheckMinGV2 = false)???????? mingv1???? ???????? ???? ????. 
//					{
//						nSum2 = 0;
//						nYSum2 = 0;
//						nBSum2 = 0;
//						bCheck = false;
//						nCnt2 = 0;
//						nFirst_y2 = 0;
//						bCheckMinGV2=false;
//					}
//					else //1???????? ?????? ?? check?? ???????????? ????????. (???? ?????????? ?????????????? ???????????? ??????)
//					{
//						nLast_y2 = nFirst_y2 + nCnt2 -1;
//						break;				
//					}
//				}	
//			}
//		}
//		if (!bCheckMinGV2) //mingv1 ?????? ???????????? ????
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
//		else //(mingv2?????? ?????? ?????? ????
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

void Raw3D_Calculation::ConvertDisplayImage(DisplayMode displayMode, CCRect rtROI, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY)
{

	int nOverlapNum = (rtROI.x - m_nOverlapStartPos) / (m_szRawImage.cx - m_nOverlapSize);

	if (m_nOverlapStartPos > 0 && rtROI.x > m_nOverlapStartPos)
		nOverlapNum++;

	int nOffsetX = m_nOverlapSize * nOverlapNum;

	double dRatio = 1.0 / m_nOverlapSize;
	for (int n = 0; n < m_nOverlapSize; n++)
	{
		m_pdOverlap[n] = dRatio * (double)n;
	}

	switch (displayMode)
	{
	case DisplayMode::HeightImage:
		for (int y = rtROI.y; y < rtROI.y + rtROI.cy; y++)
		{
			for (int x = rtROI.x, o =0; x < rtROI.x + rtROI.cx; x++,o++)
			{
				
				if (IsMinGV2(x + nOffsetX, y) == true)
				{
					m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = 0;
				}
				else
				{
					double b = (BYTE)(m_ppBuffHeight[y][x + nOffsetX] / m_szRawImage.cy);
					if (o < m_nOverlapSize)
					{ //???? ???????????? ????????
						double a = m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX];						
						m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = (BYTE)((double)a * m_pdOverlap[o] + (double)b * m_pdOverlap[m_nOverlapSize - o - 1]);
					}
					else
						m_ppMainImage[y + nDisplayOffsetY][x + nDisplayOffsetX] = b;
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