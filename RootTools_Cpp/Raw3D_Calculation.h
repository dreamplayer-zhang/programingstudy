#pragma once
#include "pch.h"
#include "Raw3D_RawData.h"
#define MAX_OVERLAP 200
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

	void Initialize(int nThreadIndex, int nThreadNum, CCSize sz3DImage, CCSize szRawImage
		, LPBYTE* ppMainImage, WORD** ppBuffHeight, short** ppBuffBright, LPBYTE* ppBuffFG, int nSnapFrameNum);
	void CreateCvtBuffer(CCSize szRawImage);

	void SetLogFormHandle(HWND hLogHandle);
	void SetFrameNum(int n);
	void StartCalculation(ConvertMode cvtMode, Calc3DMode calcMode, DisplayMode displayMode, CCPoint ptDataPos, int nMinGV1, int nMinGV2
		, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, int* pnCurrFrameNum, Parameter3D param);

	void CalculateImage();
	void NoConvertImage(int nFrameIndex);
	void ConvertImageBump(int nFrameIndex, bool bReverseScan);

	void CalcHBImage(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_FromTop(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_SKPAD_MinGV1(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_ConsiderZeroBump(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);
	void CalcHBImage_ConsiderZeroBump_AutoBeamThickness(int nFrameIndex, CCPoint ptDataPos, int nMinGV1, int nMinGV2, bool bUseDualMinGV);

	void ConvertDisplayImage(DisplayMode displayMode, CCRect rtROI, int nOverlapStartPos, int nOverlapSize, int nDisplayOffsetX, int nDisplayOffsetY);

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

	CCSize m_sz3DImage;
	CCSize m_szRawImage;

	LPBYTE* m_ppMainImage;
	LPBYTE m_pBuffRaw;

	WORD** m_ppBuffHeight;
	short** m_ppBuffBright;

	ConvertMode m_cvtMode;
	Calc3DMode m_calcMode;
	DisplayMode m_displayMode;

	CCPoint m_ptDataPos;

	int m_nOverlapStartPos;
	int m_nOverlapSize;
	int m_nMinGV1;
	int m_nMinGV2;
	int m_nDisplayOffsetX;
	int m_nDisplayOffsetY;

	bool m_bUseMinGV2;
	bool m_bRevScan;	//?????? ????????

	LPBYTE* m_ppBuffFG;

	int m_nSnapFrameNum;
	int m_nCurrFrameNum;

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
	double m_pdOverlap[MAX_OVERLAP];
	int m_SumX[MAX_RAW_Y];
	int m_SumX2[MAX_RAW_Y];
};