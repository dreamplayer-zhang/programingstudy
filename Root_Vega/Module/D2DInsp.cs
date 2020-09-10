using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RootTools_CLR;
using System.Threading;
using RootTools;
using RootTools.Memory;

namespace Root_Vega.Module
{

    public class D2DInfo
    {
        public D2DInfo()
        {
            m_AlignInfo = new AlignInfo();
            m_MaskInfo = new MaskInfo();
            m_DieInfo = new DieInfo();
            m_PrebuiltData = new PrebuiltData();
        }

        public AlignInfo m_AlignInfo;
        public MaskInfo m_MaskInfo;
        public DieInfo m_DieInfo;
        public PrebuiltData m_PrebuiltData;

        public void PreBuild()
        {
            //임시.  m_PrebuiltData.nDieWidth 는 256의 배수여야 SSE의 계산이 정상적임을 보장받음.
            m_PrebuiltData.nDieWidth = m_DieInfo.p_FirstDieRight - m_DieInfo.p_FirstDieLeft;
            m_PrebuiltData.nDieHorizontalRectCount = m_PrebuiltData.nDieWidth / 256;

            //임시.  m_PrebuiltData.nDieHeight 는 256의 배수여야 SSE의 계산이 정상적임을 보장받음.
            m_PrebuiltData.nDieHeight = m_DieInfo.p_FirstDieUp - m_DieInfo.p_FirstDieBottom;
            m_PrebuiltData.nDieVerticalRectCount = m_PrebuiltData.nDieHeight / 256;

            m_PrebuiltData.nScribeLaneWidth = m_DieInfo.p_SecondDieLeft - m_DieInfo.p_FirstDieRight;
            m_PrebuiltData.nScribeLaneHeight = m_DieInfo.p_SecondDieBottom - m_DieInfo.p_FirstDieUp;

            m_PrebuiltData.ShotWidht = m_DieInfo.p_LastDieRight - m_DieInfo.p_FirstDieLeft;
            m_PrebuiltData.ShotHeight = m_DieInfo.p_LastDieUp - m_DieInfo.p_FirstDieBottom;

            // set rowchipcount and columnchipcount
            int nRowChipCount = 0;
            int nRowLength = m_DieInfo.p_FirstDieRight;
            int nRowMaxLength = Math.Min(m_DieInfo.p_LastDieRight + m_PrebuiltData.nScribeLaneWidth / 2, m_AlignInfo.p_RightBottom.X);
            while (nRowLength < nRowMaxLength)
            {
                nRowChipCount++;
                nRowLength = nRowLength + m_PrebuiltData.nDieWidth + m_PrebuiltData.nScribeLaneWidth;
            }
            m_PrebuiltData.nRowChipCount = nRowChipCount;

            int nColumnChipCount = 0;
            int nColumnLength = m_DieInfo.p_FirstDieUp;
            int nColumnMaxLength = Math.Min(m_DieInfo.p_LastDieUp + m_PrebuiltData.nScribeLaneHeight / 2, m_AlignInfo.p_LeftTop.Y);
            while (nColumnLength < nColumnMaxLength)
            {
                nColumnChipCount++;
                nColumnLength = nColumnLength + m_PrebuiltData.nDieHeight + m_PrebuiltData.nScribeLaneHeight;
            }
            m_PrebuiltData.nColumnChipCount = nColumnChipCount;

            //set pos Data of each Chip, in PrebuiltData
            m_PrebuiltData.MakeChipPos(m_DieInfo.p_FirstDieLeft, m_DieInfo.p_FirstDieBottom);

            m_AlignInfo.bNeedReckon = false;
            m_DieInfo.bNeedReckon = false;
            m_PrebuiltData.bPrebuilt = true;
        }
    }
    public class MaskInfo
    {
        String MaskSerialNumber;

    }
    public class AlignInfo
    {
        public int p_Left
        {
            get { return p_LeftBottom.X; }
            set
            {
                bNeedReckon = true;
                m_LeftBottom.X = value;
                m_LeftTop.X = value;
            }
        }
        public int p_Right
        {
            get { return p_RightBottom.X; }
            set
            {
                bNeedReckon = true;
                m_RightBottom.X = value;
                m_RightTop.X = value;
            }
        }
        public int p_Top
        {
            get { return p_LeftTop.Y; }
            set
            {
                bNeedReckon = true;
                p_LeftTop.Y = value;
                p_RightBottom.Y = value;
            }
        }
        public int p_Bottom
        {
            get { return p_RightBottom.Y; }
            set
            {
                bNeedReckon = true;
                p_RightBottom.Y = value;
                m_RightTop.Y = value;
            }
        }

