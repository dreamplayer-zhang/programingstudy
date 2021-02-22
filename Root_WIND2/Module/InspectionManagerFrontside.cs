using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RootTools;
using RootTools.Database;
using RootTools_Vision;
using RootTools_Vision.Utility;


namespace Root_WIND2
{
    public class InspectionManagerFrontside : WorkFactory
    {
        #region [Members]
        private readonly RecipeFront recipe;
        //private readonly SharedBufferInfo sharedBufferInfo;

        private readonly Module.Vision vision;
        #endregion

        #region [Properties]
        public RecipeFront Recipe 
        { 
            get => recipe; 
        }

        
        public List<SharedBufferInfo> SharedBufferInfoList
        {
            get => this.sharedBufferInfoList;
        }
        #endregion

        public InspectionManagerFrontside(Module.Vision vision, RecipeFront recipe, SharedBufferInfo bufferInfo) : base(REMOTE_MODE.Master)
        {
            this.vision = vision;
            this.recipe = recipe;
            //this.sharedBufferInfo = bufferInfo;

            this.sharedBufferInfoList.Add(bufferInfo);
        }


        #region [Override]
        protected override void Initialize()
        {
            CreateWorkManager(WORK_TYPE.SNAP);
            CreateWorkManager(WORK_TYPE.ALIGNMENT);
            CreateWorkManager(WORK_TYPE.INSPECTION, 6);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS, 6);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);

            WIND2EventManager.SnapDone += SnapDone_Callback;
        }

        /// <summary>
        /// 다음 함수에서 생성한 WorkplaceBundle로 검사를 진행합니다.
        /// </summary>
        /// <returns></returns>
        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            RecipeType_WaferMap waferMap = recipe.WaferMap;

            if (waferMap == null || waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return null;
            }

            return CreateWorkplaceBundle_WaferMap();
        }

        /// <summary>
        /// 다음 함수에서 생성한 WorkBundle로 검사를 진행합니다.
        /// </summary>
        /// <returns></returns>
        protected override WorkBundle CreateWorkBundle()
        {
            List<ParameterBase> paramList = recipe.ParameterItemList;
            WorkBundle bundle = new WorkBundle();

            bundle.Add(new Snap());
            foreach (ParameterBase param in paramList)
            {
                WorkBase work = (WorkBase)Tools.CreateInstance(param.InspectionType);
                work.SetRecipe(recipe);
                work.SetParameter(param); // 같은 class를 사용하는 parameter 객체가 존재할 수 있으므로 반드시 work를 생성할 때 parameter를 셋팅

                bundle.Add(work);
            }

            ProcessDefect processDefect = new ProcessDefect();
            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();

            bundle.Add(processDefect);
            bundle.Add(processDefect_Wafer);

            return bundle;
        }


        /// <summary>
        /// 검사 정보(WorkplaceBundle, WorkBundle)을 생성 후 검사 시작 전에 상태를 확인하거나 변경합니다.
        /// </summary>
        /// <param name="workplaces"></param>
        /// <param name="works"></param>
        /// <returns>false를 반환하면 검사를 시작하지 않습니다.</returns>
        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            if (this.Recipe.WaferMap == null)
            {
                MessageBox.Show("맵정보가 존재하지 않습니다.");
                return false;
            }

            // DB?
            string lotId = "Lotid";
            string partId = "Partid";
            string setupId = "SetupID";
            string cstId = "CSTid";
            string waferId = "WaferID";
            //string sRecipe = "RecipeID";
            string recipeName = recipe.Name;

            DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

            // 레시피를 사용안하는 곳도 있을 있으므로...
            works.SetRecipe(recipe);

            return true;
        }

        #endregion


        public WorkplaceBundle CreateWorkplaceBundle_WaferMap()
        {
            RecipeType_WaferMap mapInfo = recipe.WaferMap;
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            PositionRecipe positionRecipe = recipe.GetItem<PositionRecipe>();
            PositionParameter positionParameter = recipe.GetItem<PositionParameter>();

            if (positionParameter == null) return null;

            WorkplaceBundle bundle = new WorkplaceBundle();
            try
            {
                int maxMasterFeaturePositionX = int.MinValue;
                int maxMasterFeaturePositionY = int.MinValue;
                int maxMasterFeatureWidth = int.MinValue;
                int maxMasterFeatureHeight = int.MinValue;
                foreach (RecipeType_ImageData feature in positionRecipe.ListMasterFeature)
                {
                    if (maxMasterFeaturePositionX < feature.PositionX + feature.Width)
                    {
                        maxMasterFeaturePositionX = feature.PositionX;
                        maxMasterFeatureWidth = feature.Width;
                    }

                    if (maxMasterFeaturePositionY < feature.PositionY + feature.Height)
                    {
                        maxMasterFeaturePositionY = feature.PositionY;
                        maxMasterFeatureHeight = feature.Height;
                    }
                }

                bundle.Add(new Workplace(-1, -1, maxMasterFeaturePositionX + originRecipe.OriginX + maxMasterFeatureWidth + positionParameter.WaferSearchRangeX, maxMasterFeaturePositionY + originRecipe.OriginY + maxMasterFeatureHeight + positionParameter.WaferSearchRangeY, 0, 0, bundle.Count));
                //bundle.Add(new Workplace(-1, -1, 0, 0, 0, 0, bundle.Count));

                var wafermap = mapInfo.Data;
                int nSizeX = mapInfo.MapSizeX;
                int nSizeY = mapInfo.MapSizeY;
                int nMasterX = mapInfo.MasterDieX;
                int nMasterY = mapInfo.MasterDieY;
                int nDiePitchX = originRecipe.DiePitchX;
                int nDiePitchY = originRecipe.DiePitchY;

                int nOriginAbsX = originRecipe.OriginX;
                int nOriginAbsY = originRecipe.OriginY - originRecipe.DiePitchY; // 좌상단 기준

                bundle.SizeX = nSizeX;
                bundle.SizeY = nSizeY;

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

                            Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);

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

                            Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);
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


                            Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);

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

                            Workplace workplace = new Workplace(x, y, nDieAbsX, nDieAbsY, nDiePitchX, nDiePitchY, bundle.Count);
                            bundle.Add(workplace);
                        }
                    }
                }

                bundle.SetSharedBuffer(this.sharedBufferInfoList[0]);
                this.workplaceBundle = bundle;
                return bundle;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Inspection 생성에 실패 하였습니다.\n", ex.Message);
            }
        }

        WorkplaceBundle workplaceBundle;
        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
            if (this.workplaceBundle == null || this.IsStop == true) return; // 검사 진행중인지 확인하는 조건으로 바꿔야함

            //Task.Delay(1000);
            Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));

            foreach (Workplace wp in this.workplaceBundle)
            {
                if (wp.WorkState >= WORK_TYPE.SNAP) continue;

                Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.Width), new Point(wp.PositionX + wp.Width, wp.PositionY));

                if (snapArea.Contains(checkArea) == true)
                {
                    wp.WorkState = WORK_TYPE.SNAP;
                }
            }
        }

        ~InspectionManagerFrontside()
        {

        }
    }
}
