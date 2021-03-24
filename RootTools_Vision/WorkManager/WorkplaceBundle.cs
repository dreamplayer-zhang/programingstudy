using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    [Serializable]
    /// <history>
    /// 21.01.12 LHH
    /// - 기존에 WorkplaceBudnle 0번에 MasterWorkplace(MapIndex가 모두 -1)을 자동으로 생성하게 했으나
    ///   개발자가 직접 CreateWorkplaceBundle 메서드를 구현하여 사용에 맞게끔 생성하도록 변경
    /// </history>
    public class WorkplaceBundle : ObservableCollection<Workplace>
    {
        #region [Members]
        private int sizeX;
        private int sizeY;
        #endregion

        #region [Getter Setter]
        public int SizeX
        {
            get => this.sizeX;
            set => this.sizeX = value;
        }

        public int SizeY
        {
            get => this.sizeY;
            set => this.sizeY = value;
        }

        //public WorkplaceBundle() { }        

        //public WorkplaceBundle(SerializationInfo info, StreamingContext context)
        //{
        //    WorkplaceBundle wb = (WorkplaceBundle)info.GetValue("workplaceList", typeof(WorkplaceBundle));
        //    this.sizeX = wb.sizeX;
        //    this.sizeY = wb.sizeY;
        //    foreach(Workplace wp in wb)
        //    {
        //        this.Add(wp);
        //    }
        //}

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("workplaceList", this);
        //}
        #endregion

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
            foreach(Workplace wp in this)
            {
                wp.SetSharedBuffer(sharedBufferR_GRAY, sharedBufferWidth, sharedBufferHeight, sharedBufferByteCnt, sharedBufferG, sharedBufferB);
            }
        }

        public void SetSharedBuffer(SharedBufferInfo info)
        {
            foreach (Workplace wp in this)
            {
                wp.SetSharedBuffer(info);
            }
        }


        /// <summary>
        /// 맵 Index를 사용하여 Workplace를 가져온다
        /// </summary>
        /// <param name="mapX"></param>
        /// <param name="mapY"></param>
        /// <returns></returns>
        public Workplace GetWorkplace(int mapX, int mapY)
        {
            foreach(Workplace wp in this)
            {
                if(wp.MapIndexX == mapX && wp.MapIndexY == mapY)
                {
                    return wp;
                }
            }
            return null;
        }

        // 다른 쓰레드를 사용하는 WorkManager에서 동시에 접근할 수 있지만
        // WorkManager는 순차적인 상태를 체크하므로 Lock이 필요없음
        public Workplace GetWorkplaceRemained(WORK_TYPE preWorkType)
        {
            foreach(Workplace wp in this)
            {
                if(wp.WorkState == preWorkType && wp.IsOccupied == false)
                {
                    wp.IsOccupied = true;
                    return wp;
                }
            }

            return null;
        }

        /// <summary>
        /// 모든 workplace의 State를 변경합니다.
        /// </summary>
        /// <param name="type"></param>
        public void SetWorkState(WORK_TYPE type)
        {
            foreach (Workplace wp in this)
                wp.WorkState = type;
        }

        public bool CheckStateCompleted(WORK_TYPE state)
        {
            foreach (Workplace wp in this)
            {
                if (wp.WorkState < state)
                    return false;
            }
            return true;
        }
        public bool CheckStateAll(WORK_TYPE state)
        {
            foreach(Workplace wp in this)
            {
                if (wp.WorkState != state)
                    return false;
            }
            return true;
        }

        public bool CheckStateLine(int nLine, WORK_TYPE state)
        {
            bool bRst = true;
            foreach (Workplace workplace in this)
            {
                if (nLine < workplace.MapIndexX)
                    continue;

                if (nLine == workplace.MapIndexX && workplace.WorkState >= state)
                    continue;

                if (nLine == workplace.MapIndexX && workplace.WorkState < state) // 해당 Line에 State가 다른 workplace가 있으면 false
                {
                    bRst = false;
                    break;
                }

                if (nLine < workplace.MapIndexX) // 다음라인으로 넘어가면 검사 종료
                    break;
            }

            return bRst;
        }

        public void Reset()
        {
            foreach(Workplace wp in this)
            {
                wp.Reset();
            }
        }

        public WorkplaceBundle Clone()
        {
            WorkplaceBundle bundle = new WorkplaceBundle();

            foreach(Workplace wp in bundle)
            {
                bundle.Add(wp.Clone());
            }

            return bundle;
        }


    }
}
