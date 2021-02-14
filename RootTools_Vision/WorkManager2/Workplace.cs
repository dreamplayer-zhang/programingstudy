using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public enum WORKPLACE_SUB_STATE
    {
        WAFER_POSITION_SUCCESS = 0b00000001,
        POSITION_SUCCESS = 0b00000010,
        LINE_FIRST_CHIP = 0b00000100,
        BAD_CHIP = 0b10000000,
    }

    public enum PREWORKDATA_KEY // PreworkdataList의 index로 반드시 0부터 빈틈없이 추가
    {
        D2D_GOLDEN_IMAGE = 0,
        D2D_SCALE_MAP = 1,
    }

    /// <summary>
    /// - 검사할 영역의 정보를 포함합니다.
    /// - Image Buffer를 직접할당하지 않습니다.
    /// - offset과 trans 이외의 값은 처음 생성할 떄 값을 할당받고 변경되지 않습니다.
    /// </summary>
    public class Workplace : ObservableObject
    {
        #region [Members]
        private readonly int index;
        private readonly int mapIndexX;
        private readonly int mapIndexY;
        private readonly int positionX;
        private readonly int positionY;
        private int offsetX;
        private int offsetY;
        private int transX;
        private int transY;
        private readonly int width;
        private readonly int height;

        private IntPtr sharedBufferR_GRAY;
        private IntPtr sharedBufferG;
        private IntPtr sharedBufferB;

        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;

        private WORK_TYPE workState;

        private bool isOccupied = false;

        private Dictionary<PREWORKDATA_KEY, object> preworkdataDicitonary = new Dictionary<PREWORKDATA_KEY, object>();

        private int subState;

        private List<Defect> defectList = new List<Defect>();
        private List<Measurement> measure = new List<Measurement>();

        #endregion

        #region [Getter Setter]
        /// <summary>
        /// WorkplaceBundle 안에서 순서
        /// </summary>
        public int Index
        {
            get => this.index;
        }

        /// <summary>
        /// Map에서의 위치 좌표 X
        /// </summary>
        public int MapIndexX
        {
            get => this.mapIndexX;
        }

        /// <summary>
        /// Map에서의 위치 좌표 Y
        /// </summary>
        public int MapIndexY
        {
            get => this.mapIndexY;
        }

        /// <summary>
        /// Buffer 상에서 최초에 생성된(티칭된) 좌표 Y
        /// </summary>

        public int PositionOriginX
        {
            get => this.positionX;
        }

        /// <summary>
        /// Buffer 상에서 최초에 생성된(티칭된) 좌표 Y
        /// </summary>
        public int PositionOriginY
        {
            get => this.positionY;
        }

        /// <summary>
        /// Buffer 상에서 Master Position에 의해서 변경된 좌표 X
        /// </summary>
        public int PositionOffsetX
        {
            get => this.positionX + this.offsetX;
        }

        /// <summary>
        /// Buffer 상에서 Master Position에 의해서 변경된 좌표 Y
        /// </summary>
        public int PositionOffsetY
        {
            get => this.positionY + this.offsetY;
        }

        /// <summary>
        /// Buffer 상에서 Master Position과 Chip Position에 의해서 변경된 좌표 X
        /// </summary>
        public int PositionX
        {
            get => this.positionX + this.offsetX + this.transX;
        }

        /// <summary>
        /// Buffer 상에서  Master Position에 Chip Position에 의해서 변경된 좌표 Y
        /// </summary>
        public int PositionY
        {
            get => this.positionY + this.offsetY + this.transY;
        }

        public int TransX
        {
            get => this.transX;
        }

        public int TransY
        {
            get => this.transY;
        }

        public int OffsetX
        {
            get => this.offsetX;
        }

        public int OffsetY
        {
            get => this.offsetY;
        }

        public int Width
        {
            get => this.width;
        }

        public int Height
        {
            get => this.height;
        }

        public WORK_TYPE WorkState
        {
            get => this.workState;
            set
            {
                SetProperty(ref this.workState, value);
                WorkEventManager.OnWorkplaceStateChanged(this, new WorkplaceStateChangedEventArgs(this));
            }
        }

        public int SharedBufferWidth
        {
            get => this.sharedBufferWidth;
        }

        public int SharedBufferHeight
        {
            get => this.sharedBufferHeight;
        }

        public int SharedBufferByteCnt
        {
            get => this.sharedBufferByteCnt;
        }

        public IntPtr SharedBufferR_GRAY
        {
            get => sharedBufferR_GRAY;
            private set => sharedBufferR_GRAY = value;
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

        public bool IsOccupied
        {
            get => this.isOccupied;
            set => this.isOccupied = value;
        }



        public List<Defect> DefectList
        {
            get => this.defectList;
            set => this.defectList = value;
        }
        private Dictionary<PREWORKDATA_KEY, object> PreworkDataDictionary { get => preworkdataDicitonary; set => preworkdataDicitonary = value; }

        #endregion


        public Workplace(int mapX, int mapY, int posX, int posY, int width, int height, int index)
        {
            this.mapIndexX = mapX;
            this.mapIndexY = mapY;
            this.positionX = posX;
            this.positionY = posY;
            this.width = width;
            this.height = height;
            this.index = index;
        }

        /// <summary>
        /// 검사에서 사용할 SharedBuffer를 설정합니다.
        /// </summary>
        /// <param name="sharedBufferR_Gray">R채널 혹은 Gray채널 버퍼 포인터</param>
        /// <param name="sharedBufferWidth">버퍼 너비</param>
        /// <param name="sharedBufferHeight">버퍼 높이</param>
        /// <param name="sharedBufferByteCnt">버퍼 채널 수</param>
        /// <param name="sharedBufferG">G채널 버퍼 포인터(없을 경우 IntPtr.Zero로 셋팅)</param>
        /// <param name="sharedBufferB">B채널 버퍼 포인터(없을 경우 IntPtr.Zero로 셋팅)</param>
        public void SetSharedBuffer(IntPtr sharedBufferR_GRAY, int sharedBufferWidth, int sharedBufferHeight, int sharedBufferByteCnt, IntPtr sharedBufferG, IntPtr sharedBufferB)
        {
            this.sharedBufferR_GRAY = sharedBufferR_GRAY;
            this.sharedBufferWidth = sharedBufferWidth;
            this.sharedBufferHeight = sharedBufferHeight;
            this.sharedBufferByteCnt = sharedBufferByteCnt;

            if (sharedBufferByteCnt != 1)
            {
                if (sharedBufferB == IntPtr.Zero)
                    throw new ArgumentException("SharedBufferB가 설정되지 않았습니다(ByteCnt == 3).", nameof(sharedBufferB));

                if (sharedBufferG == IntPtr.Zero)
                    throw new ArgumentException("SharedBufferG가 설정되지 않았습니다(ByteCnt == 3).", nameof(sharedBufferG));

                this.sharedBufferG = sharedBufferG;
                this.sharedBufferB = sharedBufferB;
            }
            else
            {
                sharedBufferB = IntPtr.Zero;
                sharedBufferG = IntPtr.Zero;
            }
        }

        public void SetSharedBuffer(SharedBufferInfo info)
        {
            this.sharedBufferR_GRAY = info.PtrR_GRAY;
            this.SharedBufferG = info.PtrG;
            this.SharedBufferB = info.PtrB;

            this.sharedBufferWidth = info.Width;
            this.sharedBufferHeight = info.Height;
            this.sharedBufferByteCnt = info.ByteCnt;
        }

        public IntPtr GetSharedBuffer(IMAGE_CHANNEL channel)
        {
            switch (channel)
            {
                case IMAGE_CHANNEL.R_GRAY:
                    return sharedBufferR_GRAY;
                case IMAGE_CHANNEL.G:
                    return sharedBufferG;
                case IMAGE_CHANNEL.B:
                    return sharedBufferB;
            }

            return sharedBufferR_GRAY;
        }

        public void SetOffset(int _offsetX, int _offsetY)
        {
            this.offsetX = _offsetX;
            this.offsetY = _offsetY;
        }

        public void AddOffset(int _offsetX, int _offsetY)
        {
            this.offsetX += _offsetX;
            this.offsetY += _offsetY;
        }

        public void SetTrans(int _transX, int _transY)
        {
            this.transX = _transX;
            this.transY = _transY;
        }

        public bool GetSubState(WORKPLACE_SUB_STATE state) => (subState & (int)state) == (int)state;

        public void SetSubState(WORKPLACE_SUB_STATE state, bool bTrue)
        {
            int tempState = GetSubState(state) ? subState - (int)state : subState;

            if (bTrue) subState = tempState + (int)state;
            else subState = tempState;
        }

        public void SetPreworkData(PREWORKDATA_KEY key, object dataObj)
        {
            if (this.preworkdataDicitonary.Count != Enum.GetValues(typeof(PREWORKDATA_KEY)).Length)
            {
                foreach (PREWORKDATA_KEY tempkey in Enum.GetValues(typeof(PREWORKDATA_KEY)))
                {
                    if (this.preworkdataDicitonary.ContainsKey(tempkey) == false)
                        this.preworkdataDicitonary.Add(tempkey, null);
                }
            }

            this.preworkdataDicitonary[key] = dataObj;
            //this.preworkDataList.Add(dataObj);
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

        public void AddMeasure()
        {
            Measurement measure = new Measurement();

            //defectList.Add(measure);
        }

        public object GetPreworkData(PREWORKDATA_KEY key)
        {
            if (preworkdataDicitonary.Count == 0) return null;

            return this.preworkdataDicitonary[key];
        }

        public void Reset()
        {
            this.WorkState = WORK_TYPE.NONE;

            this.preworkdataDicitonary.Clear();
            foreach (PREWORKDATA_KEY key in Enum.GetValues(typeof(PREWORKDATA_KEY)))
            {
                this.preworkdataDicitonary.Add(key, null);
            }

            this.offsetX = 0;
            this.offsetY = 0;
            this.transX = 0;
            this.transY = 0;

            this.isOccupied = false;
        }


        /// <summary>
        /// 초기 설정값과 SharedBuffer만 카피?
        /// </summary>
        public Workplace Clone()
        {
            Workplace wp = new Workplace(mapIndexX, mapIndexY, positionX, positionY, Width, Height, Index);

            wp.SetSharedBuffer(this.sharedBufferR_GRAY, this.sharedBufferWidth, this.SharedBufferHeight, this.sharedBufferHeight, this.sharedBufferG, this.sharedBufferB);

            return wp;
        }
    }
}