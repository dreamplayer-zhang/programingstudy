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
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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

        private WorkFactory factory;

        private List<WorkBundle> workbundleList;
        private List<WorkplaceBundle> workplacebundleList;

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
            this.factory = new WorkFactory();

            this.factory.Add(new WorkManager("Position", UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE));
            this.factory.Add(new WorkManager("Inspection", UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, true, 8));
            this.factory.Add(new WorkManager("ProcessDefect", UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION));


            this.workbundle = new WorkBundle();
            this.workplacebundle = new WorkplaceBundle();


            this.workbundleList = new List<WorkBundle>();
            this.workplacebundleList = new List<WorkplaceBundle>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.workbundleList.Count == 0 || this.workbundleList.Count == 0)
                return;

            int workbundleIndex = Convert.ToInt32(this.tbWorkBundleIndex.Text);
            int workplacebundleIndex = Convert.ToInt32(this.tbWorkplaceBundleIndex.Text);

            this.workplacebundleList[workplacebundleIndex].SetSharedBuffer(m_Image.GetPtr(), m_Image.p_Size.X, m_Image.p_Size.Y);

            this.factory.SetBundles(this.workbundleList[workbundleIndex], this.workplacebundleList[workplacebundleIndex]);

            this.factory.Start();


            SetInspectionMap(this.workplacebundleList[workplacebundleIndex].MapSizeX, this.workplacebundleList[workplacebundleIndex].MapSizeY);
        }

        private void BtnRegisterWorkBundle(object sender, RoutedEventArgs e)
        {
            if(this.workbundle.Count == 0)
            {
                return;
            }
            
           
            this.workbundleList.Add(workbundle.Clone());
            this.workbundle.Clear();

            RefeshWorkBundleStack();
            RefreshListWorkBundleStack();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            this.factory.Stop();
        }

        private void BtnAddPosition(object sender, RoutedEventArgs e)
        {
            Temp_Recipe.Recipe recipe = new Temp_Recipe.Recipe();

            Temp_Recipe.Parameter parameter = new Temp_Recipe.Parameter();

            //Recipe Data


            Bitmap bitmap = new Bitmap(@"D:\test\template images\template3.bmp");

            byte[] tplBuffer = new byte[bitmap.Width * bitmap.Height];

            BitmapData bmpData =
                bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), //bitmap 영역
                    ImageLockMode.ReadOnly,  //읽기 모드
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed); //bitmap 형식
            IntPtr ptr = bmpData.Scan0;  //비트맵의 첫째 픽셀 데이터 주소를 가져오거나 설정합니다.
            Marshal.Copy(ptr, tplBuffer, 0, bitmap.Width * bitmap.Height);
            bitmap.UnlockBits(bmpData);


            RecipePosition recipePosition = recipe.GetRecipe(typeof(RecipePosition)) as RecipePosition;

            recipePosition.ListMasterFeature.Add(new RecipeType_FeatureData(49, 49, 40, 40, tplBuffer));

            Temp_Recipe.ParameterPosition param = parameter.GetParameter(typeof(Temp_Recipe.ParameterPosition)) as Temp_Recipe.ParameterPosition;

            param.MinScoreLimit = 80;
            param.SearchRangeX = 100;
            param.SearchRangeY = 100;

            Position position = new Position();
            //position.SetData(recipe.GetRecipeData(typeof(RecipePosition)), parameter.GetParameter(typeof(Temp_Recipe.ParameterPosition)));
            this.workbundle.Add(position);

            RefeshWorkBundleStack();
        }

        private void BtnAddInspectionSurface(object sender, RoutedEventArgs e)
        {
            Surface surface = new Surface();
            //surface.SetData(m_Image.GetPtr(), new System.Windows.Size(m_Image.p_Size.X, m_Image.p_Size.Y), new System.Windows.Size(1000, 1000));
            //SurfaceParameter param = new SurfaceParameter();
            //param.IsDark = false;
            //param.MinSize = 5;
            //param.Threshold = 200;
            //surface.SetParameter(param);

            this.workbundle.Add(surface);

            RefeshWorkBundleStack();
        }
        //private void BtnAddInspectionD2D(object sender, RoutedEventArgs e)
        //{
        //    this.workbundle.Add(new D2D());
        //    RefeshWorkBundleStack();
        //}

        private void BtnAddProcessDefect(object sender, RoutedEventArgs e)
        {
            this.workbundle.Add(new ProcessDefect());
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



            //WaferMapInfo mapInfo = new WaferMapInfo(sizeX, sizeY, wafermap, 1430, 1090);
            WaferMapInfo mapInfo = new WaferMapInfo(sizeX, sizeY, wafermap);

            WorkplaceBundle workplacebundle = new WorkplaceBundle();
            //workplacebundle = WorkplaceBundle.CreateWaferMap(mapInfo , );
            workplacebundle.WorkplaceStateChanged += ChangedWorkplaceState_Callback;

            this.workplacebundleList.Add(workplacebundle);

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

        SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;

        object lockObj = new object();
        private void ChangedWorkplaceState_Callback(object obj)
        {
            lock(lockObj)
            {
                Workplace workplace = obj as Workplace;

                //string str;

                //str = string.Format("{0} {1} {2}", workplace.Index, workplace.MapPositionX, workplace.MapPositionY);
                //MessageBox.Show(str);



                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {

                    int workbundleIndex = Convert.ToInt32(this.tbWorkBundleIndex.Text);
                    int workplacebundleIndex = Convert.ToInt32(this.tbWorkplaceBundleIndex.Text);

                    if (workplace.Index == 0) return;

                    TextBox tb = (TextBox)this.vmMapView.CellItems[(int)(workplace.MapPositionX + workplace.MapPositionY * this.workplacebundleList[workplacebundleIndex].MapSizeX)];

                    switch (workplace.STATE)
                    {
                        case WORKPLACE_STATE.NONE:
                            //tb.Background = brushPosition;
                            break;
                        case WORKPLACE_STATE.SNAP:
                            tb.Background = brushPreInspection;
                            break;
                        case WORKPLACE_STATE.READY:
                            tb.Background = brushPosition;
                            break;
                        case WORKPLACE_STATE.INSPECTION:
                            tb.Background = brushInspection;
                            break;
                        case WORKPLACE_STATE.DEFECTPROCESS:
                            tb.Background = brushComplete;
                            break;
                    }
                }));
            }
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

            foreach (WorkBase work in this.workbundle)
            {
                TextBox tb = new TextBox();
                tb.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                tb.Text = work.GetType().Name;
                tb.Height = 20;
                this.stackWorkBundle.Children.Add(tb);
            }
        }

        private void RefreshListWorkBundleStack()
        {            
            this.stackListWorkBundle.Children.Clear();

            for (int i = 0; i < this.workbundleList.Count; i++ )
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

            for (int i = 0; i < this.workplacebundleList.Count; i++)
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
