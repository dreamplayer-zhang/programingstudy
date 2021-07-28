#include "pch.h"
#include "Raw3DManager.h"
#include <omp.h> 

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


Raw3DManager::Raw3DManager()
{
	m_ppBuffHeight = NULL;
	m_ppBuffBright = NULL;
	m_ppBuffFG = NULL;
	m_pThreadCalc = NULL;
	m_hLogForm = NULL;
	m_pDlgGrabber = NULL;

	m_szRawImage = CSize(0, 0);
	m_szImageBuffer = CSize(0, 0);

	RAWDATA = Raw3D_RawData::GetInstance();
}


Raw3DManager::~Raw3DManager()
{
	//ClearBuffImageAddress();
}

void Raw3DManager::Initialize(HWND pDlgGrabber, LPBYTE* ppMainImage, int n3DImageWidth, int n3DImageHeight, CSize szRawImage, int nMaxOverlapSize)
{
	m_pDlgGrabber = pDlgGrabber;

	m_ppImageMain = ppMainImage;

	m_szRawImage = szRawImage;
	m_nMaxOverlapSize = nMaxOverlapSize;

	m_szImageBuffer.cx = n3DImageWidth + roundf((float)n3DImageWidth / (float)m_szRawImage.cx + 0.5) * m_nMaxOverlapSize;	//Overlap크기를 포함한 Width
	m_szImageBuffer.cy = n3DImageHeight;

	if (RAWDATA->GetHeightBuffer() == NULL || RAWDATA->GetBrightBuffer() == NULL)
	{
		//Error로그
	//	AfxMessageBox("Failed to allocate 3D Height buffer & Bright buffer. Please check the setting.");
		return;
	}

	m_ppBuffHeight = RAWDATA->GetHeightBuffer();
	m_ppBuffBright = RAWDATA->GetBrightBuffer();
}

void Raw3DManager::SetLogFormHandle(HWND hLogHandle)
{
	m_hLogForm = hLogHandle;
}


//스냅할 총 프레임 개수
LPBYTE* Raw3DManager::CreateRawBuffer(int nSnapFrameNum)
{
//	CString sLog = "";
//	sLog.Format("SnapFrameNum : %d", nSnapFrameNum);
//	SendLog(sLog);
	DeleteRawBuffer();

	m_nSnapFrameNum = nSnapFrameNum;
	m_ppBuffFG = new LPBYTE[m_nSnapFrameNum];

	return m_ppBuffFG;
}

void Raw3DManager::DeleteRawBuffer()
{
	if (m_ppBuffFG != NULL)
	{
		delete[] m_ppBuffFG;
		m_ppBuffFG = NULL;
	}
}
Raw3D_RawData* Raw3DManager::GetRawData()
{
	return RAWDATA;
}
LPBYTE* Raw3DManager::GetRawBuffFG()
{
	return m_ppBuffFG;
}

UINT CheckThread(LPVOID lParam)
{
	Raw3DManager* pRaw3DMgr = ((Raw3DManager*)lParam);
	pRaw3DMgr->CheckSnapCalcDone();

	return 0;
}

void Raw3DManager::MakeImage(ConvertMode convertMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos
	, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
	, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param)
{
	if (m_pThreadCalc != NULL)
	{
		//Thread 비정상종료 안전장치
		for (int n = 0; n < m_nThreadNum; n++)
		{
			if (m_pThreadCalc[n].GetCalcState() != Calc3DState::Done)
			{
				m_pThreadCalc[n].StopCalc();

			//	CString sLog = "";
			//	sLog.Format("[Error] Forced Stop Calc. Thread[%d]", n);
			//	SendLog(sLog);
			}
		}

		delete[] m_pThreadCalc;
		m_pThreadCalc = NULL;
	}

	m_nThreadNum = nThreadNum;
	m_nSnapFrameNum = nSnapFrameNum;
	CreateRawBuffer(m_nSnapFrameNum);
	m_pnCurrFrameNum = pnCurrFrameNum;

	if (nOverlapSize > m_nMaxOverlapSize)
		nOverlapSize = m_nMaxOverlapSize;

	m_pThreadCalc = new Raw3D_Calculation[m_nThreadNum];
	for (int n = 0; n < m_nThreadNum; n++)
	{
		m_pThreadCalc[n].Initialize(n, m_nThreadNum, m_szImageBuffer, m_szRawImage, m_ppImageMain, m_ppBuffHeight, m_ppBuffBright, m_ppBuffFG, m_nSnapFrameNum);
		m_pThreadCalc[n].SetLogFormHandle(m_hLogForm);
		m_pThreadCalc[n].StartCalculation(convertMode, calcMode, displayMode, ptDataPos, nMinGV1, nMinGV2, nOverlapStartPos, nOverlapSize, nDisplayOffsetX, nDisplayOffsetY, bRevScan, bUseMinGV2, m_pnCurrFrameNum, param);
	}
	AfxBeginThread(CheckThread, this);
}

