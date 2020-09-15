using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Database
{
    public class Defect
    {

        protected string sInspectionID;

        protected int nDefectCode;
        protected float fSize; // Pxl Size
        protected float fWidth;
        protected float fHeight;

        protected int nChipIndexX; // Chip Index
        protected int nCHipIndexY;

        protected float fAbsX; // 절대좌표
        protected float fAbsY;
        protected float fRelX;
        protected float fRelY;

        protected int nImgsizeX; 
        protected int nImgsizeY;


        public Defect()
        {

        }
        public Defect(string _sInspectionID, int _nDcode, float _fSize)
        {
            sInspectionID = _sInspectionID;
            nDefectCode = _nDcode;
            fSize = _fSize;
        }

        public void SetDefect(string _sInspectionID, int _nDcode, float _fSize)
        {
            sInspectionID = _sInspectionID;
            nDefectCode = _nDcode;
            fSize = _fSize;
        }
        
        public int GetDcode() { return nDefectCode; }
        public float GetDsize() { return fSize; }
    }
}
