#include "pch.h"
#include "Raw3D_RawData.h"

Raw3D_RawData* Raw3D_RawData::GetInstance()
{
	if (instance == NULL)
	{
		instance = new Raw3D_RawData();
	}
	return instance;
}

Raw3D_RawData::Raw3D_RawData()
{
	m_ppBuffHeight = NULL;
	m_ppBuffBright = NULL;

	m_ppBuffRaw = NULL;

	m_nCurrFrameNum = 0;
}

Raw3D_RawData::~Raw3D_RawData()
{
	DeleteBuffer();
	//PurgeInstance();//이게 여기서 되는지 모르겠네
}

void Raw3D_RawData::Initialize(CSize szImageBuffer, CSize szMaxRawImage, int nMaxOverlapSize, int nMaxFrameNum)
{
	m_szMaxRawImage = szMaxRawImage;
	m_nMaxOverlapSize = nMaxOverlapSize;

	m_szImageBuffer.cx = szImageBuffer.cx + roundf((float)szImageBuffer.cx / (float)m_szRawImage.cx + 0.5) * m_nMaxOverlapSize;
	m_szImageBuffer.cy = szImageBuffer.cy;

	CreateBuffer(m_szImageBuffer.cx, m_szImageBuffer.cy);
	CreateRawBuffer(nMaxFrameNum);
}

void Raw3D_RawData::DeleteBuffer()
{
	if (m_ppBuffHeight != NULL)
	{
		for (int i = 0; i < m_szImageBuffer.cy; i++)
		{
			if (i % BLOCK_SIZE == 0)
				delete[]m_ppBuffHeight[i];
		}
		delete[]m_ppBuffHeight;
	}
	if (m_ppBuffBright != NULL)
	{
		for (int i = 0; i < m_szImageBuffer.cy; i++)
		{
			if (i % BLOCK_SIZE == 0)
				delete[]m_ppBuffBright[i];
		}
		delete[]m_ppBuffBright;
	}

	m_ppBuffHeight = NULL;
	m_ppBuffBright = NULL;
}


void Raw3D_RawData::CreateBuffer(int n3DImageWidth, int n3DImageHeight)
{
	DeleteBuffer();

	if (m_ppBuffHeight != NULL || m_ppBuffBright != NULL)
		return;

	m_ppBuffHeight = new WORD * [n3DImageHeight];
	m_ppBuffBright = new short* [n3DImageHeight];

	int nIdx = 0, nCurrent = 0;
	for (int i = 0; i < n3DImageHeight; i++)
	{
		nCurrent = i % BLOCK_SIZE;
		if (nCurrent == 0)
		{
			m_ppBuffBright[i] = new short[n3DImageWidth * BLOCK_SIZE];
			m_ppBuffHeight[i] = new WORD[n3DImageWidth * BLOCK_SIZE];
			nIdx = i;
		}
		else
		{
			m_ppBuffBright[i] = &m_ppBuffBright[nIdx][n3DImageWidth * nCurrent];
			m_ppBuffHeight[i] = &m_ppBuffHeight[nIdx][n3DImageWidth * nCurrent];
		}
	}
}

void Raw3D_RawData::CreateRawBuffer(int nMaxSnapFrameNum)
{
	//ClearBuffImageAddress();

	m_nMaxSnapFrameNum = nMaxSnapFrameNum;
	if (m_ppBuffRaw != NULL)
	{
		DeleteRawBuffer();
	}
	m_ppBuffRaw = new LPBYTE[m_nMaxSnapFrameNum];
	for (int i = 0; i < m_nMaxSnapFrameNum; i++)
	{
		m_ppBuffRaw[i] = new BYTE[m_szMaxRawImage.cx * m_szMaxRawImage.cy];
	}
	//return m_ppBuffRaw;
}

void Raw3D_RawData::DeleteRawBuffer()
{
	if (m_ppBuffRaw != NULL)
	{
		delete[] m_ppBuffRaw;
		m_ppBuffRaw = NULL;
	}
}