void Raw3DManager::CheckSnapCalcDone()
{
	bool bDone = false;
	bool bError = false;
	int nSameFrameCount = 0;
	int nPrevFrameNum = 0;
	while (true)
	{
		bDone = true;
		for (int n = 0; n < m_nThreadNum; n++)
		{
			if (m_pThreadCalc[n].GetCalcState() != Calc3DState::Done)
			{
				bDone = false;
			}
			if (m_pThreadCalc[n].GetCalcState() == Calc3DState::ErrorCalc)
			{
				m_pThreadCalc[n].StopCalc();

			//	CString s = "";
			//	s.Format("Snap Error Thread[%d]", n);
			//	SendLog(s);
				bError = true;
			}
		}
		if (bDone == true)
		{
			if (m_pDlgGrabber != NULL)
			{
			//	::SendMessage(m_pDlgGrabber, WM_MESSAGE_FRAMEGRABBER_SNAP_DONE, 0, 1);
			//	SendLog("Snap Done");
			}

			break;
		}
		if (bError == true)
		{
			if (m_pDlgGrabber != NULL)
			{
			//	::SendMessage(m_pDlgGrabber, WM_MESSAGE_FRAMEGRABBER_SNAP_DONE, 0, -1);
			}
			break;
		}

		/*if(m_nSnapFrameNum > 58000 && m_nSnapFrameNum < 60000)	//snap error 테스트용
		{
			for (int n = 0; n < m_nThreadNum; n++)
			{
				m_pThreadCalc[n].StopCalc();
			}

			CString s = "";
			s.Format("[Error] Test Cannot snap next frames / Curr Frame Num = %d / Need Snap Frame Num = %d", *m_pnCurrFrameNum, m_nSnapFrameNum);
			SendLog(s);

			if (m_pDlgGrabber != NULL)
			{
				::SendMessage(m_pDlgGrabber, WM_MESSAGE_FRAMEGRABBER_SNAP_DONE, 0, -1);
			}
			break;
		}*/

		if (*m_pnCurrFrameNum == nPrevFrameNum)
		{
			nSameFrameCount++;
			if (MAX_SAME_FRAME_COUNT < nSameFrameCount)
			{
				for (int n = 0; n < m_nThreadNum; n++)
				{
					m_pThreadCalc[n].StopCalc();
				}

				//CString s = "";
				//s.Format("[Error] Cannot snap next frames / Curr Frame Num = %d / Need Snap Frame Num = %d", *m_pnCurrFrameNum, m_nSnapFrameNum);
				//SendLog(s);

				if (m_pDlgGrabber != NULL)
				{
				//	::SendMessage(m_pDlgGrabber, WM_MESSAGE_FRAMEGRABBER_SNAP_DONE, 0, -1);
				}
				break;
			}
		}
		else
		{
			nPrevFrameNum = *m_pnCurrFrameNum;
		}

		Sleep(1);
	}
}

void Raw3DManager::StopCalculation()
{
	for (int n = 0; n < m_nThreadNum; n++)
	{
		if (m_pThreadCalc != NULL)
		{
			m_pThreadCalc[n].StopCalc();
		}
	}
}
/*
void Raw3DManager::SendLog(CString strMsg)
{
	if (m_hLogForm != NULL)
	{
	//	CString sLogMsg = strMsg;
	//	::SendMessage(m_hLogForm, WM_MESSAGE_LOG_DATA, LOG_INDEX_MAPPING, (LPARAM)&sLogMsg);	//mapping이 제일 로그가 없으니까 임시로..
	}
}*/

CSize Raw3DManager::GetImageBufferSize()
{
	if (m_szImageBuffer.cx == 0 || m_szImageBuffer.cy == 0)
		//AfxMessageBox("[Warning] 3D Grabber is not initialized. - GetImageBufferSize()");

	return m_szImageBuffer;
}

CSize Raw3DManager::GetRawImageSize()
{
	if (m_szRawImage.cx == 0 || m_szRawImage.cy == 0)
		//AfxMessageBox("[Warning] 3D Grabber is not initialized. - GetRawImageSize()");

	return m_szRawImage;
}

WORD** Raw3DManager::GetHeightBuffer()
{
	return m_ppBuffHeight;
}

short** Raw3DManager::GetBrightBuffer()
{
	return m_ppBuffBright;
}