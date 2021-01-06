using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using System.Windows.Threading;
using RootTools.Database;


namespace Root_WIND2
{
    class BacksideInspection_ViewModel: ObservableObject
    {
        Setup_ViewModel m_Setup;
        Recipe m_Recipe;

        private MapControl_ViewModel m_MapControl_VM;
        public MapControl_ViewModel p_MapControl_VM
        {
            get
            {
                return m_MapControl_VM;
            }
            set
            {
                SetProperty(ref m_MapControl_VM, value);
            }
        }

        private FrontsideInspection_ImageViewer_ViewModel m_DrawTool_VM;
        public FrontsideInspection_ImageViewer_ViewModel p_DrawTool_VM
        {
            get
            {
                return m_DrawTool_VM;
            }
            set
            {
                SetProperty(ref m_DrawTool_VM, value);
            }
        }

        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public DispatcherTimer timer;

        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }


        public void init(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = setup.Recipe;

            p_MapControl_VM = new MapControl_ViewModel(ProgramManager.Instance.InspectionBack);
            p_DrawTool_VM = new FrontsideInspection_ImageViewer_ViewModel();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(100000);
            timer.Tick += new EventHandler(timer_Tick);
            //DatabaseManager.Instance.SetDatabase(1);

            WorkEventManager.PositionDone += PositionDone_Callback;
            WorkEventManager.InspectionDone += SurfaceInspDone_Callback;
            WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;
        }

        object lockObj = new object();
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            lock (this.lockObj)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    String test = "";
                    if (true) // Display Option : Position Trans
                    {
                        test += "Trans : {" + workplace.TransX.ToString() + ", " + workplace.TransY.ToString() + "}" + "\n";
                    }
                    if (workplace.Index == 0)
                        DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                    else
                        DrawRectChipFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                }));
            }
        }
        private void SurfaceInspDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();
            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                if (false) // Display Option : Rel Position
                    text += "Pos : {" + defectInfo.m_fRelX.ToString() + ", " + defectInfo.m_fRelY.ToString() + "}" + "\n";
                if (false) // Display Option : Defect Size
                    text += "Size : " + defectInfo.m_fSize.ToString() + "\n";
                if (false) // Display Option : GV Value
                    text += "GV : " + defectInfo.m_fGV.ToString() + "\n";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList, args.reDraw);
            }));
        }

        private void ProcessDefectDone_Callback(object obj, PocessDefectDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                UpdateDataGrid();
            }));
        }

        private void DrawDone_Callback(CPoint leftTop, CPoint rightBottom)
        {
            p_DrawTool_VM.Clear();
            this.m_DrawTool_VM.DrawRect(leftTop, rightBottom, FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatchingFail);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //DatabaseManager.Instance.SelectData();
            ////m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }

        public void SetPage(UserControl page)
        {
            RecipeType_WaferMap waferMap = m_Recipe.WaferMap;

            if (waferMap.Data != null)
            {
                int nMapX = waferMap.MapSizeX;
                int nMapY = waferMap.MapSizeY;

                p_MapControl_VM.SetMap(waferMap.Data, new CPoint(nMapX, nMapY));
            }
            else
            {
                p_MapControl_VM.SetMap(m_Setup.InspectionVision.mapdata, new CPoint(14, 14));
            }

            p_DrawTool_VM.Clear();
        }


        public ICommand btnInspTestStart
        {
            get
            {
                return new RelayCommand(_btnTest);
            }
        }
        public ICommand btnInspTestSnap
        {
            get
            {
                return new RelayCommand(_btnSnap);
            }
        }
        public ICommand btnInspTestStop
        {
            get
            {
                return new RelayCommand(_btnStop);
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(m_Setup.SetWizardBackSide);
            }
        }

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, FrontsideInspection_ImageViewer_ViewModel.ColorType.MasterFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess ? FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatching : FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatchingFail, text);
        }

        public void DrawRectShotFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, FrontsideInspection_ImageViewer_ViewModel.ColorType.ShotFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatching, text);
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, FrontsideInspection_ImageViewer_ViewModel.ColorType.ChipFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, bSuccess ? FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatching : FrontsideInspection_ImageViewer_ViewModel.ColorType.FeatureMatchingFail, text);
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
        {
            if (reDraw)
                p_DrawTool_VM.Clear();

            p_DrawTool_VM.DrawRect(rectList, FrontsideInspection_ImageViewer_ViewModel.ColorType.Defect, text);
        }

        public void UpdateDataGrid()
        {
            //DatabaseManager.Instance.SelectData();
            //m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }
        private void _btnStop()
        {
            timer.Stop();
            m_Setup.InspectionVision.Stop();
            DatabaseManager.Instance.SelectData();
            m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }

        public void _btnSnap()
        {

        }

        public virtual void _btnTest()
        {
            //Temp
            timer.Start();

            //if (DatabaseManager.Instance.GetConnectionStatus())
            //{
            //    string sTableName = "wind2.defect";
            //    DatabaseManager.Instance.ClearTableData(sTableName);
            //}

            //m_Setup.InspectionManager.Recipe.GetParameter().Save();

            p_DrawTool_VM.Clear();

            IntPtr SharedBuf = new IntPtr();
            if (p_DrawTool_VM.p_ImageData.p_nByte == 3)
            {
                if (p_DrawTool_VM.p_eColorViewMode != RootViewer_ViewModel.eColorViewMode.All)
                    SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr((int)p_DrawTool_VM.p_eColorViewMode - 1);
                else // All 일때는 R채널로...
                    SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr(0);

                m_Setup.InspectionVision.SetWorkplaceBuffer(SharedBuf, p_DrawTool_VM.p_ImageData.GetPtr(0), p_DrawTool_VM.p_ImageData.GetPtr(1), p_DrawTool_VM.p_ImageData.GetPtr(2));
            }
            else
            {
                SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr();
                m_Setup.InspectionVision.SharedBuffer = SharedBuf;
            }

            m_Setup.InspectionVision.SharedBufferByteCnt = p_DrawTool_VM.p_ImageData.p_nByte;

            if (m_Setup.InspectionVision.CreateInspection() == false)
            {
                return;
            }
            m_Setup.InspectionVision.Start(false);

        }


        public void LoadInspTestData()
        {
            RecipeType_WaferMap mapdata = m_Recipe.WaferMap;
            if (mapdata.Data != null)
            {
                int nMapX = mapdata.MapSizeX;
                int nMapY = mapdata.MapSizeY;

                p_MapControl_VM.SetMap(mapdata.Data, new CPoint(nMapX, nMapY));
                p_MapControl_VM.CreateMapUI();
            }
        }
    }
}