        public bool bNeedReckon = true;
        private CPoint m_LeftBottom=new CPoint();
        public CPoint p_LeftBottom
        {
            get { return m_LeftBottom; }
            set
            {
                bNeedReckon = true;
                m_LeftBottom = value;
            }
        }

        private CPoint m_LeftTop = new CPoint();
        public CPoint p_LeftTop
        {
            get { return m_LeftTop; }
            set
            {
                bNeedReckon = true;
                m_LeftTop = value;
            }
        }

        private CPoint m_RightBottom = new CPoint();
        public CPoint p_RightBottom
        {
            get { return m_RightBottom; }
            set
            {
                bNeedReckon = true;
                m_RightBottom = value;
            }
        }

        private CPoint m_RightTop = new CPoint();
        public CPoint p_RightTop
        {
            get { return m_RightTop; }
            set
            {
                bNeedReckon = true;
                m_RightTop = value;
            }
        }


        //prebuiltData
    }
    public class DieInfo
    {
        public bool bNeedReckon = true;
        private int m_FirstDieLeft;
        public int p_FirstDieLeft
        {
            get { return m_FirstDieLeft; }
            set
            {
                bNeedReckon = true;
                m_FirstDieLeft = value;
            }
        }

        private int m_FirstDieRight;
        public int p_FirstDieRight
        {
            get { return m_FirstDieRight; }
            set
            {
                bNeedReckon = true;
                m_FirstDieRight = value;
            }
        }

        private int m_SecondDieLeft;
        public int p_SecondDieLeft
        {
            get { return m_SecondDieLeft; }
            set
            {
                bNeedReckon = true;
                m_SecondDieLeft = value;
            }
        }

        private int m_LastDieRight;
        public int p_LastDieRight
        {
            get { return m_LastDieRight; }
            set
            {
                bNeedReckon = true;
                m_LastDieRight = value;
            }
        }

        private int m_FirstDieBottom;
        public int p_FirstDieBottom
        {
            get { return m_FirstDieBottom; }
            set
            {
                bNeedReckon = true;
                m_FirstDieBottom = value;
            }
        }

        private int m_FirstDieUp;
        public int p_FirstDieUp
        {
            get { return m_FirstDieUp; }
            set
            {
                bNeedReckon = true;
                m_FirstDieUp = value;
            }
        }
        private int m_SecondDieBottom;
        public int p_SecondDieBottom
        {
            get { return m_SecondDieBottom; }
            set
            {
                bNeedReckon = true;
                m_SecondDieBottom = value;
            }
        }
        private int m_LastDieUp;
        public int p_LastDieUp
        {
            get { return m_LastDieUp; }
            set
            {
                bNeedReckon = true;
                m_LastDieUp = value;
            }
        }


        //prebuiltData
    }
    public class PrebuiltData
    {
        public bool bPrebuilt = false;
        public int nDieWidth;
        public int nDieHeight;
        public int nDieVerticalRectCount;
        public int nDieHorizontalRectCount;

        public int nScribeLaneWidth;
        public int nScribeLaneHeight;
        public int ShotWidht;
        public int ShotHeight;


        public int nRowChipCount;
        public int nColumnChipCount;
        public int[][] nChipPos_X;
        public int[][] nChipPos_Y;

