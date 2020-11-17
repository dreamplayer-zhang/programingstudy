using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
    public class Defect
    {


        // 각 Inspection에서 올라오는 결과 데이터
        public int nDefectIndex;
        public string sInspectionID;
        public int nDefectCode;
        public float fSize; // Pxl Size
        public float fWidth;
        public float fHeight;
        
        public float fRelX; // 절대좌표 - CeterPoint
        public float fRelY;
        public float fAbsX; // 상대좌표 Origin 좌 하단 <-> Defect 좌 상단
        public float fAbsY;
        
        public int fGV;
        public int nChipIndexX; // Chip Index
        public int nCHipIndexY;
        //public string sImagePath;
        
        protected int nImgsizeX; 
        protected int nImgsizeY;

        protected Rect DefectBox;
        public Rect p_DefectBox { get => DefectBox; set => DefectBox = value; }

        // 모든 Defect 정보들 
        public Defect()
        {
        }
        public Defect(string InspectionID, int defectCode, float defectSz, int fDefectGV, float defectW, float defectH, float fRelX, float fRelY, float fAbsX, float fAbsY, int chipIdxX, int chipIdxY)
        {
            this.sInspectionID = InspectionID;
            nDefectCode = defectCode;

            fSize = defectSz; // Pxl Size
            fGV = fDefectGV;
            this.fRelX = fRelX; // 절대좌표 - CeterPoint
            this.fRelY = fRelY;
            this.fAbsX = fAbsX;
            this.fAbsY = fAbsY;

            fWidth = defectW;
            fHeight = defectH;

            DefectBox = new Rect(fAbsX - (defectW/2), fAbsY - (defectH/2), defectW, defectH);

            nChipIndexX = chipIdxX;
            nCHipIndexY = chipIdxY;
        }
        public Defect(string InspectionID, int defectCode, float defectSz, int fDefectGV, float defectAbsLeft, float defectAbsTop, float defectW, float defectH, int chipIdxX, int chipIdxY)
        {
            this.sInspectionID = InspectionID;
            nDefectCode = defectCode;

            fSize = defectSz; // Pxl Size
            fGV = fDefectGV;

            fAbsX = defectAbsLeft + defectW / 2;
            fAbsY = defectAbsTop + defectH / 2;

            fWidth = defectW;
            fHeight = defectH;

            DefectBox = new Rect(defectAbsLeft, defectAbsTop, defectW, defectH);

            nChipIndexX = chipIdxX;
            nCHipIndexY = chipIdxY;
        }

        public void SetDefectIndex(int nIndex)
        {
            this.nDefectIndex = nIndex;
        }

        public void SetDefectInfo(string insepctionID, int defectCode, float defectSz, int fDefectGV, float defectW, float defectH, float defectRelLeft, float defectRelTop, float defectAbsLeft, float defectAbsTop, int chipIdxX, int chipIdxY)
        {
            this.sInspectionID = insepctionID;
            nDefectCode = defectCode;

            fSize = defectSz; // Pxl Size
            fGV = fDefectGV;

            fWidth = defectW;
            fHeight = defectH;

            fRelX = defectRelLeft + defectW / 2;
            fRelY = defectRelTop + defectH / 2;

            fAbsX = defectAbsLeft + defectW / 2;
            fAbsY = defectAbsTop + defectH / 2;

            DefectBox = new Rect(defectAbsLeft, defectAbsTop, defectW, defectH);

            nChipIndexX = chipIdxX;
            nCHipIndexY = chipIdxY;
        }
        public void CalcAbsToRelPos(int nRefX, int nRefY)
        {
            fRelX = fAbsX - nRefX;
            fRelY = fAbsY - nRefY;
        }
    }
}
