using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RootTools;
using RootTools.Database;
using RootTools_Vision;


namespace Root_WIND2
{
    public class InspectionManagerBackside : WorkFactory
    {
        #region [Members]
        private readonly RecipeBack recipe;
        private SharedBufferInfo sharedBufferinfo;
        #endregion

        #region [Properties]
        public SharedBufferInfo SharedBufferInfo
        {
            get { return sharedBufferinfo; }
        }

        public RecipeBack Recipe
        {
            get => recipe;
        }
        #endregion

        public InspectionManagerBackside(RecipeBack _recipe, SharedBufferInfo bufferInfo)
        {
            this.recipe = _recipe;
            this.sharedBufferinfo = bufferInfo;
        }       


        public int[] mapdata = new int[14 * 14];




        private new void Start()
        {
            string lotId = "Lotid";
            string partId = "Partid";
            string setupId = "SetupID";
            string cstId = "CSTid";
            string waferId = "WaferID";
            //string sRecipe = "RecipeID";
            string recipeName = recipe.Name;

            DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

            base.Start();
        }

        #region [Overrides]
        protected override void Initialize()
        {
            CreateWorkManager(WORK_TYPE.SNAP);
            CreateWorkManager(WORK_TYPE.ALIGNMENT);
            CreateWorkManager(WORK_TYPE.INSPECTION, 8);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS, 8);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);
        }

        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            RecipeType_WaferMap waferMap = recipe.WaferMap;

            if (waferMap == null || waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return null;
            }

            WorkplaceBundle workplaceBundle = new WorkplaceBundle();

            return CreateWorkplaceBundle_WaferMap();
        }

        protected override WorkBundle CreateWorkBundle()
        {
            WorkBundle workBundle = new WorkBundle();

            BacksideAlignment alignment = new BacksideAlignment();

            BacksideSurface surface = new BacksideSurface();
            BacksideSurfaceParameter param = new BacksideSurfaceParameter();
            param.Intensity = 150;
            param.Size = 1;
            surface.SetParameter(param);

            ProcessDefect processDefect = new ProcessDefect();
            ProcessDefect_Backside processDefect_Backside = new ProcessDefect_Backside("defect");


            workBundle.Add(alignment);
            workBundle.Add(surface);
            workBundle.Add(processDefect);
            workBundle.Add(processDefect_Backside);

            //workBundle.SetRecipe(recipe); // Recipe에서?

            return workBundle;
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {           
            works.SetRecipe(recipe);

            return true;
        }

        #endregion

        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
            //if (this.workplaceBundle == null) return; // 검사 진행중인지 확인하는 조건으로 바꿔야함

            //Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));

            //foreach (Workplace wp in this.workplaceBundle)
            //{
            //    Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.BufferSizeY), new Point(wp.PositionX + wp.BufferSizeX, wp.PositionY));

            //    if (snapArea.Contains(checkArea) == true)
            //    {
            //        wp.STATE = WORK_TYPE.SNAP;
            //    }
            //}
        }


        public WorkplaceBundle CreateWorkplaceBundle_WaferMap()
        {
            RecipeType_WaferMap mapInfo = recipe.WaferMap;
            BacksideRecipe backRecipe = recipe.GetItem<BacksideRecipe>();

            WorkplaceBundle bundle = new WorkplaceBundle();
            try
            {                
                bundle.Add(new Workplace(-1, -1, 0, 0, 0, 0, bundle.Count));

                var wafermap = mapInfo.Data;
                int nSizeX = mapInfo.MapSizeX;
                int nSizeY = mapInfo.MapSizeY;
                int nMasterX = mapInfo.MasterDieX;
                int nMasterY = mapInfo.MasterDieY;
                int nDiePitchX = backRecipe.DiePitchX;
                int nDiePitchY = backRecipe.DiePitchY;

                int nOriginAbsX = backRecipe.OriginX;
                int nOriginAbsY = backRecipe.OriginY - backRecipe.DiePitchY; // 좌상단 기준

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

                bundle.SetSharedBuffer(this.sharedBufferinfo);
                return bundle;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Inspection 생성에 실패 하였습니다.\n", ex.Message);
            }
        }
    }
}
