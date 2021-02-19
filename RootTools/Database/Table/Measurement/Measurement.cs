using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
    public class Measurement
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

        public Measurement()
        { 

        }
    }
}
