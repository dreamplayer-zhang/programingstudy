using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
	public class EBRMeasurement : Measurement
	{
		public enum MeasureItem
		{
			Bevel,
			EBR,
		}

		public EBRMeasurement()
		{

		}

        
        public EBRMeasurement(string strInspectionID, MeasureItem measureName, float fData, float fDefectW, float fDefectH, float fDefectAbsLeft, float fDefectAbsTop, int nChipIdxX, int nChipIdxY)
        {
            m_strInspectionID = strInspectionID;
            m_strSide = "EDGE";
            m_strMeasureType = MeasureType.EBR.ToString();
            m_strMeasureItem = measureName.ToString();

            m_fData = fData;
            m_fWidth = fDefectW;
            m_fHeight = fDefectH;

            m_fAbsX = fDefectAbsLeft + fDefectW / 2;
            m_fAbsY = fDefectAbsTop + fDefectH / 2;

            m_nChipIndexX = nChipIdxX;
            m_nCHipIndexY = nChipIdxY;

            m_rtDefectBox = new Rect(fDefectAbsLeft, fDefectAbsTop, fDefectW, fDefectH);
        }
        

        public void SetData(string strInspectionID, EBRMeasureItem measureName, float fData, float fDefectW, float fDefectH, float fDefectAbsLeft, float fDefectAbsTop, int nChipIdxX, int nChipIdxY)
		{
            m_strInspectionID = strInspectionID;
            m_strSide = "EDGE";
            m_strMeasureType = MeasureType.EBR.ToString();
            m_strMeasureItem = measureName.ToString();

            m_fData = fData;
            m_fWidth = fDefectW;
            m_fHeight = fDefectH;

            m_fAbsX = fDefectAbsLeft + fDefectW / 2;
            m_fAbsY = fDefectAbsTop + fDefectH / 2;

            m_nChipIndexX = nChipIdxX;
            m_nCHipIndexY = nChipIdxY;
        }
    }
}
