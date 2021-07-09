using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.Database
{
    [Serializable]
    public class DefectEIP:Data
    {
        public int nDefectIdx;
        public string strInspID;
        public int nDefectCode;
        public double dSize;
        public double dWidth;
        public double dHeight;

        public double dRelX, dRelY;
        public double dAbsX, dAbsY;

        public int nGV;

        protected int nImgSizeX;
        protected int nImgSizeY;

        protected Rect rtDefectBox;

        public string memPOOL;
        public string memGROUP;
        public string memMEMORY;

        public Rect p_rtDefectBox { get => rtDefectBox; set => rtDefectBox = value; }

        public DefectEIP() { }

        public DefectEIP(string strInspID, int nDefectCode, float dSize, int nGV, float dWidth, float dHeight, float dRelX, float dRelY, 
            float dAbsX, float dAbsY)
        {
            this.strInspID = strInspID;
            this.nDefectCode = nDefectCode;

            this.dSize = dSize; // Pxl Size
            this.nGV = nGV;
            this.dRelX = dRelX; // 절대좌표 - CeterPoint
            this.dRelY = dRelY;
            this.dAbsX = dAbsX;
            this.dAbsY = dAbsY;

            this.dWidth = dWidth;
            this.dHeight = dHeight;

            rtDefectBox = new Rect(dAbsX - (dWidth / 2), dAbsY - (dHeight / 2), dWidth, dHeight);
        }
        public void SetDefectIndex(int nIndex)
        {
            nDefectIdx = nIndex;
        }

        public void SetDefectInfo(string strInspID, int nDefectCode, double dSize, int nGV,
            double dWidth, double dHeight, double fDefectRelLeft, double fDefectRelTop, double fDefectAbsLeft, double fDefectAbsTop)
        {
            this.strInspID = strInspID;
            this.nDefectCode = nDefectCode;

            this.dSize = dSize; // Pxl Size
            this.nGV = nGV;

            this.dWidth = dWidth;
            this.dHeight = dHeight;

            dRelX = fDefectRelLeft + dWidth / 2;
            dRelY = fDefectRelTop + dHeight / 2;

            dAbsX = fDefectAbsLeft + dWidth / 2;
            dAbsY = fDefectAbsTop + dHeight / 2;

            rtDefectBox = new Rect(fDefectAbsLeft, fDefectAbsTop, dWidth, dHeight);
        }

        public override Rect GetRect()
        {
            return p_rtDefectBox;
        }

        public override float GetWidth()
        {
            return (float)dWidth;
        }

        public override float GetHeight()
        {
            return (float)dHeight;
        }
        public void CalcAbsToRelPos(int nRefX, int nRefY)
        {
            dRelX = dAbsX- nRefX;
            dRelY = -(dAbsY- nRefY); // 칩의 좌하단 기준
        }
    }
}
