using System;
using System.Collections.Generic;
using System.Linq;
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
    public delegate void EventMapStateChanged(int x, int y, WORKPLACE_STATE state);

    public class InspectionManager_Vision : WorkFactory
    {

        public event EventMapStateChanged MapStateChanged;

        SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;


        #region [Member Variables]
        WorkBundle workBundle;
        WorkplaceBundle workplaceBundle;

        #endregion

        public InspectionManager_Vision(IntPtr _sharedBuffer, int _width, int _height)
        {
            this.sharedBuffer = _sharedBuffer;
            this.sharedBufferWidth = _width;
            this.sharedBufferHeight = _height;
            sharedBufferByteCnt = 1;
        }

        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.SNAP, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("Inspection", WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, STATE_CHECK_TYPE.CHIP, 8));
            this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("ProcessDefect_Wafer", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS_WAFER, WORKPLACE_STATE.DEFECTPROCESS, STATE_CHECK_TYPE.WAFER));


            WIND2EventManager.SnapDone += SnapDone_Callback;
        }

        private Recipe recipe;
        private IntPtr sharedBuffer;
        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;

        public Recipe Recipe { get => recipe; set => recipe = value; }
        public IntPtr SharedBuffer { get => sharedBuffer; set => sharedBuffer = value; }
        public int SharedBufferWidth { get => sharedBufferWidth; set => sharedBufferWidth = value; }
        public int SharedBufferHeight { get => sharedBufferHeight; set => sharedBufferHeight = value; }
        public int SharedBufferByteCnt { get => sharedBufferByteCnt; set => sharedBufferByteCnt = value; }

        public enum InspectionMode
        {
            FRONT,
            BACK,
            //EBR,
            //EDGE,
        }

        private InspectionMode inspectionMode = InspectionMode.FRONT;
        public InspectionMode p_InspectionMode { get => inspectionMode; set => inspectionMode = value; }

		public int[] mapdata = new int[14 * 14];


        public bool CreateInspection()
        {
            return CreateInspection(this.recipe);
        }

        protected override bool CreateInspection(Recipe _recipe)
        {
            try
            {
                RecipeType_WaferMap waferMap = _recipe.WaferMap;

                if (waferMap == null || waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0)
                {
                    MessageBox.Show("Map 정보가 없습니다.");
                    return false;
                }

                workplaceBundle = WorkplaceBundle.CreateWaferMap(_recipe);
                workplaceBundle.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
                workplaceBundle.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

                workBundle = WorkBundle.CreateWorkBundle(_recipe, workplaceBundle);

                this.SetBundles(workBundle, workplaceBundle);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Inspection 생성에 실패하였습니다.\nDetail : " + ex.Message);
                return false;
            }

            return true;
        }


        public void CreateInspecion_Backside()
        {
            //RecipeType_WaferMap mapInfo = m_Recipe.GetRecipeInfo(typeof(RecipeType_WaferMap)) as RecipeType_WaferMap;
            RecipeType_WaferMap waferMap = recipe.WaferMap;

            WorkBundle works = new WorkBundle();

            Surface surface = new Surface();
            surface.SetRecipe(recipe);

            works.Add(surface);

            ProcessDefect processDefect = new ProcessDefect();
            works.Add(processDefect);

            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(waferMap, this.recipe.GetRecipe<OriginRecipe>());            
            workplaces.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetRecipe(recipe);
            processDefect_Wafer.SetWorkplaceBundle(workplaces);
            works.Add(processDefect_Wafer);

            workplaces.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);


            this.SetBundles(works, workplaces);
        }



        // 삭제 예정
        //public void CreateInspecion(/*WaferMapInfo*/)
        //{


        //    RecipeType_WaferMap waferMap = recipe.WaferMap;

        //    if (waferMap == null || waferMap.MapSizeX == 0 || waferMap.MapSizeY == 0)
        //    {
        //        MessageBox.Show("Map 정보가 없습니다.");
        //        return;
        //    }
           
        //    WorkBundle works = new WorkBundle();
        //    Position position = new Position();
        //    position.SetRecipe(recipe);
        //    PositionParameter positionParam = recipe.GetRecipe<PositionParameter>();
        //    positionParam.SearchRangeX = 100;
        //    positionParam.SearchRangeY = 100;
        //    positionParam.MinScoreLimit = 60;

        //    position.SetRecipe(recipe);

        //    currentWorkplaceBundle = WorkplaceBundle.CreateWaferMap(waferMap, this.recipe.GetRecipe<OriginRecipe>());

        //    Surface surface = new Surface();
        //    surface.SetRecipe(recipe);
        //    surface.SetWorkplaceBundle(currentWorkplaceBundle);

        //    D2D d2d = new D2D();
        //    d2d.SetRecipe(recipe);
        //    d2d.SetWorkplaceBundle(currentWorkplaceBundle);

        //    works.Add(position);
        //    //works.Add(surface);
        //    //works.Add(d2d);

        //    ProcessDefect processDefect = new ProcessDefect();
        //    works.Add(processDefect);
        //    currentWorkplaceBundle.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

        //    ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
        //    processDefect_Wafer.SetRecipe(recipe);
        //    processDefect_Wafer.SetWorkplaceBundle(currentWorkplaceBundle);
        //    works.Add(processDefect_Wafer);

        //    currentWorkplaceBundle.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);

        //    this.SetBundles(works, currentWorkplaceBundle);
        //}

        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
            if (this.workplaceBundle == null) return;

            Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));
            
            foreach(Workplace wp in this.workplaceBundle)
            {
                Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.BufferSizeY), new Point(wp.PositionX + wp.BufferSizeX, wp.PositionY));
                
                if(snapArea.Contains(checkArea) == true)
                {
                    wp.STATE = WORKPLACE_STATE.SNAP;
                }
            }

        }

        object lockObj = new object();
        private void ChangedWorkplaceState_Callback(object _obj)
        {
            lock (lockObj)
            {
                Workplace workplace = _obj as Workplace;

                if (MapStateChanged != null && workplace.MapPositionX >= 0 && workplace.MapPositionY >= 0)
                        MapStateChanged(workplace.MapPositionX, workplace.MapPositionY, workplace.STATE);
            }
        }

        public new void Start()
        {
            if (this.Recipe == null && this.Recipe.WaferMap == null)
                return;

            foreach (Workplace wp in this.workplaceBundle)
            {
                wp.STATE = WORKPLACE_STATE.SNAP;
            }

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

        public new void Stop()
        {
            base.Stop();
        }
    }
}