        public void MakeChipPos(int nFirstDieLeft, int nFirstDieBottom)
        {
            nChipPos_Y = new int[nRowChipCount][];
            nChipPos_X = new int[nRowChipCount][];
            for (int i = 0; i < nRowChipCount; i++)
            {
                nChipPos_Y[i] = new int[nColumnChipCount];
                nChipPos_X[i] = new int[nColumnChipCount];
            }

            int nRowPos = nFirstDieLeft;
            for (int i = 0; i < nRowChipCount; i++)
            {
                for (int j = 0; j < nColumnChipCount; j++)
                {
                    nChipPos_X[i][j] = nRowPos;
                }
                nRowPos = nRowPos + nDieWidth + nScribeLaneWidth;
            }

            int nColumnPos = nFirstDieBottom;
            for (int j = 0; j < nColumnChipCount; j++)
            {
                for (int i = 0; i < nRowChipCount; i++)
                {
                    nChipPos_Y[i][j] = nColumnPos;
                }
                nColumnPos = nColumnPos + nDieHeight + nScribeLaneHeight;
            }
        }

    }

    public class D2DInspect
    {
        const int nSSEWidth = 256;

        //for c#
        public D2DInfo m_D2DInfo = new D2DInfo();
        ImageData m_ImageData;
        MemoryData sD2Dmemdata;
        MemoryData sD2DABSmemdaa;

        //ImageData[][] DoubleImage;
        //for clr
        CCLRD2DModule d2dModule = new CCLRD2DModule();

        public unsafe void StartInsp(ImageData _m_ImageData, MemoryData _sD2Dmemdata, MemoryData _sD2DABSmemdata)
        {

            m_D2DInfo.PreBuild();
            sD2Dmemdata = _sD2Dmemdata;
            sD2DABSmemdaa = _sD2DABSmemdata;

            m_ImageData = _m_ImageData;
            
            int nThreadCount = Math.Min(sD2Dmemdata.p_nCount, sD2DABSmemdaa.p_nCount);
            Thread[] threadList = new Thread[nThreadCount];
            //Thread[][] threadList = new Thread[m_D2DInfo.m_PrebuiltData.nRowChipCount][];
            //for (int i = 0; i < m_D2DInfo.m_PrebuiltData.nColumnChipCount; i++)
            //{
            //    threadList[i] = new Thread[m_D2DInfo.m_PrebuiltData.nDieVerticalRectCount];
            //}

            d2dModule.SetWidth(m_ImageData.p_Size.X, 2 * m_D2DInfo.m_PrebuiltData.nDieWidth, m_D2DInfo.m_PrebuiltData.nDieWidth);
            int nRefY = 0;
            int counter = 0;
            for (int i = 0; i < m_D2DInfo.m_PrebuiltData.nRowChipCount; i++)
            {
                for (int j = 0; j < m_D2DInfo.m_PrebuiltData.nColumnChipCount; i++)
                {
                    counter++;
                    counter %= nThreadCount;
                    if (j == 0)
                        nRefY = 1;
                    else
                        nRefY = j - 1;

                    threadList[counter] = new Thread(new ParameterizedThreadStart(ChipInsp));
                    d2dModule.SetDieInfo(i, j, i, nRefY);
                    d2dModule.SetPtrInfo((byte*)m_ImageData.GetPtr(m_D2DInfo.m_PrebuiltData.nChipPos_X[i][j], m_D2DInfo.m_PrebuiltData.nChipPos_Y[i][j]), (byte*)m_ImageData.GetPtr(m_D2DInfo.m_PrebuiltData.nChipPos_X[i][nRefY], m_D2DInfo.m_PrebuiltData.nChipPos_Y[i][nRefY]), (byte*)sD2Dmemdata.GetPtr(counter), (byte*)sD2DABSmemdaa.GetPtr(counter));
                    //CRect pChip = new CRect(m_D2DInfo.m_PrebuiltData.nChipPos_X[i][j], m_D2DInfo.m_PrebuiltData.nChipPos_Y[i][nRefY], m_D2DInfo.m_PrebuiltData.nDieWidth, m_D2DInfo.m_PrebuiltData.nDieHeight);
                    //m_ImageData.GetRectImage(pChip);
                    //Emgu.CV.Mat matSrc = new Emgu.CV.Mat(img.p_Size.X, img.p_Size.Y, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);

                    //target, abs 이미지 ptr 넣어주기
                    //d2dModule.SetPtrInfo();
                    threadList[counter].Start(d2dModule);
                }
            }


            //d2d_test.testDB();
            //test1.pByte = ;

        }

