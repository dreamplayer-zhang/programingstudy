using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public void SetSharedBuffer(IntPtr sharedBuffer, int width, int height, int byteCnt = 1)
        {
            foreach (Workplace workplace in this)
            {
                workplace.SetSharedBuffer(sharedBuffer, width, height, byteCnt);
            }
        }
        public void SetSharedRGBBuffer(IntPtr sharedBufferR, IntPtr sharedBufferG, IntPtr sharedBufferB)
        {
            foreach (Workplace workplace in this)
            {
                workplace.SetSharedRGBBuffer(sharedBufferR, sharedBufferG, sharedBufferB);
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

        private object lockObj = new object();
        public Workplace GetWorkplaceByState(WORKPLACE_STATE state)
        {
            lock(lockObj)
            {
                foreach (Workplace workplace in this)
                {
                    if (workplace.STATE == state && workplace.IsOccupied == false)
                    {
                        workplace.IsOccupied = true;
                        return workplace;
                    }
                }
            }

            return null;
        }

        public bool CheckStateAll(WORKPLACE_STATE state)
        {
            foreach(Workplace workplace in this)
            {
                if(workplace.STATE != state)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckStateLine(int nLine, WORKPLACE_STATE state)
        {
            bool bRst = true;
            foreach(Workplace workplace in this)
            {
                if (nLine < workplace.MapPositionX)
                    continue;

                if (nLine == workplace.MapPositionX && workplace.STATE == state)
                    continue;

                if (nLine == workplace.MapPositionX && workplace.STATE != state) // 해당 Line에 State가 다른 workplace가 있으면 false
                {
                    bRst = false;
                    break;
                }

                if (nLine < workplace.MapPositionX) // 다음라인으로 넘어가면 검사 종료
                    break;
            }

            return bRst;
        }

        public void SetStateAll(WORKPLACE_STATE state)
        {
            foreach (Workplace workplace in this)
            {
                workplace.STATE = state;
            }
        }

        public void SetStateLine(int nLine, WORKPLACE_STATE state)
        {
            foreach (Workplace workplace in this)
            {
                if (nLine > workplace.MapPositionX)
                    continue;

                if (nLine == workplace.MapPositionX)
                {
                    workplace.STATE = state;
                }

                if (nLine < workplace.MapPositionX)
                    break;
            }
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

        public static WorkplaceBundle CreateWaferMap(Recipe _recipe)
        {

            RecipeType_WaferMap mapInfo = _recipe.WaferMap;
            OriginRecipe originRecipe = _recipe.GetRecipe<OriginRecipe>();

            WorkplaceBundle bundle = new WorkplaceBundle();


            var wafermap = mapInfo.Data;
            int nSizeX = mapInfo.MapSizeX;
            int nSizeY = mapInfo.MapSizeY;
            int nMasterX = mapInfo.MasterDieX;
            int nMasterY = mapInfo.MasterDieY;
            int nDiePitchX = originRecipe.DiePitchX;    //DitPitch 필요없음 삭제 예정
            int nDiePitchY = originRecipe.DiePitchY;

            int nOriginAbsX = originRecipe.OriginX;
            int nOriginAbsY = originRecipe.OriginY;

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
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);

                        if (y == nMasterY)
                        {
                            workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                        }

                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);
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
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;


                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);

                        if (y == nMasterY)
                        {
                            workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                        }

                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }
            }

            return bundle;
        }


        public static WorkplaceBundle CreateWaferMap(RecipeType_WaferMap mapInfo, OriginRecipe originRecipe)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();


            var wafermap = mapInfo.Data;
            int nSizeX = mapInfo.MapSizeX;
            int nSizeY = mapInfo.MapSizeY;
            int nMasterX = mapInfo.MasterDieX;
            int nMasterY = mapInfo.MasterDieY;
            int nDiePitchX = originRecipe.DiePitchX;    //DitPitch 필요없음 삭제 예정
            int nDiePitchY = originRecipe.DiePitchY;

            int nOriginAbsX = originRecipe.OriginX;
            int nOriginAbsY = originRecipe.OriginY;

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
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);

                        if(y ==  nMasterY)
                        {
                            workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                        }

                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);
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
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;


                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);

                        if (y == nMasterY)
                        {
                            workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                        }

                        bundle.Add(workplace);
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        int distX = x - nMasterX;
                        int distY = y - nMasterY;
                        int nDieAbsX = nOriginAbsX + distX * nDiePitchX;
                        int nDieAbsY = nOriginAbsY + distY * nDiePitchY;

                        Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY);
                        bundle.Add(workplace);
                    }
                }
            }

            return bundle;
        }

        // edge 검사할때 workplace 영역설정 하려고 만든거
        public static WorkplaceBundle CreateWorkplaceBundle(int shareBufferWidth, int sharedBufferHeight)
		{
            WorkplaceBundle bundle = new WorkplaceBundle();
            
            // edge는 map이 없음
            int mapX = 0;
            int mapY = 0;
            int posX = 0;
            int posY = 0;

            int roiHeight = 1000; // 검사 영역으로 자를 높이 - recipe
            int workplaceNum;

            if (sharedBufferHeight % roiHeight == 0)
                workplaceNum = sharedBufferHeight / roiHeight;
            else
                workplaceNum = (sharedBufferHeight / roiHeight) + 1;

            for (int i = 0; i < workplaceNum; i++)
            {
                Workplace workplace = new Workplace(mapX, mapY, posX, posY + (roiHeight*i), shareBufferWidth, roiHeight);
                bundle.Add(workplace);
            }

            return bundle;
        }
    }
}
