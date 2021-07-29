#pragma once
#include "pch.h"
#include "Raw3D_RawData.h"

#define MAX_RAW_Y 256
static enum ConvertMode
{
	NoConvert,
	Bump,
	CVT_MODE_NUM
};

static enum Calc3DMode
{
	Normal,	//peak2
	FromTop,
	SK_Under15,
	ConsiderZero,
	ConsiderZeroAutoBeamThk,
	CALC_MODE_NUM
};

static enum DisplayMode
{
	HeightImage,
	BrightImage,
	DISP_MODE_NUM
};

static enum Calc3DState
{
	ReadyCalc,
	Calculating,
	Done,
	ErrorCalc
};
class Raw3D_Calculation
{
public:
	Raw3D_Calculation();
	~Raw3D_Calculation();

	void Initialize(int nThreadIndex, int nThreadNum, CSize sz3DImage, CSize szRawImage
		, LPBYTE* ppMainImage, WORD** ppBuffHeight, short** ppBuffBright, LPBYTE* ppBuffFG, int nSnapFrameNum);
	void CreateCvtBuffer(CSize szRawImage);

	void SetLogFormHandle(HWND hLogHandle);

	void StartCalculation(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos, int nMinGV1, int nMinGV2
		, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param);

	void CalculateImage();
	void NoConvertImage(int nFrameIndex);
	void ConvertImageBump(int nFrameIndex, bool bReverseScan);

	void CalcHBImage(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_FromTop(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_SKPAD_MinGV1(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_ConsiderZeroBump(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_ConsiderZeroBump_AutoBeamThickness(int nFrameIndex, CPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);

	void ConvertDisplayImage(DisplayMode displayMode, CRect rtROI, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY);

	ConvertMode GetConvertMode();
	Calc3DMode GetCalculationMode();

	inline void SetCalcState(Calc3DState state)
	{
		m_State = state;
	}
	inline Calc3DState GetCalcState()
	{
		return m_State;
	}

	void StopCalc();

private:
	Raw3D_RawData* RAWDATA;

	int m_nThreadIndex;
	int m_nThreadNum;

	CSize m_sz3DImage;
	CSize m_szRawImage;

	LPBYTE* m_ppMainImage;
	LPBYTE m_pBuffRaw;

	WORD** m_ppBuffHeight;
	short** m_ppBuffBright;

	ConvertMode m_cvtMode;
	Calc3DMode m_calcMode;
	DisplayMode m_displayMode;

	CPoint m_ptDataPos;

	int m_nOverlapStartPos;
	int m_nOverlapSize;
	int m_nMinGV1;
	int m_nMinGV2;
	int m_nDisplayOffsetX;
	int m_nDisplayOffsetY;

	bool m_bUseMinGV2;
	bool m_bRevScan;	//역방향 스캔여부

	LPBYTE* m_ppBuffFG;

	int m_nSnapFrameNum;
	int* m_pnCurrFrameNum;

	//void SendLog(CString strMsg);

	Calc3DState m_State;

	bool m_bStop;

	Parameter3D m_param;

	inline bool IsMinGV2(int nX, int nY)
	{
		if (m_ppBuffBright[nY][nX] < 0)
			return true;
		else
			return false;
	}

	int m_SumX[MAX_RAW_Y];
	int m_SumX2[MAX_RAW_Y];
};