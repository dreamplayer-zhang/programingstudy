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
    public class InspectionManagerFrontside : WorkFactory
    {

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

        public InspectionManagerFrontside(IntPtr _sharedBuffer, int _width, int _height)
        {
            this.sharedBufferR_Gray = _sharedBuffer;
            this.sharedBufferWidth = _width;
            this.sharedBufferHeight = _height;
            sharedBufferByteCnt = 1;
        }

        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", WORK_TYPE.ALIGNMENT, WORKPLACE_STATE.READY, WORKPLACE_STATE.SNAP, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("Inspection", WORK_TYPE.INSPECTION, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, STATE_CHECK_TYPE.CHIP, 4));
            this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("ProcessDefect_Wafer", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS_WAFER, WORKPLACE_STATE.DEFECTPROCESS, STATE_CHECK_TYPE.WAFER));


            WIND2EventManager.SnapDone += SnapDone_Callback;
        }

        private Recipe recipe;

        private IntPtr sharedBufferR_Gray;
        private IntPtr sharedBufferG;
        private IntPtr sharedBufferB;

        private int sharedBufferWidth;
        private int sharedBufferHeight;
        private int sharedBufferByteCnt;

        public Recipe Recipe { get => recipe; set => recipe = value; }
        public IntPtr SharedBufferR_Gray { get => sharedBufferR_Gray; set => sharedBufferR_Gray = value; }
        public IntPtr SharedBufferG { get => sharedBufferG; set => sharedBufferG = value; }
        public IntPtr SharedBufferB { get => sharedBufferB; set => sharedBufferB = value; }

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

        public override bool CreateInspection(Recipe _recipe)
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
                workplaceBundle.SetSharedBuffer(this.SharedBufferR_Gray, this.SharedBufferWidth, this.SharedBufferHeight, this.SharedBufferByteCnt);
                workplaceBundle.SetSharedRGBBuffer(this.SharedBufferR_Gray, this.SharedBufferG, this.SharedBufferB);

                workBundle = WorkBundle.CreateWorkBundle(_recipe, workplaceBundle);

                if(this.SetBundles(workBundle, workplaceBundle) == false)
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inspection 생성에 실패하였습니다.\n호출함수 : "+ MethodBase.GetCurrentMethod().Name + "\nDetail : " + ex.Message);
                return false;
            }

            return true;
        }

        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
            if (this.workplaceBundle == null) return; // 검사 진행중인지 확인하는 조건으로 바꿔야함

            Rect snapArea = new Rect(new Point(args.startPosition.X, args.startPosition.Y), new Point(args.endPosition.X, args.endPosition.Y));

            foreach (Workplace wp in this.workplaceBundle)
            {
                Rect checkArea = new Rect(new Point(wp.PositionX, wp.PositionY + wp.BufferSizeY), new Point(wp.PositionX + wp.BufferSizeX, wp.PositionY));

                if (snapArea.Contains(checkArea) == true)
                {
                    wp.STATE = WORKPLACE_STATE.SNAP;
                }
            }

        }

        //object lockObj = new object();
        //private void ChangedWorkplaceState_Callback(object _obj)
        //{
        //    lock (lockObj)
        //    {
        //        Workplace workplace = _obj as Workplace;

        //        if (MapStateChanged != null && workplace.MapPositionX >= 0 && workplace.MapPositionY >= 0)
        //            MapStateChanged(workplace.MapPositionX, workplace.MapPositionY, workplace.STATE);
        //    }
        //}

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

        public void SetWorkplaceBuffer(IntPtr ptrR, IntPtr ptrG, IntPtr ptrB)
        {
            this.SharedBufferR_Gray = ptrR;
            this.SharedBufferG = ptrG;
            this.SharedBufferB = ptrB;
        }

        public void Start(bool Snap)
        {
            if (this.Recipe == null && this.Recipe.WaferMap == null)
                return;

            if(Snap == false)
            {
                foreach (Workplace wp in this.workplaceBundle)
                {
                    wp.STATE = WORKPLACE_STATE.SNAP;
                }
            }

            Start();
        }

        public new void Stop()
        {
            base.Stop();
        }
    }
}
