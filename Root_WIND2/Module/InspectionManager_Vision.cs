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


        public InspectionManager_Vision(IntPtr _sharedBuffer, int _width, int _height)
        {
            this.sharedBuffer = _sharedBuffer;
            this.sharedBufferWidth = _width;
            this.sharedBufferHeight = _height;
            sharedBufferByteCnt = 1;
        }

        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("Inspection", WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, STATE_CHECK_TYPE.CHIP, 8));
            this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("ProcessDefect_Wafer", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS_WAFER, WORKPLACE_STATE.DEFECTPROCESS, STATE_CHECK_TYPE.WAFER));
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

		public byte[] WaferMapInfo = new byte[14 * 14];

        public void CreateInspecion_Backside()
        {
            //RecipeInfo_MapData mapInfo = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            RecipeInfo_MapData recipeInfo = recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            WaferMapInfo mapInfo = new WaferMapInfo(recipeInfo.m_WaferMap.nMapSizeX, recipeInfo.m_WaferMap.nMapSizeY, recipeInfo.m_WaferMap.pWaferMap, recipeInfo.m_WaferMap.ListWaferMap);

            WorkBundle works = new WorkBundle();

            Surface surface = new Surface();
            surface.SetData(recipe.GetRecipeData(), recipe.GetParameter());

            works.Add(surface);

            ProcessDefect processDefect = new ProcessDefect();
            works.Add(processDefect);

            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(mapInfo, this.recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin);            
            workplaces.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            processDefect_Wafer.SetWorkplaceBundle(workplaces);
            works.Add(processDefect_Wafer);

            workplaces.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);


            this.SetBundles(works, workplaces);
        }

        public void CreateInspecion(/*WaferMapInfo*/)
        {
            RecipeInfo_MapData TEST = recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;

            if (TEST.m_WaferMap == null)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return;
            }
           
            int InspectionNumber = 0;
            int nMapSize = 14;

            WaferMapInfo mapInfo = new WaferMapInfo(TEST.m_WaferMap.nMapSizeX, TEST.m_WaferMap.nMapSizeY, TEST.m_WaferMap.pWaferMap);
            WorkBundle works = new WorkBundle();
            Position position = new Position();
            Position_Chip position_chip = new Position_Chip();
            ParamData_Position param = recipe.GetParameter(typeof(ParamData_Position)) as ParamData_Position;
            param.SearchRangeX = 100;
            param.SearchRangeY = 100;
            param.MinScoreLimit = 60;

            position.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            position_chip.SetData(recipe.GetRecipeData(), recipe.GetParameter());


            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(mapInfo, this.recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin);

            Surface surface = new Surface();
            surface.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            surface.SetWorkplaceBundle(workplaces);

            D2D d2d = new D2D();
            d2d.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            d2d.SetWorkplaceBundle(workplaces);
            d2d.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            works.Add(position);
            works.Add(position_chip);
            works.Add(surface);
            works.Add(d2d);

            ProcessDefect processDefect = new ProcessDefect();
            works.Add(processDefect);
            workplaces.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetData(recipe.GetRecipeData(), recipe.GetParameter());
            processDefect_Wafer.SetWorkplaceBundle(workplaces);
            works.Add(processDefect_Wafer);

            workplaces.SetSharedBuffer(this.SharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);

            this.SetBundles(works, workplaces);
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
            if (this.Recipe == null)
                return;

            string lotId = "Lotid";
            string partId = "Partid";
            string setupId = "SetupID";
            string cstId = "CSTid";
            string waferId = "WaferID";
            //string sRecipe = "RecipeID";
            string recipeName = recipe.m_RecipeInfo.m_RecipeName;

            DatabaseManager.Instance.SetLotinfo(lotId, partId, setupId, cstId, waferId, recipeName);

            base.Start();
        }

        public new void Stop()
        {
            base.Stop();
        }
    }
}
