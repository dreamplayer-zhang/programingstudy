#pragma once
#include "Raw3D_Calculation.h"
#include "Raw3D_RawData.h"

class Raw3DManager
{
public:
	Raw3DManager();
	~Raw3DManager();

	void Initialize(LPBYTE ppMainImage, int n3DImageWidth, int n3DImageHeight, CSize szRawImage, int nMaxOverlapSize);

	void MakeImage(ConvertMode convertMode, Calc3DMode calcMode, DisplayMode displayMode, CPoint ptDataPos
		, int nMinGV1, int nMinGV2, int nThreadNum, int nSnapFrameNum, int nOverlapStartPos, int nOverlapSize
		, int nDisplayOffsetX, int nDisplayOffsetY, bool bRevScan, bool bUseMinGV2, Parameter3D param);
	void SetFrameNum(int fn);
	CSize GetImageBufferSize();
	CSize GetRawImageSize();
	WORD** GetHeightBuffer();
	short** GetBrightBuffer();

	LPBYTE* GetRawBuffFG();
	Raw3D_RawData* GetRawData();
	void CheckSnapCalcDone();

	void StopCalculation();

private:
	LPBYTE* CreateRawBuffer(int nSnapFrameNum);
	void DeleteRawBuffer();

	Raw3D_RawData* RAWDATA;

	Raw3D_Calculation* m_pThreadCalc;

	LPBYTE* m_ppImageMain;



	WORD** m_ppBuffHeight;
	short** m_ppBuffBright;

	CSize m_szImageBuffer;
	CSize m_szRawImage;

	int m_nMaxOverlapSize;

	//volatile LPBYTE* m_ppBuffFG;	//이거 한번 써보고 안되면 지워야함. 아니야 얘 안써
	LPBYTE* m_ppBuffFG;

	int m_nCurrFrameNum;


	int m_nSnapFrameNum;
	int m_nThreadNum;
};


