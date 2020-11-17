using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using RootTools;
using RootTools.Database;
using RootTools_Vision;


namespace Root_WIND2
{

    public delegate void EventMapStateChanged(int x, int y, WORKPLACE_STATE state);

    public class WIND2_InspectionManager : WorkFactory
    {
        public event EventMapStateChanged MapStateChanged;

        SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;


        public WIND2_InspectionManager(IntPtr _sharedBuffer, int _width, int _height)
        {
            this.m_ptrSharedBuffer = _sharedBuffer;
            this.m_SharedBufferWidth = _width;
            this.m_SharedBufferHeight = _height;
            m_SharedBufferByteCnt = 1;
        }

        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", RootTools_Vision.UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE));
            this.Add(new WorkManager("Inspection", RootTools_Vision.UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, false, 8));
            this.Add(new WorkManager("ProcessDefect", RootTools_Vision.UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION));
            this.Add(new WorkManager("ProcessDefect_Wafer", RootTools_Vision.UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS_WAFER, WORKPLACE_STATE.DEFECTPROCESS, true));
        }

        private IntPtr m_ptrSharedBuffer;
        private int m_SharedBufferWidth;
        private int m_SharedBufferHeight;
        private int m_SharedBufferByteCnt;

        private Recipe m_Recipe;

        public Recipe Recipe { get => m_Recipe; set => m_Recipe = value; }
        public IntPtr PtrSharedBuffer { get => m_ptrSharedBuffer; set => m_ptrSharedBuffer = value; }
        public int SharedBufferWidth { get => m_SharedBufferWidth; set => m_SharedBufferWidth = value; }
        public int SharedBufferHeight { get => m_SharedBufferHeight; set => m_SharedBufferHeight = value; }
        public int SharedBufferByteCnt { get => m_SharedBufferByteCnt; set => m_SharedBufferByteCnt = value; }

        public enum eInspectionMode
        {
            FRONT,
            BACK,
            EBR,
            EDGE,
        }
        public eInspectionMode m_InspectionMode = eInspectionMode.FRONT;
        public eInspectionMode p_InspectionMode { get => m_InspectionMode; set => m_InspectionMode = value; }

        public byte[] waferMapInfo = new byte[14*14];

        public void CreateInspecion_Backside()
        {
            //RecipeInfo_MapData mapInfo = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            RecipeInfo_MapData recipeInfo = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            WaferMapInfo mapInfo = new WaferMapInfo(recipeInfo.m_WaferMap.nMapSizeX, recipeInfo.m_WaferMap.nMapSizeY, recipeInfo.m_WaferMap.pWaferMap, recipeInfo.m_WaferMap.ListWaferMap);

            WorkBundle works = new WorkBundle();

            Surface surface = new Surface();
            surface.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());

            works.Add(surface);

            ProcessDefect processDefect = new ProcessDefect();
            works.Add(processDefect);

            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(mapInfo, this.m_Recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin);            
            workplaces.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());
            processDefect_Wafer.SetWorkplaceBundle(workplaces);
            works.Add(processDefect_Wafer);

            workplaces.SetSharedBuffer(this.PtrSharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);


            this.SetBundles(works, workplaces);
        }

        public void CreateInspecion(/*WaferMapInfo*/)
        {
            int InspectionNumber = 0;
            int nMapSize = 14;

            RecipeInfo_MapData TEST = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            WaferMapInfo mapInfo = new WaferMapInfo(TEST.m_WaferMap.nMapSizeX, TEST.m_WaferMap.nMapSizeY, TEST.m_WaferMap.pWaferMap);

            //WaferMapInfo mapInfo = new WaferMapInfo(nMapSize, nMapSize, waferMapInfo);

            WorkBundle works = new WorkBundle();
            Position position = new Position();
            Position_Chip position_chip = new Position_Chip();
            ParamData_Position param = m_Recipe.GetParameter(typeof(ParamData_Position)) as ParamData_Position;
            param.SearchRangeX = 100;
            param.SearchRangeY = 100;
            param.MinScoreLimit = 60;

            position.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());
            position_chip.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());


            //Surface surface = new Surface();
            //surface.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());
            D2D d2d = new D2D();
            d2d.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());
            works.Add(position);
            works.Add(position_chip);
            //works.Add(surface);
            works.Add(d2d);

            ProcessDefect processDefect = new ProcessDefect();
            works.Add(processDefect);

            WorkplaceBundle workplaces = WorkplaceBundle.CreateWaferMap(mapInfo, this.m_Recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin);
            workplaces.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetData(m_Recipe.GetRecipeData(), m_Recipe.GetParameter());
            processDefect_Wafer.SetWorkplaceBundle(workplaces);
            works.Add(processDefect_Wafer);

            workplaces.SetSharedBuffer(this.PtrSharedBuffer, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);


            this.SetBundles(works, workplaces);
        }

        object lockObj = new object();
        private void ChangedWorkplaceState_Callback(object obj)
        {
            lock (lockObj)
            {
                Workplace workplace = obj as Workplace;

                if (MapStateChanged != null && workplace.MapPositionX >= 0 && workplace.MapPositionY >= 0)
                        MapStateChanged(workplace.MapPositionX, workplace.MapPositionY, workplace.STATE);
            }
        }


        public new void Start()
        {
            if (this.Recipe == null) 
                return;

            string sLot = "Lotid";
            string sPart = "Partid";
            string sSetup = "SetupID";
            string sCst = "CSTid";
            string sWafer = "WaferID";
            //string sRecipe = "RecipeID";
            string sRecipe = m_Recipe.m_RecipeInfo.m_RecipeName;

            DatabaseManager.Instance.SetLotinfo(sLot, sPart, sSetup, sCst,sWafer, sRecipe);

            base.Start();
        }

        public new void Stop()
        {
            base.Stop();
        }
    }
}
