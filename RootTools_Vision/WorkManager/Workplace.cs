using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools.Database;

namespace RootTools_Vision
{
    public enum WORKPLACE_STATE
    {
        //32개 가능?
        NONE                = 0b0000000,
        SNAP                = 0b0000010,
        READY               = 0b0000100,
        INSPECTION          = 0b0001000,
        DEFECTPROCESS       = 0b0010000,
        DEFECTPROCESS_WAFER = 0b0100000,
    }

    public enum WORKPLACE_SUB_STATE
    {
        POSITION_SUCCESS    = 0b00000001,
        LINE_FIRST_CHIP     = 0b00000010
    }

    public delegate void EventPositionUpdated(object obj);

    public delegate void EventPositionIntialized(object obj, int transX, int transY);



    public class Workplace
    {
        public event EventStateChanged StateChanged;

        public event EventPositionUpdated PositionUpdated;

        public event EventPositionIntialized PositionIntialized;

        private WORKPLACE_STATE state;

        public WORKPLACE_STATE STATE
        {
            get { return state; }
            set 
            { 
                state = value;

                if( StateChanged != null)
                    StateChanged(this);
            }
        }

        private int subState;  

        #region [Variables]
        private int index;
        private int mapPositionX;
        private int mapPositionY;
        private int positionX;  // 사용 용도에 따라서 Image Position이나 Axis 좌표가 될 수 있음 ex) WIND, CAMELLIA
        private int positionY;
        private int transX;
        private int transY;
        private int sizeX;
        private int sizeY;

        private IntPtr sharedBuffer;
        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;

        private bool isOccupied = false;
        private List<Defect> defectList = new List<Defect>();

        #endregion

        #region [Getter Setter]
        public int Index { get => index; set => index = value; }// Index는 Workbundle에서 자동설정
        public int MapPositionX { get => mapPositionX; private  set => mapPositionX = value; }
        public int MapPositionY { get => mapPositionY; private  set => mapPositionY = value; }
        public int PositionX { get => positionX; private set => positionX = value; }
        public int PositionY { get => positionY; private  set => positionY = value; }
        public int TransX { get => transX; private  set => transX = value; }
        public int TransY { get => transY; private  set => transY = value; }
        public int SizeX { get => sizeX; private set => sizeX = value; }
        public int SizeY { get => sizeY; private set => sizeY = value; }

        public IntPtr SharedBuffer
        {
            get => sharedBuffer; 
            private set => sharedBuffer = value; 
        }
        public int SharedBufferWidth { get => sharedBufferWidth; private set => sharedBufferWidth = value; }
        public int SharedBufferHeight { get => sharedBufferHeight; private set => sharedBufferHeight = value; }
        public int SharedBufferByteCnt { get => sharedBufferByteCnt; private set => sharedBufferByteCnt = value; }
        public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
        public List<Defect> DefectList { get => defectList; set => defectList = value; }
        #endregion

        public Workplace()
        {

        }

        public void Reset()
        {
            this.state = WORKPLACE_STATE.NONE;
        }

        public Workplace(int mapX, int mapY, int posX, int posY, int szX, int szY, int idx = 0) // Index는 Workbundle에서 자동설정
        {
            this.mapPositionX = mapX;
            this.mapPositionY = mapY;
            this.positionX = posX;
            this.positionY = posY;
            this.sizeX = szX;
            this.sizeY = szY;
            this.index = idx;
        }


        /// <summary>
        /// WORKPLACE_SUB_STATE 에 해당하는 bit값이 true인 false인지 비교한다. 
        /// </summary>
        /// <returns></returns>

        public bool GetSubState(WORKPLACE_SUB_STATE state) => (subState & (int)state) == (int)state;

        public void SetSubState(WORKPLACE_SUB_STATE state, bool bTrue)
        {
            int tempState = GetSubState(state)? subState - (int)state : subState;

            if (bTrue) subState = tempState + (int)state;
            else subState = tempState;
        }


        public void SetSharedBuffer(IntPtr _sharedBuffer, int width, int height, int byteCnt)
        {
            this.sharedBuffer = _sharedBuffer;
            this.sharedBufferWidth = width;
            this.sharedBufferHeight = height;
            this.sharedBufferByteCnt = byteCnt;
        }

        public void SetImagePosition(int posX, int posY)
        {
            this.positionX = posX;
            this.positionY = posY;
        }

        public void SetImagePositionByTrans(int transX, int transY, bool bApplyAll = false)
        {
            this.positionX += transX;
            this.positionY += transY;

            this.transX = transX;
            this.transY = transY;

            if (this.PositionIntialized != null && bApplyAll == true)
                this.PositionIntialized(this, transX, transY);
        }

        public void MoveImagePosition(int transX, int transY, bool bUpdate = false)
        {
            if (transX == 0 && transY == 0) return;

            this.positionX += transX;
            this.positionY += transY;

            this.transX = transX;
            this.transY = transY;

            if (this.PositionUpdated != null && bUpdate == true)
                this.PositionUpdated(this);
        }
        public void AddDefect(string sInspectionID, int defectCode, float defectSz, int defectVal, float defectAbsLeft, float defectAbsTop, float defectW, float defectH, int chipIdxX, int chipIdxY) // SurfaceDefectParam
        {
            Defect defect = new Defect(sInspectionID,
                defectCode, 
                defectSz,
                defectVal,
                defectAbsLeft,
                defectAbsTop, 
                defectW, 
                defectH,
                chipIdxX,
                chipIdxY);

            defectList.Add(defect);             
        }
    }
}
