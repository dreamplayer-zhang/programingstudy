#pragma once

struct Parameter3D
{
	int n3DScanParam;
	int nSnapFrameRest;
};

class Raw3D_RawData
{
public:
	static Raw3D_RawData* GetInstance();
	void Initialize(CSize szImageBuffer, CSize szMaxRawImage, int nMaxOverlapSize, int nMaxFrameNum);
/*
	void SaveRawImage(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize);
	BOOL ReadRawImage(CString sFileName);

	void SaveRawImageCSV(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize, bool bBrightData);
	BOOL ReadRawImageCSV(CString sFileName);
*/
	WORD** GetHeightBuffer();
	short** GetBrightBuffer();
	LPBYTE* GetRawBuffer();

	CSize GetMaxRawImageSize();
	CSize GetRawDataBufferSize();

	//void SendLog(CString strMsg);

	inline CPoint GetPosViewtoData(CPoint ptView, int nFoV, int nOverlapSize, int nDisplayOffsetX = 0, int nDisplayOffsetY = 0)
	{
		int nOverlapNum = ptView.x / (nFoV - nOverlapSize);
		CPoint ptResult = CPoint(ptView.x + (nOverlapNum * nOverlapSize) - nDisplayOffsetX, ptView.y - nDisplayOffsetY);

		return ptResult;
	}


private:
	Raw3D_RawData();
	Raw3D_RawData(const Raw3D_RawData& rhs);
	~Raw3D_RawData();

	void DeleteBuffer();
	void CreateBuffer(int n3DImageWidth, int n3DImageHeight);

	void DeleteRawBuffer();
	void CreateRawBuffer(int nMaxSnapFrameNum);

	HWND m_hLogForm;

	WORD** m_ppBuffHeight;
	short** m_ppBuffBright;

	LPBYTE* m_ppBuffRaw;

	CSize m_szImageBuffer;
	CSize m_szRawImage;
	CSize m_szMaxRawImage;

	int m_nMaxOverlapSize;
	int m_nMaxSnapFrameNum;

	int m_nCurrFrameNum;
};
static Raw3D_RawData* instance = NULL;
