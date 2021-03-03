using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
    public class Defect : Data
    {
        // 각 Inspection에서 올라오는 결과 데이터
        public int m_nDefectIndex;
        public string m_strInspectionID;
        public int m_nDefectCode;
        public float m_fSize; // Pxl Size
        public float m_fWidth;
        public float m_fHeight;
        
        public float m_fRelX; // 절대좌표 - CeterPoint
        public float m_fRelY;
        public float m_fAbsX; // 상대좌표 Origin 좌 하단 <-> Defect 좌 상단
        public float m_fAbsY;
        
        public float m_fGV;
        public int m_nChipIndexX; // Chip Index
        public int m_nCHipIndexY;
        //public string sImagePath;
                
        protected int m_nImgsizeX; 
        protected int m_nImgsizeY;

        protected Rect m_rtDefectBox;
        public Rect p_rtDefectBox { get => m_rtDefectBox; set => m_rtDefectBox = value; }

        // 모든 Defect 정보들 
        public Defect()
        {
        }
        public Defect(string strInspectionID, int nDefectCode, float fDefectSz, float fDefectGV, float fDefectW, float fDefectH, float fRelX, float fRelY, float fAbsX, float fAbsY, int chipIdxX, int chipIdxY)
        {
            m_strInspectionID = strInspectionID;
            m_nDefectCode = nDefectCode;

            m_fSize = fDefectSz; // Pxl Size
            m_fGV = fDefectGV;
            m_fRelX = fRelX; // 절대좌표 - CeterPoint
            m_fRelY = fRelY;
            m_fAbsX = fAbsX;
            m_fAbsY = fAbsY;

            m_fWidth = fDefectW;
            m_fHeight = fDefectH;

            m_rtDefectBox = new Rect(fAbsX - (fDefectW / 2), fAbsY - (fDefectH / 2), fDefectW, fDefectH);

            m_nChipIndexX = chipIdxX;
            m_nCHipIndexY = chipIdxY;
        }
        public Defect(string strInspectionID, int nDefectCode, float fDefectSz, float fDefectGV, float fDefectAbsLeft, float fDefectAbsTop, float fDefectW, float fDefectH, int nChipIdxX, int nChipIdxY)
        {
            m_strInspectionID = strInspectionID;
            m_nDefectCode = nDefectCode;

            m_fSize = fDefectSz; // Pxl Size
            m_fGV = fDefectGV;

            m_fAbsX = fDefectAbsLeft + fDefectW / 2;
            m_fAbsY = fDefectAbsTop + fDefectH / 2;

            m_fWidth = fDefectW;
            m_fHeight = fDefectH;

            m_rtDefectBox = new Rect(fDefectAbsLeft, fDefectAbsTop, fDefectW, fDefectH);

            m_nChipIndexX = nChipIdxX;
            m_nCHipIndexY = nChipIdxY;
        }

        public void SetDefectIndex(int nIndex)
        {
            m_nDefectIndex = nIndex;
        }

        public void SetDefectInfo(string strInsepctionID, int nDefectCode, float fDefectSz, float fDefectGV, float fDefectW, float fDefectH, float fDefectRelLeft, float fDefectRelTop, float fDefectAbsLeft, float fDefectAbsTop, int nChipIdxX, int nChipIdxY)
        {
            m_strInspectionID = strInsepctionID;
            m_nDefectCode = nDefectCode;

            m_fSize = fDefectSz; // Pxl Size
            m_fGV = fDefectGV;

            m_fWidth = fDefectW;
            m_fHeight = fDefectH;

            m_fRelX = fDefectRelLeft + fDefectW / 2;
            m_fRelY = fDefectRelTop + fDefectH / 2;

            m_fAbsX = fDefectAbsLeft + fDefectW / 2;
            m_fAbsY = fDefectAbsTop + fDefectH / 2;

            m_rtDefectBox = new Rect(fDefectAbsLeft, fDefectAbsTop, fDefectW, fDefectH);

            m_nChipIndexX = nChipIdxX;
            m_nCHipIndexY = nChipIdxY;
        }

        public void CalcAbsToRelPos(int nRefX, int nRefY)
        {
            m_fRelX = m_fAbsX - nRefX;
            m_fRelY = m_fAbsY - nRefY;
        }

        public override Rect GetRect()
        {
            return p_rtDefectBox;
        }

        public override float GetWidth()
        {
            return m_fWidth;
        }

        public override float GetHeight()
        {
            return m_fHeight;
        }

        public void CalcDegree()
		{

		}
    }
}
