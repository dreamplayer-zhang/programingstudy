using RootTools;
using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

    [Serializable]
    public enum PREWORKDATA_KEY // PreworkdataList의 index로 반드시 0부터 빈틈없이 추가
    {
        D2D_GOLDEN_IMAGE = 0,
        D2D_SCALE_MAP = 1,
    }

    [Serializable]
    /// <summary>
    /// - 검사할 영역의 정보를 포함합니다.
    /// - Image Buffer를 직접할당하지 않습니다.
    /// - offset과 trans 이외의 값은 처음 생성할 떄 값을 할당받고 변경되지 않습니다.
    /// </summary>
    public class Workplace : ObservableObject, ISerializable
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

        private SharedBufferInfo sharedBufferInfo;
        
        private IntPtr sharedBufferR_GRAY;
        private IntPtr sharedBufferG;
        private IntPtr sharedBufferB;

        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;
        
        private WORK_TYPE workState;

        private WorkplaceBundle parentBundle;

        private bool snapDone = false;

        private CameraInfo cameraInfo;

        [NonSerialized] private List<Defect> defectList = new List<Defect>();
        [NonSerialized] private List<Measurement> measureList = new List<Measurement>();
        [NonSerialized] private Dictionary<PREWORKDATA_KEY, object> preworkdataDicitonary = new Dictionary<PREWORKDATA_KEY, object>();

        [NonSerialized] private bool isOccupied = false;
        [NonSerialized] private int subState;

        public Workplace(SerializationInfo info, StreamingContext context)
        {
            this.index = (int)info.GetValue(nameof(index), typeof(int));
            this.mapIndexX = (int)info.GetValue(nameof(mapIndexX), typeof(int));
            this.mapIndexY = (int)info.GetValue(nameof(mapIndexY), typeof(int));
            this.positionX = (int)info.GetValue(nameof(positionX), typeof(int));
            this.positionY = (int)info.GetValue(nameof(positionY), typeof(int));
            this.offsetX = (int)info.GetValue(nameof(offsetX), typeof(int));
            this.offsetY = (int)info.GetValue(nameof(offsetY), typeof(int));
            this.transX = (int)info.GetValue(nameof(transX), typeof(int));
            this.transY = (int)info.GetValue(nameof(transY), typeof(int));
            this.width = (int)info.GetValue(nameof(width), typeof(int));
            this.height = (int)info.GetValue(nameof(height), typeof(int));
            this.sharedBufferR_GRAY = (IntPtr)info.GetValue(nameof(sharedBufferR_GRAY), typeof(IntPtr));
            this.sharedBufferG = (IntPtr)info.GetValue(nameof(sharedBufferG), typeof(IntPtr));
            this.sharedBufferB = (IntPtr)info.GetValue(nameof(sharedBufferB), typeof(IntPtr));
            this.sharedBufferWidth = (int)info.GetValue(nameof(sharedBufferWidth), typeof(int));
            this.sharedBufferHeight = (int)info.GetValue(nameof(sharedBufferHeight), typeof(int));
            this.sharedBufferByteCnt = (int)info.GetValue(nameof(sharedBufferByteCnt), typeof(int));
            this.workState = (WORK_TYPE)info.GetValue(nameof(workState), typeof(WORK_TYPE));
            this.defectList = (List<Defect>)info.GetValue(nameof(defectList), typeof(List<Defect>));
            this.measureList = (List<Measurement>)info.GetValue(nameof(measureList), typeof(List<Measurement>));
            this.preworkdataDicitonary = (Dictionary<PREWORKDATA_KEY, object>)info.GetValue(nameof(preworkdataDicitonary), typeof(Dictionary<PREWORKDATA_KEY, object>));
            this.parentBundle = (WorkplaceBundle)info.GetValue(nameof(parentBundle), typeof(WorkplaceBundle));
            this.snapDone = (bool)info.GetValue(nameof(snapDone), typeof(bool));
            this.cameraInfo = (CameraInfo)info.GetValue(nameof(cameraInfo), typeof(CameraInfo));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(index), index);
            info.AddValue(nameof(mapIndexX), mapIndexX);
            info.AddValue(nameof(mapIndexY), mapIndexY);
            info.AddValue(nameof(positionX), positionX);
            info.AddValue(nameof(positionY), positionY);
            info.AddValue(nameof(offsetX), offsetX);
            info.AddValue(nameof(offsetY), offsetY);
            info.AddValue(nameof(transX), transX);
            info.AddValue(nameof(transY), transY);
            info.AddValue(nameof(width), width);
            info.AddValue(nameof(height), height);
            info.AddValue(nameof(sharedBufferR_GRAY), sharedBufferR_GRAY);
            info.AddValue(nameof(sharedBufferG), sharedBufferG);
            info.AddValue(nameof(sharedBufferB), sharedBufferB);
            info.AddValue(nameof(sharedBufferWidth), sharedBufferWidth);
            info.AddValue(nameof(sharedBufferHeight), sharedBufferHeight);
            info.AddValue(nameof(sharedBufferByteCnt), sharedBufferByteCnt);
            info.AddValue(nameof(workState), workState);
            info.AddValue(nameof(defectList), defectList, typeof(List<Defect>));
            info.AddValue(nameof(measureList), measureList, typeof(List<Measurement>));
            info.AddValue(nameof(preworkdataDicitonary), preworkdataDicitonary, typeof(Dictionary<PREWORKDATA_KEY, object>));
            info.AddValue(nameof(parentBundle), parentBundle, typeof(WorkplaceBundle));
            info.AddValue(nameof(snapDone), snapDone, typeof(bool));
            info.AddValue(nameof(CameraInfo), cameraInfo, typeof(CameraInfo));
        }
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

        public bool SnapDone
        {
            get => this.snapDone;
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
        
        public SharedBufferInfo SharedBufferInfo
        {
            get => sharedBufferInfo;
            private set => sharedBufferInfo = value;
        }

        public bool IsOccupied
        {
            get => this.isOccupied;
            set => this.isOccupied = value;
        }


        public WorkplaceBundle ParentBundle
        {
            get => this.parentBundle;
            set => this.parentBundle = value;
        }

        public CameraInfo CameraInfo
        {
            get => this.cameraInfo;
        }

        
        
        public List<Defect> DefectList
        {
            get => this.defectList;
            set => this.defectList = value;
        }

        public List<Measurement> MeasureList
		{
            get => this.measureList;
            set => this.measureList = value;
		}
        private Dictionary<PREWORKDATA_KEY, object> PreworkDataDictionary { get => preworkdataDicitonary; set => preworkdataDicitonary = value; }

        #endregion


        public Workplace()
        {
            this.mapIndexX = 0;
            this.mapIndexY = 0;
            this.positionX = 0;
            this.positionY = 0;
            this.width = 0;
            this.height = 0;
            this.index = 0;
        }
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

        public void CheckSnapDone_Line(CRect snapArea)
        {
            if (this.positionX + this.width <= snapArea.Right)
            {
                this.snapDone = true;
            }
            else
                this.snapDone = false;
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
            this.sharedBufferInfo.PtrR_GRAY = sharedBufferR_GRAY;
            this.sharedBufferInfo.Width = sharedBufferWidth;
            this.sharedBufferInfo.Height = sharedBufferHeight;
            this.sharedBufferInfo.ByteCnt = sharedBufferByteCnt;

            if (sharedBufferByteCnt != 1)
            {
                if (sharedBufferB == IntPtr.Zero)
                    throw new ArgumentException("SharedBufferB가 설정되지 않았습니다(ByteCnt == 3).", nameof(sharedBufferB));

                if (sharedBufferG == IntPtr.Zero)
                    throw new ArgumentException("SharedBufferG가 설정되지 않았습니다(ByteCnt == 3).", nameof(sharedBufferG));

                this.sharedBufferInfo.PtrG = sharedBufferG;
                this.sharedBufferInfo.PtrB = sharedBufferB;
            }
            else
            {
                sharedBufferB = IntPtr.Zero;
                sharedBufferG = IntPtr.Zero;
            }

            // 기존
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

        public void SetCameraInfo(CameraInfo cameraInfo)
        {
            this.cameraInfo.RealResX = cameraInfo.RealResX;
            this.cameraInfo.RealResY = cameraInfo.RealResY;
            this.cameraInfo.TargetResX = cameraInfo.TargetResX;
            this.cameraInfo.TargetResY = cameraInfo.TargetResY;
        }

        public void SetSharedBuffer(SharedBufferInfo info)
        {
            this.sharedBufferInfo = info;

            // 기존
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
        

        public IntPtr GetSharedBufferInfo(IMAGE_CHANNEL channel)
        {
            switch (channel)
            {
                case IMAGE_CHANNEL.R_GRAY:
                    return sharedBufferInfo.PtrR_GRAY;
                case IMAGE_CHANNEL.G:
                    return sharedBufferInfo.PtrG;
                case IMAGE_CHANNEL.B:
                    return sharedBufferInfo.PtrB;
            }

            return sharedBufferInfo.PtrR_GRAY;
        }

        public IntPtr GetSharedBufferInfo(int memnum)
        {
            return sharedBufferInfo.PtrList[memnum];
        }
        public int GetSharedBufferInfoListSize()
        {
            return SharedBufferInfo.PtrList.Count;
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

            if (defectList == null) defectList = new List<Defect>();

            defectList.Add(defect);
        }


        private object lockObj = new object();
        public void AddDefect(string sInspectionID, int defectCode, float defectSz, float defectVal, float defectRelX, float defectRelY, float defectAbsLeft, float defectAbsTop, float defectW, float defectH, int chipIdxX, int chipIdxY) // SurfaceDefectParam
        {
            lock (lockObj)
            {
                Defect defect = new Defect(sInspectionID,
                    defectCode,
                    defectSz,
                    defectVal,
                    defectW,
                    defectH,
                    defectRelX,
                    defectRelY,
                    defectAbsLeft,
                    defectAbsTop,
                    chipIdxX,
                    chipIdxY);

                defectList.Add(defect);
            }
        }

        public void AddMeasurement(string strInspectionID, string strSide, Measurement.MeasureType type, Measurement.EBRMeasureItem measureItem, float fData, float fDefectW, float fDefectH, float fAngle, float fDefectAbsLeft, float fDefectAbsTop, int nChipIdxX, int nChipIdxY, float fRelX = 0, float fRelY = 0)
		{
            Measurement measurement = new Measurement(strInspectionID,
                                                      strSide,
                                                      type.ToString(),
                                                      measureItem.ToString(),
                                                      fData,
                                                      fDefectW,
                                                      fDefectH,
                                                      fAngle,
                                                      fDefectAbsLeft,
                                                      fDefectAbsTop,
                                                      nChipIdxX,
                                                      nChipIdxY,
                                                      fRelX,
                                                      fRelY);

            measureList.Add(measurement);
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
        /*
        public Workplace Clone()
        {
            Workplace wp = new Workplace(mapIndexX, mapIndexY, positionX, positionY, Width, Height, Index);

            wp.SetSharedBuffer(this.sharedBufferR_GRAY, this.sharedBufferWidth, this.SharedBufferHeight, this.sharedBufferHeight, this.sharedBufferG, this.sharedBufferB);

            return wp;
        }
        */

        public Workplace Clone()
        {
            Workplace wp = new Workplace(mapIndexX, mapIndexY, positionX, positionY, Width, Height, Index);
            wp.ParentBundle = this.ParentBundle;
            //wp.WorkState = this.WorkState;
            wp.SetSharedBuffer(this.sharedBufferInfo);

            return wp;
        }
    }
}