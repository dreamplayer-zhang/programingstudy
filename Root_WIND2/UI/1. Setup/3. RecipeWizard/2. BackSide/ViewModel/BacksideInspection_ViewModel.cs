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

            p_MapControl_VM = new MapControl_ViewModel();
            p_DrawTool_VM = new FrontsideInspection_ImageViewer_ViewModel();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(100000);
            timer.Tick += new EventHandler(timer_Tick);
            //DatabaseManager.Instance.SetDatabase(1);

            GlobalObjects.Instance.Get<InspectionManagerBackside>().PositionDone += PositionDone_Callback;
            GlobalObjects.Instance.Get<InspectionManagerBackside>().InspectionDone += SurfaceInspDone_Callback;
            GlobalObjects.Instance.Get<InspectionManagerBackside>().ProcessDefectDone += ProcessDefectDone_Callback;
        }


        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;

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
        private void SurfaceInspDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();


            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";
                /*
                if (false) // Display Option : Rel Position
                    text += "Pos : {" + defectInfo.m_fRelX.ToString() + ", " + defectInfo.m_fRelY.ToString() + "}" + "\n";
                if (false) // Display Option : Defect Size
                    text += "Size : " + defectInfo.m_fSize.ToString() + "\n";
                if (false) // Display Option : GV Value
                    text += "GV : " + defectInfo.m_fGV.ToString() + "\n";
                */
                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList, args.reDraw);
            }));
        }

        private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs args)
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
            p_MapControl_VM.SetMap();
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

            GlobalObjects.Instance.Get<InspectionManagerFrontside>().Stop();

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

            GlobalObjects.Instance.Get<InspectionManagerFrontside>().Start(WORK_TYPE.SNAP);
        }


        public void LoadInspTestData()
        {
            p_MapControl_VM.SetMap();
            p_MapControl_VM.CreateMapUI();
        }
    }
}
