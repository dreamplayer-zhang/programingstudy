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
        POSITION_SUCCESS            = 0b00000001,
        LINE_FIRST_CHIP             = 0b00000010,
        WAFER_POSITION_SUCCESS      = 0b00000100,
        BAD_CHIP                    = 0b10000000,
    }

    public enum PREWORKDATA_KEY // PreworkdataList의 index로 반드시 0부터 빈틈없이 추가
    {
        D2D_GOLDEN_IMAGE = 0,
        D2D_SCALE_MAP = 1,
    }

    public delegate void EventPositionUpdated(object obj);

    public delegate void EventPositionIntialized(object obj, int transX, int transY);



    public class Workplace
    {
        public event EventPositionUpdated PositionUpdated;

        public event EventPositionIntialized PositionIntialized;

        private WORKPLACE_STATE state;

        public WORKPLACE_STATE STATE
        {
            get { return state; }
            set 
            { 
                state = value;

                WorkEventManager.OnWorkplaceStateChanged(this, new WorkplaceStateChangedEventArgs(this));
            }
        }

          

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
        private byte[] workplaceBuffer;

        private IntPtr sharedBuffer;
        private IntPtr sharedBufferR;
        private IntPtr sharedBufferG;
        private IntPtr sharedBufferB;
        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;

        private bool isOccupied = false;
        private List<Defect> defectList = new List<Defect>();
        //private List<object> preworkDataList = new List<object>();  // Prework에서 생성한 데이터를 넣는 곳으로 예를 들어 D2D의 골든 이미지가 있다.

        private Dictionary<PREWORKDATA_KEY, object> preworkdataDicitonary = new Dictionary<PREWORKDATA_KEY, object>();

        private int subState;
        #endregion

        #region [Getter Setter]
        public int Index { get => index; set => index = value; }// Index는 Workbundle에서 자동설정
        public int MapPositionX { get => mapPositionX; private  set => mapPositionX = value; }
        public int MapPositionY { get => mapPositionY; private  set => mapPositionY = value; }
        public int PositionX { get => positionX; private set => positionX = value; }
        public int PositionY { get => positionY; private  set => positionY = value; }
        public int TransX { get => transX; private  set => transX = value; }
        public int TransY { get => transY; private  set => transY = value; }
        public int BufferSizeX { get => sizeX; private set => sizeX = value; }
        public int BufferSizeY { get => sizeY; private set => sizeY = value; }
        
        public byte[] WorkplaceBuffer 
        { 
            get => workplaceBuffer; 
            private set => workplaceBuffer = value; 
        }
        public IntPtr SharedBuffer
        {
            get => sharedBuffer; 
            private set => sharedBuffer = value; 
        }
        public IntPtr SharedBufferR
        {
            get => sharedBufferR;
            private set => sharedBufferR = value;
        }
        public IntPtr SharedBufferG
        {
            get => sharedBufferG;
            private set => sharedBufferG = value;
        }
        public IntPtr SharedBufferB
        {
            get => sharedBufferB;
            private set => sharedBufferB = value;
        }
        public int SharedBufferWidth { get => sharedBufferWidth; private set => sharedBufferWidth = value; }
        public int SharedBufferHeight { get => sharedBufferHeight; private set => sharedBufferHeight = value; }
        public int SharedBufferByteCnt { get => sharedBufferByteCnt; private set => sharedBufferByteCnt = value; }
        public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
        public List<Defect> DefectList { get => defectList; set => defectList = value; }


        // 이 파라매터는 직접 호출하는 것을 지양하고, SetPreworkData()/GetPreworkData()함수를 통해서 호출
        private Dictionary<PREWORKDATA_KEY, object> PreworkDataDictionary { get => preworkdataDicitonary; set => preworkdataDicitonary = value; }  
        #endregion

        public Workplace()
        {

        }

        public void Reset()
        {
            this.STATE = WORKPLACE_STATE.NONE;

            this.PreworkDataDictionary.Clear();

            foreach(PREWORKDATA_KEY key in Enum.GetValues(typeof(PREWORKDATA_KEY)))
            {
                this.preworkdataDicitonary.Add(key, null);
            }            
        }

        public void SetPreworkData(PREWORKDATA_KEY key, object dataObj)
        {
            this.preworkdataDicitonary[key] = dataObj;
            //this.preworkDataList.Add(dataObj);
        }

        public object GetPreworkData(PREWORKDATA_KEY key)
        {
            if (preworkdataDicitonary.Count == 0) return null;

            return this.preworkdataDicitonary[key];   
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
            this.workplaceBuffer = new byte[szX * szY];
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
        public void SetSharedRGBBuffer(IntPtr _sharedBufferR, IntPtr _sharedBufferG, IntPtr _sharedBufferB)
        {
            this.sharedBufferR = _sharedBufferR;
            this.sharedBufferG = _sharedBufferG;
            this.sharedBufferB = _sharedBufferB;
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
        public void AddDefect(string sInspectionID, int defectCode, float defectSz, float defectVal, float defectAbsLeft, float defectAbsTop, float defectW, float defectH, int chipIdxX, int chipIdxY) // SurfaceDefectParam
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
