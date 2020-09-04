using RootTools;
using RootTools.Memory;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;

namespace RootTools_Vision
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        protected Dispatcher _dispatcher;

        public ImageData m_Image;
        string sPool = "pool";
        string sGroup = "group";
        string sMem = "mem";
        public BackgroundWorker Worker_ViewerUpdate = new BackgroundWorker();
        public int MemWidth = 40000;
        public int MemHeight = 40000;
        Vision_Engineer m_engineer;
        MemoryTool m_MemoryModule;
        public ImageViewer_ViewModel m_ImageViewer;
        //public DataViewer_ViewModel m_DataViewer;


        #region [ViewModel]
        MapViewer_ViewModel vmMapView = new MapViewer_ViewModel();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Timer t = new Timer(TimerCallback, null, 100, 100);
        }

        private WorkManager workManager;
        private WorkBundle workbundle;
        private WorkplaceBundle workplacebundle;

        private void TimerCallback(Object o)
        {

              
        }

        public void SetUIContext()
        {
            this.mapViewer.DataContext = vmMapView;
            this.view.DataContext = m_ImageViewer;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetUIContext();
          
            try
            {
                m_engineer = new Vision_Engineer();
                m_engineer.Init("Vision");

                _dispatcher = Dispatcher.CurrentDispatcher;

                m_MemoryModule = m_engineer.ClassMemoryTool();
                m_MemoryModule.GetPool(sPool, true).p_gbPool = 2;
                m_MemoryModule.GetPool(sPool, true).GetGroup(sGroup).CreateMemory(sMem, 1, 1, new CPoint(MemWidth, MemHeight));
                m_MemoryModule.GetMemory(sPool, sGroup, sMem);

                m_Image = new ImageData(m_MemoryModule.GetMemory(sPool, sGroup, sMem));
                m_ImageViewer = new ImageViewer_ViewModel(m_Image, null);

                view.DataContext = m_ImageViewer;
                btnImageOpen.DataContext = m_ImageViewer;
                //m_DataViewer = new DataViewer_ViewModel(DatabaseManager.Instance.m_DefectTable);
                //dataViewer.DataContext = m_DataViewer;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

           

            // Init WorkManager
            this.workManager = new WorkManager(8);
            
            this.workbundle = new WorkBundle();
            this.workplacebundle = new WorkplaceBundle();
            this.workManager.ChangedWorkState += ChangedWorkState_Callback;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.workManager.works.Count == 0 || this.workManager.workplaces.Count == 0)
                return;

            int workbundleIndex = Convert.ToInt32(this.tbWorkBundleIndex.Text);
            int workplacebundleIndex = Convert.ToInt32(this.tbWorkplaceBundleIndex.Text);
            this.workManager.SetWork(workbundleIndex, workplacebundleIndex);
            this.workManager.SetWorkResource(m_Image.m_ptrImg, m_Image.p_Size.X, m_Image.p_Size.Y);

            this.workManager.Start();


            SetInspectionMap(this.workManager.workplaces[workplacebundleIndex].UnitSizeX, this.workManager.workplaces[workplacebundleIndex].UnitSizeY);
        }

        private void BtnRegisterWorkBundle(object sender, RoutedEventArgs e)
        {
            if(this.workbundle.Count == 0)
            {
                return;
            }
            this.workManager.AddWorkBundle(new WorkBundle(this.workbundle));

            this.workbundle.Clear();

            RefeshWorkBundleStack();
            RefreshListWorkBundleStack();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            this.workManager.Stop();
        }

        private void BtnAddPosition(object sender, RoutedEventArgs e)
        {
            this.workbundle.Add(new Position());
            RefeshWorkBundleStack();
        }
        private void BtnAddPreInspection(object sender, RoutedEventArgs e)
        {
            this.workbundle.Add(new PreInspection());
            RefeshWorkBundleStack();
        }
        private void BtnAddInspectionSurface(object sender, RoutedEventArgs e)
        {
            
            Surface surface = new Surface();
            surface.SetData(m_Image.GetPtr(), new System.Windows.Size(m_Image.p_Size.X, m_Image.p_Size.Y), new System.Windows.Size(1000, 1000));
            SurfaceParameter param = new SurfaceParameter();
            param.IsDark = false;
            param.MinSize = 5;
            param.Threshold = 200;
            surface.SetParameter(param);

            this.workbundle.Add(surface);

            RefeshWorkBundleStack();
        }
        private void BtnAddInspectionD2D(object sender, RoutedEventArgs e)
        {
            this.workbundle.Add(new D2D());
            RefeshWorkBundleStack();
        }
        private void BtnAddMeasurement(object sender, RoutedEventArgs e)
        {
            this.workbundle.Add(new Measurement());
            RefeshWorkBundleStack();
        }

        private void BtnRegisterWorkplaceBundle(object sender, RoutedEventArgs e)
        {
            int sizeX = Convert.ToInt32(this.tbMapSizeX.Text);
            int sizeY = Convert.ToInt32(this.tbMapSizeY.Text);

            if (sizeX == 0 || sizeY == 0) return;

            byte[] wafermap = new byte[sizeX * sizeY];
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    double cx = (double)sizeX / 2.0f;
                    double cy = (double)sizeY / 2.0f;

                    double r = Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                    if(r <= cx)
                    {
                        wafermap[y * sizeX + x] = 1;
                    }
                    else
                    {
                        wafermap[y * sizeX + x] = 0;
                    }
                }
            }



            WaferMapInfo mapInfo = new WaferMapInfo(sizeX, sizeY, wafermap, 500, 500);
            

            WorkplaceBundle workplacebundle = WorkplaceBundle.CreateWaferMap(mapInfo);

            this.workManager.AddWorkplaceBundle(workplacebundle);

            this.workplacebundle.Clear();
            this.gridMap.Children.Clear();
            this.gridMap.RowDefinitions.Clear();
            this.gridMap.ColumnDefinitions.Clear();

            RefreshListWorkplaceBundleStack();
        }

        private void SetInspectionMap(int sizeX, int sizeY)
        {
            this.vmMapView.MapSize = new System.Windows.Point(0, 0);
            this.vmMapView.MapSize = new System.Windows.Point(sizeX, sizeY);
        }

        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;

        private void ChangedWorkState_Callback(WORK_TYPE work_type, WORKER_STATE worker_state, WORKPLACE_STATE workplace_state, int indexWorkplace, System.Drawing.Point SubIndex)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                TextBox tb = (TextBox)this.vmMapView.CellItems[(int)(SubIndex.X + SubIndex.Y * this.vmMapView.MapSize.X)];
                if(worker_state == WORKER_STATE.WORK_COMPLETED)
                {
                    tb.Background = brushComplete;
                    return;
                }

                switch (work_type)
                {
                    case WORK_TYPE.Position:
                        tb.Background = brushPosition;
                        break;
                    case WORK_TYPE.PreInspection:
                        tb.Background = brushPreInspection;
                        break;
                    case WORK_TYPE.Inspection:
                        tb.Background = brushInspection;
                        break;
                    case WORK_TYPE.Measurement:
                        tb.Background = brushMeasurement;
                        break;
                }
            }));
        }

        private void BtnCreateMap(object sender, RoutedEventArgs e)
        {
            this.gridMap.ShowGridLines = true;
            this.gridMap.Children.Clear();
            this.gridMap.RowDefinitions.Clear();
            this.gridMap.ColumnDefinitions.Clear();

            int sizeX = Convert.ToInt32(this.tbMapSizeX.Text);
            int sizeY = Convert.ToInt32(this.tbMapSizeY.Text);
            
            for(int x = 0; x < sizeX; x++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                this.gridMap.ColumnDefinitions.Add(cd);
            }

            for(int y = 0 ; y<sizeY; y++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                this.gridMap.RowDefinitions.Add(rd);
            }

            for(int y = 0; y < sizeY; y++)
            {
                for(int x = 0 ; x < sizeX; x++)
                {
                    TextBox tb = new TextBox();
                    tb.Text = string.Format("({0},{1})", x, y);
                    tb.TextAlignment = TextAlignment.Center;
                    tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    tb.Background = System.Windows.Media.Brushes.YellowGreen;
                    this.gridMap.Children.Add(tb);

                    Grid.SetRow(tb, y);
                    Grid.SetColumn(tb, x);
                }
            }
        }

        private void RefeshWorkBundleStack()
        {
            this.stackWorkBundle.Children.Clear();

            foreach(IWork work in this.workbundle)
            {
                TextBox tb = new TextBox();
                tb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                tb.Text = work.TYPE.ToString();
                tb.Height = 20;
                if (work.TYPE == WORK_TYPE.Position)
                {
                    tb.Background = System.Windows.Media.Brushes.GreenYellow;
                    tb.Text = work.TYPE.ToString();
                }
                else if (work.TYPE == WORK_TYPE.PreInspection)
                {
                    tb.Background = System.Windows.Media.Brushes.Cornsilk;
                    tb.Text = work.TYPE.ToString();
                }
                else if (work.TYPE == WORK_TYPE.Inspection)
                {
                    tb.Background = System.Windows.Media.Brushes.Gold;
                    tb.Text = work.TYPE.ToString() + " : " +  ((IInspection)work).TYPE.ToString();
                }
                else if (work.TYPE == WORK_TYPE.Measurement)
                {
                    tb.Background = System.Windows.Media.Brushes.CornflowerBlue;
                    tb.Text = work.TYPE.ToString();
                }

                this.stackWorkBundle.Children.Add(tb);
            }
        }

        private void RefreshListWorkBundleStack()
        {            
            this.stackListWorkBundle.Children.Clear();

            for (int i = 0; i < this.workManager.works.Count; i++ )
            {
                TextBox tb = new TextBox();
                tb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                tb.Height = 20;

                tb.Background = System.Windows.Media.Brushes.Orange;
                tb.Text = string.Format("WorkBundle[{0}]",i);

                this.stackListWorkBundle.Children.Add(tb);
            }
        }

        private void RefreshListWorkplaceBundleStack()
        {
            this.stackListWorkplaceBundle.Children.Clear();

            for (int i = 0; i < this.workManager.workplaces.Count; i++)
            {
                TextBox tb = new TextBox();
                tb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                tb.Height = 20;

                tb.Background = System.Windows.Media.Brushes.Orange;
                tb.Text = string.Format("WorkplaceBundle[{0}]", i);

                this.stackListWorkplaceBundle.Children.Add(tb);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //m_ImageViewer.ImageOpen();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //var bb = DatabaseManager.Instance.SelectData();
            ////m_DataViewer.SelectTable();
            //m_DataViewer.p_Dataset = DatabaseManager.Instance.m_DefectTable;

            //testest.TEST();      
            //dataViewer.DataContext = DatabaseManager.Instance.m_DefectTable;
            //dataViewer.ItemsSource = DatabaseManager.Instance.m_DefectTable.DefaultView;
        }

      


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string s = "truncate defectlist";
            //DatabaseManager.Instance.SendQuery(s);
        }
    }
}
