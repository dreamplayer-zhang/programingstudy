using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
    public class Measurement : Data
    {
        public int m_nMeasurementIndex;
        public string m_strInspectionID;

        public string m_strSide;
        public string m_strMeasureType;
        public string m_strMeasureItem;

        public float m_fData;
        public float m_fWidth;
        public float m_fHeight;
        public float m_fAngle;

        public float m_fRelX; // 절대좌표 - CeterPoint
        public float m_fRelY;
        public float m_fAbsX; // 상대좌표 Origin 좌 하단 <-> Defect 좌 상단
        public float m_fAbsY;

        public int m_nChipIndexX; // Chip Index
        public int m_nCHipIndexY;

        protected Rect m_rtDefectBox;
        public Rect p_rtDefectBox { get => m_rtDefectBox; set => m_rtDefectBox = value; }

        public enum MeasureType
		{
            EBR,
		}

        public enum EBRMeasureItem
        {
            Bevel,
            EBR,
        }

        public Measurement()
        { 

        }

        public Measurement(string strInspectionID, string strSide, string strType, string strMeasureName, float fData, float fDefectW, float fDefectH, float fAngle, float fDefectAbsLeft, float fDefectAbsTop, int nChipIdxX, int nChipIdxY)
        {
            m_strInspectionID = strInspectionID;
            m_strSide = strSide;
            m_strMeasureType = strType;
            m_strMeasureItem = strMeasureName;

            m_fData = fData;
            m_fWidth = fDefectW;
            m_fHeight = fDefectH;
            m_fAngle = fAngle;

            m_fAbsX = fDefectAbsLeft + fDefectW / 2;
            m_fAbsY = fDefectAbsTop + fDefectH / 2;

            m_nChipIndexX = nChipIdxX;
            m_nCHipIndexY = nChipIdxY;

			m_rtDefectBox = new Rect(fDefectAbsLeft, fDefectAbsTop, fDefectW, fDefectH);
        }

        public void SetMeasureIndex(int nIndex)
        {
            m_nMeasurementIndex = nIndex;
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
    }
}
