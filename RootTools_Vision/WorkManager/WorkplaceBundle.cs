using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public delegate void EventStateChanged(object obj);


    public class WorkplaceBundle : Collection<Workplace>
    {
        public event EventStateChanged WorkplaceStateChanged;


        #region [Variables]
        private int mapSizeX;
        private int mapSizeY;

        //private int masterPositionX;
        //private int masterPositionY;

        //private bool isMasterDetected;
        #endregion


        #region [Getter Setter]
        public int MapSizeX { get => mapSizeX; set => mapSizeX = value; }
        public int MapSizeY { get => mapSizeY; set => mapSizeY = value; }

        // 첫번째 Workplace를 마스터 workplace로
        //public int MasterPositionX { get => masterPositionX; set => masterPositionX = value; }
        //public int MasterPositionY { get => masterPositionY; set => masterPositionY = value; }
        //public bool IsMasterDetected { get => isMasterDetected; private set => isMasterDetected = value; }
        #endregion

        public WorkplaceBundle()
        {
            // Master를 무조건 0번째 workplace
            this.Add(new Workplace(-1, -1, 0, 0, 0, 0, 0));
        }

        public void Reset()
        {
            foreach(Workplace workplace in this)
            {
                workplace.Reset();
            }
        }

        public void SetSharedBuffer(IntPtr sharedBuffer, int width, int height)
        {
            foreach (Workplace workplace in this)
            {
                workplace.SetSharedBuffer(sharedBuffer, width, height);
            }
        }

        public new void Add(Workplace workplace)
        {
            workplace.StateChanged += WorkplaceStateChanged_Callback;
            workplace.PositionUpdated += WorkplacePositionUpdated_Callback;
            workplace.PositionIntialized += WorkplacePositionInitialized_Callback;
            workplace.Index = this.Count;
            base.Add(workplace);
        }

        public Workplace GetWorkplaceByState(WORKPLACE_STATE state)
        {
            foreach(Workplace workplace in this)
            {
                if(workplace.STATE == state)
                {
                    return workplace;
                }
            }

            return null;
        }

        public void WorkplaceStateChanged_Callback(object obj)
        {
            if (WorkplaceStateChanged != null)
                WorkplaceStateChanged(obj);
        }

        public void WorkplacePositionInitialized_Callback(object obj, int transX, int transY)
        {
            Workplace workplace = obj as Workplace;
            foreach(Workplace wp in this)
            {
                if (wp.Index == workplace.Index) continue;

                wp.SetImagePositionByTrans(transX, transY);
            }
        }


        public object objLock = new object();
        public void WorkplacePositionUpdated_Callback(object obj)
        {
            lock (objLock)
            {
                Workplace workplace = obj as Workplace;

                int mapX = workplace.MapPositionX;
                int mapY = workplace.MapPositionY;

                int transX = workplace.TransX;
                int transY = workplace.TransY;

                foreach (Workplace wp in this)
                {
                    if (wp.MapPositionX > mapX + 1) continue;

                    if (wp.MapPositionX == mapX && wp.MapPositionY == mapY + 1) // 같은 라인인 경우 Y축만 업데이트
                    {
                        wp.MoveImagePosition(transX, transY);  // Trans를 누적하지 않는다.
                    }

                    // 위에서 아래로 Position을 하기 때문에 오른쪽 칩중에 현재 라인에 없는 칩만 포지셔닝 하면됨
                    // 아래쪽에서 오른쪽에 현재 라인에 없는 칩은 해당 라인 포지셔닝 할때 위에 조건에서 알아서 튜닝됨
                    if (wp.MapPositionX == mapX + 1 && wp.MapPositionY <= mapY) 
                    {
                        wp.MoveImagePosition(transX, transY);
                    }
                }
            }
        }


        public static WorkplaceBundle CreateWaferMap(WaferMapInfo mapInfo)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();


            byte[] wafermap = mapInfo.WaferMapData;
            int nSizeX = mapInfo.MapSizeX;
            int nSizeY = mapInfo.MapSizeY;
            int nMasterX = mapInfo.MasterDieX;
            int nMasterY = mapInfo.MasterDieY;
            int nDiePitchX = mapInfo.DieSizeX;    //DitPitch 필요없음 삭제 예정
            int nDiePitchY = mapInfo.DieSizeY;


            bundle.mapSizeX = nSizeX;
            bundle.mapSizeY = nSizeY;

            // Right
            for (int x = nMasterX; x < nSizeX; x++)
            {
                // Top
                for (int y = nMasterY; y >= 0; y--)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        Workplace workplace = new Workplace(x, y, x * nDiePitchX, y * nDiePitchY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        Workplace workplace = new Workplace(x, y, x * nDiePitchX, y * nDiePitchY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }
            }


            // Left
            for (int x = nMasterX - 1; x >= 0; x--)
            {
                // Top
                for (int y = nMasterY; y >= 0; y--)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        Workplace workplace = new Workplace(x, y, x * nDiePitchX, y * nDiePitchY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        Workplace workplace = new Workplace(x, y, x * nDiePitchX, y * nDiePitchY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }
            }

            return bundle;
        }
    }
}