        public unsafe void MakeDoubleSize(CCLRD2DModule d2dModule)
        {
            //추후 sse or emgu 사용

            int padding = 10;

            int TotalWidth = m_ImageData.p_Size.X;
            byte* pChipTarget = d2dModule.GetChipTargetPtr()-padding*TotalWidth-padding;
            byte* pDoubleSizeTarget = d2dModule.GetDoubleSizeTargetPtr();
            int ChipWidth = m_D2DInfo.m_PrebuiltData.nDieWidth+2*padding;
            int ChipHeight = m_D2DInfo.m_PrebuiltData.nDieHeight+2*padding;
            byte* pLineChipTarget;
            
            for (int j = 0; j < ChipHeight; j++)
            {
                pLineChipTarget = pChipTarget + (2 * j) * TotalWidth;
                byte* pLineDoubleSizeTarget = d2dModule.GetDoubleSizeTargetPtr();

                for (int i = 0; i < ChipWidth; i++)
                {
                    *pDoubleSizeTarget = *pLineChipTarget;
                    pDoubleSizeTarget++;
                    *pDoubleSizeTarget = (byte)((*pLineChipTarget + *(pLineChipTarget + 1)) / 2);
                    pDoubleSizeTarget++;
                    pLineChipTarget++;

                }
                pLineChipTarget = pChipTarget + (2 * j) * TotalWidth;
                for (int i = 0; i < ChipWidth; i++)
                {
                    *pDoubleSizeTarget = (byte)(((int)(*pLineChipTarget) + *(pLineChipTarget + TotalWidth)) / 2);
                    pDoubleSizeTarget++;
                    *pDoubleSizeTarget = (byte)(((int)(*pLineChipTarget) + *(pLineChipTarget + TotalWidth) + *(pLineChipTarget + 1) + *(pLineChipTarget + 1 + TotalWidth)) / 4);
                    pDoubleSizeTarget++;
                    pLineChipTarget++;

                }
            }
        }

        public void ChipInsp(object _d2dModule)
        {
            CCLRD2DModule d2dModule = (CCLRD2DModule)_d2dModule;
            //m_ImageData.GetRectImage();
            //CRect pChip = new CRect();
            //2배이미지 만들기.
            MakeDoubleSize(d2dModule);

            //trigger
            //Make ABS Image
            //둘다 SSE에서 실행.
            int TotalWidth = m_ImageData.p_Size.X;
            int ChipWidth = m_D2DInfo.m_PrebuiltData.nDieWidth;

            int nNumX, nNumY;
            for (int j = 0; j < m_D2DInfo.m_PrebuiltData.nDieVerticalRectCount; j++)
                for (int i = 0; i < m_D2DInfo.m_PrebuiltData.nDieHorizontalRectCount; i++)
                {
                    nNumX = d2dModule.GetTargetDieNumX();
                    nNumY = d2dModule.GetTargetDieNumY();
                    d2dModule.SetRectInfo(i,j,m_D2DInfo.m_PrebuiltData.nChipPos_X[nNumX][nNumY]+i* nSSEWidth, m_D2DInfo.m_PrebuiltData.nChipPos_Y[nNumX][nNumY]+j* nSSEWidth);
                    d2dModule.AddPtrInfo(2*nSSEWidth * i + 2*nSSEWidth * j * ChipWidth, nSSEWidth * i + nSSEWidth * j * TotalWidth, nSSEWidth * i + nSSEWidth * j * ChipWidth);
                    d2dModule.D2DInspChipRectProto();
                }
            d2dModule.D2DInspChipRectProto();

            //Emgu.CV.Mat matSrc = new Emgu.CV.Mat(img.p_Size.X, img.p_Size.Y, Emgu.CV.CvEnum.DepthType.Cv8U, img.p_nByte, img.GetPtr(), (int)img.p_Stride);

            //d2dModule.D2DInspProto;

        }
    }




}
