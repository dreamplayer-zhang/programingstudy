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
    class BacksideInspTest_ViewModel : ObservableObject
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

        private DrawTool_ViewModel m_DrawTool_VM;
        public DrawTool_ViewModel p_DrawTool_VM
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

        public BacksideInspTest_Panel Main;
        public InspTestPage InspTest;

        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public DispatcherTimer timer;

        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }


        public BacksideInspTest_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = setup.Recipe;

            init(setup);
        }


        public void init(Setup_ViewModel setup)
        {
            Main = new BacksideInspTest_Panel();
            InspTest = new InspTestPage();

            p_MapControl_VM = new MapControl_ViewModel(m_Setup.InspectionManagerVision);
            p_DrawTool_VM = new DrawTool_ViewModel(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(100000);
            timer.Tick += new EventHandler(timer_Tick);
            //zDatabaseManager.Instance.SetDatabase(1);

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //DatabaseManager.Instance.SelectData();
            //m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }

        public void SetPage(UserControl page)
        {
            RecipeInfo_MapData mapdata = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;

            if (mapdata.m_WaferMap != null)
            {
                int nMapX = mapdata.m_WaferMap.nMapSizeX;
                int nMapY = mapdata.m_WaferMap.nMapSizeY;

                p_MapControl_VM.SetMap(mapdata.m_WaferMap.pWaferMap, new CPoint(nMapX, nMapY));
            }
            else
            {
                p_MapControl_VM.SetMap(m_Setup.InspectionManagerVision.WaferMapInfo, new CPoint(14, 14));
            }


            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);

            p_DrawTool_VM.Clear();
        }


        public ICommand btnInspTestStart
        {
            get
            {
                return new RelayCommand(_btnTest);
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

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text)
        {
            p_DrawTool_VM.DrawRect(ptOldStart, ptOldEnd, DrawTool_ViewModel.ColorType.ChipFeature);
            p_DrawTool_VM.DrawRect(ptNewStart, ptNewEnd, DrawTool_ViewModel.ColorType.FeatureMatching, text);
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
        {
            if (reDraw)
                p_DrawTool_VM.Clear();

            p_DrawTool_VM.DrawRect(rectList, DrawTool_ViewModel.ColorType.Defect, text);
        }

        public void UpdateDataGrid()
        {
            //DatabaseManager.Instance.SelectData();
            //m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
        }
        private void _btnStop()
        {
            //timer.Stop();
            DatabaseManager.Instance.SelectData();
            m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;

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
            }
            else
            { 
                SharedBuf = p_DrawTool_VM.p_ImageData.GetPtr();
            }

            m_Setup.InspectionManagerVision.SharedBuffer = SharedBuf;
            m_Setup.InspectionManagerVision.SharedBufferByteCnt = p_DrawTool_VM.p_ImageData.p_nByte;
            m_Setup.InspectionManagerVision.CreateInspecion_Backside();
            m_Setup.InspectionManagerVision.Start();

        }
    }
}
