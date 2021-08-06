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
	void Initialize(WORD* ppBuffHeight, short* ppBuffBright, CCSize szImageBuffer, LPBYTE ppBuffRaw, CCSize szMaxRawImage, int nMaxOverlapSize, int nMaxFrameNum);
/*
	void SaveRawImage(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize);
	BOOL ReadRawImage(CString sFileName);

	void SaveRawImageCSV(CString sFileName, CPoint ptLT, CPoint ptRB, int nFoV, int nOverlapSize, bool bBrightData);
	BOOL ReadRawImageCSV(CString sFileName);
*/
	WORD** GetHeightBuffer();
	short** GetBrightBuffer();
	LPBYTE* GetRawBuffer();

	CCSize GetMaxRawImageSize();
	CCSize GetRawDataBufferSize();

	//void SendLog(CString strMsg);
	
	inline CCPoint GetPosViewtoData(CCPoint ptView, int nFoV, int nOverlapSize, int nDisplayOffsetX = 0, int nDisplayOffsetY = 0)
	{
		int nOverlapNum = ptView.x / (nFoV - nOverlapSize);
		CCPoint ptResult;
		ptResult.x = ptView.x + (nOverlapNum * nOverlapSize) - nDisplayOffsetX;
		ptResult.y = ptView.y - nDisplayOffsetY;
		return ptResult;
	}


private:
	Raw3D_RawData();
	Raw3D_RawData(const Raw3D_RawData& rhs);
	~Raw3D_RawData();
	/*
	void DeleteBuffer();
	void CreateBuffer(int n3DImageWidth, int n3DImageHeight);
	void DeleteRawBuffer();
	void CreateRawBuffer(int nMaxSnapFrameNum);
	*/

	WORD** m_ppBuffHeight;
	short** m_ppBuffBright;

	LPBYTE* m_ppBuffRaw;

	CCSize m_szImageBuffer;
	CCSize m_szRawImage;
	CCSize m_szMaxRawImage;

	int m_nMaxOverlapSize;
	int m_nMaxSnapFrameNum;

	int m_nCurrFrameNum;
};
static Raw3D_RawData* instance = NULL;