/*
void Raw3D_RawData::SaveRawImage(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize)
{
	int nOverlapNum = ptRB.x / (nFoV - nOverlapSize);
	int nROIWidth = ptRB.x - ptLT.x + (nOverlapSize * nOverlapNum);
	int nROIHeight = ptRB.y - ptLT.y;

	if ((ptLT.x > 0 && ptRB.x < m_szImageBuffer.cx) && (ptLT.y > 0 && ptRB.y < m_szImageBuffer.cy))
	{
		CFile file;
		if (file.Open((LPCTSTR)(sFileName), CFile::modeCreate | CFile::modeWrite | CFile::typeBinary) != NULL)
		{
			file.Write(&nROIWidth, sizeof(int));
			file.Write(&nROIHeight, sizeof(int));

			for (int n = ptLT.y; n < ptRB.y; n++)
			{
				file.Write(&m_ppBuffHeight[n][ptLT.x], nROIWidth * sizeof(WORD));
			}

			for (int n = ptLT.y; n < ptRB.y; n++)
			{
				file.Write(&m_ppBuffBright[n][ptLT.x], nROIWidth * sizeof(short));
			}

			file.Close();
		}
	}
	else
		AfxMessageBox("[Error] Selected range is bigger than 3DImage Buffer size.");
}


BOOL Raw3D_RawData::ReadRawImage(CString sFileName)
{
	CFile file;
	int nROIWidth = 0, nROIHeight = 0;
	if (file.Open((LPCTSTR)(sFileName), CFile::modeRead | CFile::typeBinary) != NULL)
	{
		file.Read(&nROIWidth, sizeof(int));
		file.Read(&nROIHeight, sizeof(int));

		if (nROIWidth > m_szImageBuffer.cx || nROIHeight > m_szImageBuffer.cy)
		{
			AfxMessageBox("Image is bigger than 3DImage Buffer size.");
			return FALSE;
		}

		for (int n = 0; n < nROIHeight; n++)
		{
			memset(m_ppBuffHeight[n], 0, nROIWidth * sizeof(WORD));
			memset(m_ppBuffBright[n], 0, nROIWidth * sizeof(short));
		}

		for (int n = 0; n < nROIHeight; n++)
		{
			file.Read(&m_ppBuffHeight[n][0], nROIWidth * sizeof(WORD));
		}

		for (int n = 0; n < nROIHeight; n++)
		{
			file.Read(&m_ppBuffBright[n][0], nROIWidth * sizeof(short));
		}
		file.Close();
		return TRUE;
	}

	return FALSE;
}

void Raw3D_RawData::SaveRawImageCSV(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize, bool bBrightData)
{
	int nROIWidth = ptRB.x - ptLT.x;
	int nROIHeight = ptRB.y - ptLT.y;

	CStdioFile file;
	if (file.Open((LPCTSTR)(sFileName), CFile::modeCreate | CFile::modeWrite) != NULL)
	{
		CString sTemp = "", sTemp2 = "";
		sTemp.Format("Width:,%d\n", nROIWidth);
		file.WriteString(sTemp);
		sTemp.Format("Height:,%d\n", nROIHeight);
		file.WriteString(sTemp);

		if (bBrightData == true)
		{
			for (int y = ptLT.y; y < ptRB.y; y++)
			{
				sTemp = "";
				for (int x = ptLT.x; x < ptRB.x; x++)
				{
					CPoint ptData = GetPosViewtoData(CPoint(x, y), nFoV, nOverlapSize);
					if ((ptData.x >= 0 && ptData.x < m_szImageBuffer.cx) && (ptData.y >= 0 && ptData.y < m_szImageBuffer.cy))
						sTemp2.Format("%d,", m_ppBuffBright[ptData.y][ptData.x]);
					else
						sTemp2.Format("%d,", -9999);

					sTemp += sTemp2;
				}
				sTemp += "\n";
				file.WriteString(sTemp);
			}
		}
		else
		{
			for (int y = ptLT.y; y < ptRB.y; y++)
			{
				sTemp = "";
				for (int x = ptLT.x; x < ptRB.x; x++)
				{
					CPoint ptData = GetPosViewtoData(CPoint(x, y), nFoV, nOverlapSize);
					if ((ptData.x >= 0 && ptData.x < m_szImageBuffer.cx) && (ptData.y >= 0 && ptData.y < m_szImageBuffer.cy))
						sTemp2.Format("%d,", m_ppBuffHeight[ptData.y][ptData.x]);
					else
						sTemp2.Format("%d,", -9999);

					sTemp += sTemp2;
				}
				sTemp += "\n";
				file.WriteString(sTemp);
			}
		}
		file.Close();
	}
}

BOOL Raw3D_RawData::ReadRawImageCSV(CString sFileName)
{
	CStdioFile file;
	int nROIWidth = 0, nROIHeight = 0;
	CString sRead, sData;
	if (file.Open((LPCTSTR)(sFileName), CFile::modeRead) != NULL)
	{
		file.ReadString(sRead);
		AfxExtractSubString(sData, sRead, 1, ',');
		nROIWidth = atoi(sData);

		file.ReadString(sRead);
		AfxExtractSubString(sData, sRead, 1, ',');
		nROIHeight = atoi(sData);

		if (nROIWidth > m_szImageBuffer.cx || nROIHeight > m_szImageBuffer.cy)
		{
			AfxMessageBox("Image is bigger than 3DImage Buffer");
			return FALSE;
		}

		for (int n = 0; n < nROIHeight; n++)
		{
			memset(m_ppBuffHeight[n], 0, nROIWidth * sizeof(WORD));
			memset(m_ppBuffBright[n], 0, nROIWidth * sizeof(short));
		}

		for (int y = 0; y < nROIHeight; y++)
		{
			file.ReadString(sRead);
			for (int x = 0; x < nROIWidth; x++)
			{
				AfxExtractSubString(sData, sRead, x, ',');
				m_ppBuffHeight[y][x] = (WORD)atoi(sData);
			}
		}

		for (int y = 0; y < nROIHeight; y++)
		{
			file.ReadString(sRead);
			for (int x = 0; x < nROIWidth; x++)
			{
				AfxExtractSubString(sData, sRead, x, ',');
				m_ppBuffBright[y][x] = (short)atoi(sData);
			}
		}
		file.Close();
		return TRUE;
	}

	return FALSE;
}
*/
CSize Raw3D_RawData::GetMaxRawImageSize()
{
	return m_szMaxRawImage;
}

CSize Raw3D_RawData::GetRawDataBufferSize()
{
	return m_szImageBuffer;
}

WORD** Raw3D_RawData::GetHeightBuffer()
{
	return m_ppBuffHeight;
}

short** Raw3D_RawData::GetBrightBuffer()
{
	return m_ppBuffBright;
}

LPBYTE* Raw3D_RawData::GetRawBuffer()
{
	return m_ppBuffRaw;
}
/*
void Raw3D_RawData::SendLog(CString strMsg)
{
	if (m_hLogForm != NULL)
	{
		CString sLogMsg = strMsg;
//////		::SendMessage(m_hLogForm, WM_MESSAGE_LOG_DATA, LOG_INDEX_INSPECTION, (LPARAM)&sLogMsg);
	}
}*/