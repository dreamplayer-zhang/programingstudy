using RootTools;
using RootTools.Database;
using RootTools_Vision;
using RootTools_CLR;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;
using System.Collections;
using System.Reflection;

namespace Root_WIND2
{
    class Review_ViewModel : ObservableObject
    {
        public Review_ViewModel(Review review)
        {
            init();

           
            _selectedStartDate = DateTime.Now.Date;
            _selectedEndDate = DateTime.Now.Date;
        }


        RecipeBase recipe;
        List<Defect> m_ReviewDefectlist;

        private DateTime _selectedStartDate;
        private DateTime _selectedEndDate;

        private ObservableCollection<UIElement> m_Element = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_Element
        {
            get
            {
                return m_Element;
            }
            set
            {
                SetProperty(ref m_Element, value);
            }
        }

        public DefectView m_DefectView = new DefectView();


        public void init()
        {
            p_Element.Add(m_DefectView);
            recipe = new RecipeFront();
        }

        #region Command Btn
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UIManager.Instance.ChangeUIMode();
                });
            }
        }
        public ICommand btnSearch
        {
            get
            {
                return new RelayCommand(SearchLotinfoData);
            }
        }
        public ICommand btnLoadImage
        {
            get
            {
                return new RelayCommand(LoadGoldenImage);
            }
        }
        public ICommand btnCheckAll
        {
            get
            {
                return new RelayCommand(SearchLotinfoData);
            }
        }
        public ICommand btnUncheckAll
        {
            get
            {
                return new RelayCommand(SearchLotinfoData);
            }
        }
        public ICommand btnShowTrend
        {
            get
            {
                return new RelayCommand(GoldenImageTrend);
            }
        }
        public ICommand btnSaveTrend
        {
            get
            {
                return new RelayCommand(SaveTrendImg);
            }
        }
        public void GoldenImagelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0) ;
        }

        #endregion

        #region GET / SET

        private BitmapSource m_DefectImage;
        public BitmapSource p_DefectImage
        {
            get
            {
                return m_DefectImage;
            }
            set
            {
                SetProperty(ref m_DefectImage, value);
            }

        }

        private Database_DataView_VM m_DataViewer_Lotinfo = new Database_DataView_VM();
        public Database_DataView_VM p_DataViewer_Lotinfo
        {
            get { return this.m_DataViewer_Lotinfo; }
            set { SetProperty(ref m_DataViewer_Lotinfo, value); }
        }

        DataTable Defect_Datatable;
        public DataTable pDefect_Datatable
        {
            get => Defect_Datatable;
            set => SetProperty(ref Defect_Datatable, value);
        }

        DataTable lotinfo_Datatable;
        public DataTable pLotinfo_Datatable
        {
            get => lotinfo_Datatable;
            set => SetProperty(ref lotinfo_Datatable, value);
        }

        private object selectedItem_Lotinfo;
        public object pSelected_Lotinfo
        {
            get => selectedItem_Lotinfo;
            set
            {
                SetProperty(ref selectedItem_Lotinfo, value);

                DataRowView selectedRow = (DataRowView)pSelected_Lotinfo;
                if (selectedRow != null)
                {
                    //string sInspectionID = (string)selectedRow.Row.ItemArray[0]; // Temp
                    FieldInfo[] lotinfoFieldInfos = null;
                    Type lotinfoType = typeof(Lotinfo);
                    lotinfoFieldInfos = lotinfoType.GetFields(BindingFlags.Instance | BindingFlags.Public);

                    try
                    {
                        string sInspectionID = (string)GetDataGridItem(lotinfo_Datatable, selectedRow, lotinfoFieldInfos[2].Name);
                        string sReicpePath = @"C:\Root\Recipe";
                        string sRecipeID = (string)GetDataGridItem(lotinfo_Datatable, selectedRow, lotinfoFieldInfos[6].Name);
                        string sReicpeFileName = sRecipeID + ".rcp";
                        recipe.Read(Path.Combine(sReicpePath, sRecipeID, sReicpeFileName));
                        m_DefectView.SetRecipe(recipe);
                        DisplayDefectData(sInspectionID);

                        m_DefectView.tbRcpName.Text = sRecipeID.ToString();
                        m_DefectView.tbWaferID.Text = (string)GetDataGridItem(lotinfo_Datatable, selectedRow, lotinfoFieldInfos[5].Name);
                        m_DefectView.tbTotalCnt.Text = m_ReviewDefectlist.Count.ToString() + " (EA)";
                        m_DefectView.tb_EdgeCnt.Text = m_ReviewDefectlist.Count.ToString() + " (EA)";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("pSelected_Lotinfo : " + ex.Message);
                    }
                }
            }
        }

        private object selectedItem_Defect;
        public object pSelected_Defect
        {
            get => selectedItem_Defect;
            set
            {
                SetProperty(ref selectedItem_Defect, value);

                DataRowView selectedRow = (DataRowView)pSelected_Defect;
                if (selectedRow != null)
                {
                    FieldInfo[] defectFieldInfos = null;
                    Type defectType = typeof(Defect);
                    defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

                    // BMP Open Event
                    int nIndex = (int)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[0].Name);
                    string sInspectionID = (string)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[1].Name);
                    string sFileName = nIndex.ToString() + ".bmp";
                    DisplayDefectImage(sInspectionID, sFileName);

                    // Defect Code 보고... 나중에 회의
                    //if (defect.m_nDefectCode / 10000 == 1)  // Frontside
                    {
                        //DisplaySelectedFrontDefect(selectedRow);
                    }
                    //else if (defect.m_nDefectCode / 10000 == 2)  // Backside
                    {
                        //DisplaySelectedBackDefect(selectedRow);
                    }
                    //else if (defect.m_nDefectCode / 10000 == 3) // Edge
                    {
                        DisplayEdgeSelectedDefect(selectedRow); // To-do edge 전용으로만 보이니까 추후에 구조 수정필요
                    }

                }
            }
        }

        // Defect GV Distribution Graph 
        private SeriesCollection defectGVHistogram;
        public SeriesCollection DefectGVHistogram
        {
            get
            {
                return defectGVHistogram;
            }
            set
            {
                defectGVHistogram = value;
                RaisePropertyChanged("DefectGVHistogram");
            }
        }
        private string[] gvLabels;
        public string[] GVXLabel
        {
            get
            {
                return gvLabels;
            }
            set
            {
                gvLabels = value;
                RaisePropertyChanged("GVXLabel");
            }
        }
        public string GVXTitle { get; set; }
        public Func<int, string> GVYLabel { get; set; }
        public string GVYTitle { get; set; }

        // Graph Mode에 따라서 Graph의 범위 (From~To) ,보여지는 Bin 개수 (sliderScale)변함
        private int sliderScale = 128 / 2;
        private double gvFrom = 0;
        private double gvTo = 128 / 2;

        public double GVFrom
        {
            get { return gvFrom; }
            set
            {
                gvFrom = (value > 255 - sliderScale) ? 255 - sliderScale : value;

                gvFrom = (value < 0) ? 0 : gvFrom;
                RaisePropertyChanged("GVFrom");
            }
        }
        public double GVTo
        {
            get { return gvTo; }
            set
            {
                gvTo = (value > 255) ? 255 : value;
                RaisePropertyChanged("GVTo");
            }
        }

        // Histogram 이동
        private int gvSliderVal = 0;
        public int Slider_GV
        {
            get { return gvSliderVal; }
            set
            {
                if (gvHistogramMode == GVHistogramType.All)
                {
                    gvSliderVal = value;
                    GVTo = sliderScale + gvSliderVal;
                    GVFrom = 0 + gvSliderVal;
                }
            }
        }

        // 이거 없으면 Y값이 bin들의 값에따라 계속 변함
        private int gvYMaxVal = 100;
        public int GVYMaxVal
        {
            get { return gvYMaxVal; }
            set
            {
                gvYMaxVal = value;
                RaisePropertyChanged("GVYMaxVal");
            }
        }

        // Radio Button으로 출력 Histogram 형태 조절
        public bool Check_GVHistogramAll
        {
            get
            {
                return gvHistogramMode == GVHistogramType.All;
            }
            set
            {
                gvHistogramMode = value ? GVHistogramType.All : gvHistogramMode;
                if (gvHistogramMode == GVHistogramType.All)
                    sliderScale = 100;

                gvSliderVal = 0;
                GVTo = sliderScale + gvSliderVal;
                GVFrom = 0 + gvSliderVal;
                RaisePropertyChanged("Slider_GV");
                DrawDefectGVGraph();
            }

        }
        public bool Check_GVHistogramDark
        {
            get
            {
                return gvHistogramMode == GVHistogramType.Dark;
            }
            set
            {
                gvHistogramMode = value ? GVHistogramType.Dark : gvHistogramMode;
                if (gvHistogramMode == GVHistogramType.Dark)
                    sliderScale = 128 / 2;

                gvSliderVal = 0;
                GVTo = sliderScale;
                GVFrom = 0;
                RaisePropertyChanged("Slider_GV");
                DrawDefectGVGraph();
            }
        }
        public bool Check_GVHistogramBright
        {
            get
            {
                return gvHistogramMode == GVHistogramType.Bright;
            }
            set
            {
                gvHistogramMode = value ? GVHistogramType.Bright : gvHistogramMode;
                if (gvHistogramMode == GVHistogramType.Bright)
                    sliderScale = 128 / 2;

                gvSliderVal = 0;
                GVTo = sliderScale;
                GVFrom = 0;
                RaisePropertyChanged("Slider_GV");
                DrawDefectGVGraph();
            }
        }

        // Defect Size Distribution Graph 
        private SeriesCollection defectSizeHistogram;
        public SeriesCollection DefectSizeHistogram
        {
            get
            {
                return defectSizeHistogram;
            }
            set
            {
                defectSizeHistogram = value;
                RaisePropertyChanged("DefectSizeHistogram");
            }
        }
        private string[] sizeLabels;
        public string[] SizeXLabel
        {
            get
            {
                return sizeLabels;
            }
            set
            {
                sizeLabels = value;
                RaisePropertyChanged("SizeXLabel");
            }
        }
        public string SizeXTitle { get; set; }
        public Func<int, string> SizeYLabel { get; set; }
        public string SizeYTitle { get; set; }

        // Graph Mode에 따라서 Graph의 범위 (From~To) ,보여지는 Bin 개수 (sliderScale)변함
        private double sizeFrom = 0;
        private double sizeTo = 50;

        public double SizeFrom
        {
            get { return sizeFrom; }
            set
            {
                sizeFrom = value;
                RaisePropertyChanged("SizeFrom");
            }
        }
        public double SizeTo
        {
            get { return sizeTo; }
            set
            {
                sizeTo = value;
                RaisePropertyChanged("SizeTo");
            }
        }

        // Histogram 이동
        private int sizeSliderVal = 0;
        public int Slider_Size
        {
            get { return sizeSliderVal; }
            set
            {
                if (sizeHistogramMode == SizeHistogramType.All)
                {
                    sizeSliderVal = value;
                    SizeTo = 100 + sizeSliderVal;
                    SizeFrom = 0 + sizeSliderVal;
                }
            }
        }

        // 이거 없으면 Y값이 bin들의 값에따라 계속 변함
        private int sizeYMaxVal = 100;
        public int SizeYMaxVal
        {
            get { return sizeYMaxVal; }
            set
            {
                sizeYMaxVal = value;
                RaisePropertyChanged("SizeYMaxVal");
            }
        }

        // Radio Button으로 출력 Histogram 형태 조절
        public bool Check_SizeHistogramAll
        {
            get
            {
                return sizeHistogramMode == SizeHistogramType.All;
            }
            set
            {
                sizeHistogramMode = value ? SizeHistogramType.All : sizeHistogramMode;

                if (sizeHistogramMode == SizeHistogramType.All)
                {
                    gvSliderVal = 0;
                    SizeTo = 100 + gvSliderVal;
                    SizeFrom = 0 + gvSliderVal;
                    RaisePropertyChanged("Slider_Size");
                    DrawDefectSizeGraph();
                }
            }

        }
        public bool Check_SizeHistogramSmall
        {
            get
            {
                return sizeHistogramMode == SizeHistogramType.Small;
            }
            set
            {
                sizeHistogramMode = value ? SizeHistogramType.Small : sizeHistogramMode;

                if (sizeHistogramMode == SizeHistogramType.Small)
                {
                    sizeSliderVal = 0;
                    SizeTo = 50;
                    SizeFrom = 0;
                    RaisePropertyChanged("Slider_Size");
                    DrawDefectSizeGraph();
                }
            }
        }
        public bool Check_SizeHistogramMedium
        {
            get
            {
                return sizeHistogramMode == SizeHistogramType.Medium;
            }
            set
            {
                sizeHistogramMode = value ? SizeHistogramType.Medium : sizeHistogramMode;

                if (sizeHistogramMode == SizeHistogramType.Medium)
                {
                    sizeSliderVal = 0;
                    SizeTo = 50 + Slider_Size;
                    SizeFrom = 0 + Slider_Size;
                    RaisePropertyChanged("Slider_Size");
                    DrawDefectSizeGraph();
                }
            }
        }
        public bool Check_SizeHistogramLarge
        {
            get
            {
                return sizeHistogramMode == SizeHistogramType.Large;
            }
            set
            {
                sizeHistogramMode = value ? SizeHistogramType.Large : sizeHistogramMode;

                if (sizeHistogramMode == SizeHistogramType.Large)
                {
                    sizeSliderVal = 0;
                    SizeTo = 50 + Slider_Size;
                    SizeFrom = 0 + Slider_Size;
                    RaisePropertyChanged("Slider_Size");
                    DrawDefectSizeGraph();
                }
            }
        }

        // Golden Image Trend Tab
        private List<byte[]> goldenImagesData = new List<byte[]>();
        private int goldenImageW = 0, goldenImageH = 0;
        private ObservableCollection<ListViewItemTemplate> goldenImageList = new ObservableCollection<ListViewItemTemplate>();
        public ObservableCollection<ListViewItemTemplate> GoldenImageList
        {
            get
            {
                return goldenImageList;
            }
            set
            {
                SetProperty(ref goldenImageList, value);
                RaisePropertyChanged("GoldenImageList");
            }
        }

        private int listViewIdx;
        public int ListViewIdx
        {
            get
            {
                return listViewIdx;
            }
            set
            {
                SetProperty(ref listViewIdx, value);
                GoldenImage = GoldenImageList[listViewIdx].GoldenImgData;
            }
        }
        private BitmapSource goldenImage;
        public BitmapSource GoldenImage
        {
            get
            {
                return goldenImage;
            }
            set
            {
                SetProperty(ref goldenImage, value);
            }
        }

        bool checkedStartDatetime = false;

        public bool CheckedStartDatetime
        {
            get { return checkedStartDatetime; }
            set
            {
                checkedStartDatetime = value;
            }
        }

        bool checkedEndDatetime = false;
        public bool CheckedEndDatetime
        {
            get { return checkedEndDatetime; }
            set
            {
                checkedEndDatetime = value;
            }
        }

        public DateTime SelectedStartDate
        {
            get
            {
                return _selectedStartDate;
            }
            set
            {
                //_selectedStartDate = value;
                SetProperty(ref _selectedStartDate, value);
                RaisePropertyChanged("SelectedStartDate");
            }
        }

        public DateTime SelectedEndDate
        {
            get
            {
                return _selectedEndDate;
            }
            set
            {
                SetProperty(ref _selectedEndDate, value);
                RaisePropertyChanged("SelectedEndDate");
            }
        }

        #endregion

        #region DataTypeEnum
        private GVHistogramType gvHistogramMode = GVHistogramType.Dark;
        private enum GVHistogramType
        {
            All,
            Dark,
            Bright,
        }
        private SizeHistogramType sizeHistogramMode = SizeHistogramType.Small;
        private enum SizeHistogramType
        {
            All,
            Small,
            Medium,
            Large,
        }
        #endregion

        public void DisplayDefectImage(string sInspectionID, string sDefectImageName)
        {
            string sDefectimagePath = @"D:\DefectImage";
            sDefectimagePath = Path.Combine(sDefectimagePath, sInspectionID, sDefectImageName);
            if (File.Exists(sDefectimagePath))
            {
                Bitmap defectImage = (Bitmap)Bitmap.FromFile(sDefectimagePath);
                p_DefectImage = ImageHelper.GetBitmapSourceFromBitmap(defectImage);
            }
            else
                p_DefectImage = null;
        }
        private void DisplaySelectedFrontDefect(DataRowView selectedRow)
        {
            if (m_ReviewDefectlist == null)
                return;

            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            double relX = (double)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[6].Name);
            double relY = (double)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[7].Name);

            m_DefectView.DisplaySelectedFrontDefect(m_ReviewDefectlist.Count, relX, relY);
        }
        private void DisplaySelectedBackDefect(DataRowView selectedRow)
        {
            if (m_ReviewDefectlist == null)
                return;

            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            double relX = (double)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[6].Name);
            double relY = (double)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[7].Name);

            m_DefectView.DisplaySelectedBackDefect(m_ReviewDefectlist.Count, relX, relY);
        }
        private void DisplayEdgeSelectedDefect(DataRowView selectedRow)
		{
            if (m_ReviewDefectlist == null)
                return;

            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            int index = (int)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[0].Name);
            //double absY = (double)GetDataGridItem(Defect_Datatable, selectedRow, defectFieldInfos[9].Name);
            Double absY = 0;
            double theta = CalculateEdgeDefectTheta(absY);

            m_DefectView.DisplaySelectedEdgeDefect(m_ReviewDefectlist.Count, index, theta);
        }

        public void DisplayDefectData(string sInspectionID)
        {
            SearchDefectData(sInspectionID);            // Draw Defect Wafer Map
            m_ReviewDefectlist = GetDefectFromDataTable(Defect_Datatable);

            m_DefectView.Clear();
            m_DefectView.DrawWaferMap();
            ClassifyDefect();    // To-do edge 전용으로만 보이니까 추후에 구조 수정필요

            DrawDefectSizeGraph();              // Draw Defect Size Distribution Histogram
            DrawDefectGVGraph();                // Draw Defect GV Distribution Histogram

            GoldenImageList.Clear();
        }
        private void ClassifyDefect()
        {
            //m_DefectView.tbRcpName.Text = 
            foreach (Defect defect in m_ReviewDefectlist)
            {
                // 대충 코드는 이런식으로 분류를 하면 되지않을까...
                //if (defect.m_nDefectCode / 10000 == 1)  // Frontside
                {
                    m_DefectView.AddFrontDefect(defect.m_fRelX, defect.m_fRelY);
                }
                //else if (defect.m_nDefectCode / 10000 == 2)  // Backside
                {
                    //m_DefectView.AddBackDefect(defect.m_fRelX, defect.m_fRelY);
                }
                //else if (defect.m_nDefectCode / 10000 == 3) // Edge
                {
                    //double theta = CalculateEdgeDefectTheta(defect.m_fAbsY);
                    //m_DefectView.AddEdgeDefect(theta);
                }
            }
        }

        private double CalculateEdgeDefectTheta(double absY)
		{
            int nNotch = 0;
            double nTheta = 0;

            if (absY > nNotch)
                nTheta = (absY - nNotch) / 540000 * 360;
            if (absY < nNotch)
                nTheta = (nNotch - absY) / 540000 * 360;

            return nTheta;
        }

        public void DisplayFrontsideDefectWaferMap()
        {

        }
        private void Defect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            selected.Fill = Brushes.Blue;
        }

        public void SearchDefectData(string sInspectionID)
        {
            //Defect 갱신
            string sDefect = "defect";
            pDefect_Datatable = DatabaseManager.Instance.SelectTablewithInspectionID(sDefect, sInspectionID);
        }

        public void SearchLotinfoData()
        {
            // Lotinfo 갱신
            string sLotInfo = "lotinfo";

            // Add DateTime Filter
            if (CheckedStartDatetime && CheckedEndDatetime)
            {
                pLotinfo_Datatable = DatabaseManager.Instance.SelectTableDatetime(sLotInfo, SelectedStartDate.ToString("yyyy-MM-dd"), SelectedEndDate.ToString("yyyy-MM-dd"));
            }
            else if (CheckedStartDatetime && !CheckedEndDatetime)
            {
                pLotinfo_Datatable = DatabaseManager.Instance.SelectTableDatetime(sLotInfo, SelectedStartDate.ToString("yyyy-MM-dd"), null);
            }
            else if (!CheckedStartDatetime && CheckedEndDatetime)
            {
                pLotinfo_Datatable = DatabaseManager.Instance.SelectTableDatetime(sLotInfo, null, SelectedEndDate.ToString("yyyy-MM-dd"));
            }
            // Add Wafer ID Filter
            // Add Recipe Name Filter
            else
            {
                pLotinfo_Datatable = DatabaseManager.Instance.SelectTable(sLotInfo);
            }
        }
        public void LoadGoldenImage()
        {
            GoldenImageList.Clear();

            if (recipe.RecipeFolderPath.Length == 0)
                return;

            string imgPath = recipe.RecipeFolderPath + @"RefImageHistory\";
            DirectoryInfo di = new DirectoryInfo(recipe.RecipeFolderPath + @"RefImageHistory");
            if (di.Exists == false)
                return;
           
            List<string> imgNames =  Directory.GetFiles(imgPath, "*.bmp", SearchOption.AllDirectories).ToList();

            foreach (string path in imgNames)
            {
                unsafe {
                    fixed (int* w = &goldenImageW, h = &goldenImageH)
                        goldenImagesData.Add(Tools.LoadBitmapToRawdata(path, w, h));

                    ListViewItemTemplate temp = new ListViewItemTemplate();

                    temp.GoldenImgData = new BitmapImage(new (path));
                    temp.Title = path.Substring(imgPath.Length, path.Length - imgPath.Length - 4); // 끝에 .bmp 제거

                    GoldenImageList.Add(temp);
                }
            }
        }

        public void GoldenImageTrend()
        {
            List<byte[]> goldenImg = new List<byte[]>();

            if (goldenImagesData.Count == 0)
                return;

            byte[] dstImg = new byte[goldenImagesData[0].Length];
            CLR_IP.Cpp_GoldenImageReview(goldenImagesData.ToArray(), dstImg, goldenImagesData.Count, goldenImageW, goldenImageH);

            GoldenImage = Tools.ConvertBitmapToSource(Tools.CovertArrayToBitmap(dstImg, goldenImageW, goldenImageH, 3));

        }
        public void SaveTrendImg()
        {
            int w = 2040;
            int h = 1080;

            byte[] rawData = new byte[w*h];

            Tools.LoadBitmapToRawdata(@"D:\Images\AOP 포장기\VRSImage_2.bmp", rawData, w, h, 1);
            
            //********** int CalcTapeThickness(byte[] rawData, int w, int h) **********//
            int measurementAreaW = w / 8;
            int measurementAreaH = h;

            bool[] isTapeArea = Enumerable.Repeat<bool>(false, h).ToArray<bool>();

            Parallel.For(0, measurementAreaH, r =>
            {
                int lineSum = 0;
                for (int c = w / 2 - w / 16; c < w / 2 + w / 16; c++)
                {
                    lineSum += rawData[r * w + c];
                }
                lineSum /= measurementAreaW;

                isTapeArea[r] = (lineSum < 128) ? true: false;
            });

            bool startCalc = false;
            int tapeThickness = 0;
            for(int r = measurementAreaH - 10; r > 10; r--)
            {
                if (!startCalc)
                {
                    if (isTapeArea[r])
                    {
                        startCalc = true;
                        for (int i = 0; i < w; i++)
                            rawData[r * w + i] = 128;
                    }
                }
                else
                {
                    if (!isTapeArea[r])
                    {
                        for (int i = 0; i < w; i++)
                            rawData[r * w + i] = 128;
                        break;
                    }
                   
                    tapeThickness++;   
                }  
            }
            for (int c = w / 2 - w / 16; c < w / 2 + w / 16; c++)
                rawData[1000 * w + c] = 128;

            //********** return tapeThickness **********//
            Tools.SaveRawdataToBitmap(@"D:\TapeTicknessTest.bmp", rawData, w, h, 1);

            float remainRatio = (float)(tapeThickness / 500.0 * 100);
        }
        public object GetDataGridItem(DataTable table, DataRow datarow, string sColumnName)
        {
            object result;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == sColumnName)
                {
                    result = datarow.ItemArray[i];
                    return result;
                }
            }
            return null;
        }

        public List<Defect> GetDefectFromDataTable(DataTable table)
        {
            List<Defect> defects = new List<Defect>();

            int nDefectIndex = 0;
            string sInpectionID = "";
            int nDefectCode = 0;
            double fSize = 0;
            double fWidth = 0;
            double fHeight = 0;
            double fRelX = 0;
            double fRelY = 0;
            double fAbsX = 0;
            double fAbsY = 0;
            double fGV = 0;
            int nChipIndexX = 0;
            int nChipIndexY = 0;

            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (DataRow dataRow in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (table.Columns[i].ColumnName == defectFieldInfos[0].Name) nDefectIndex = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[1].Name) sInpectionID = (string)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[2].Name) nDefectCode = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[3].Name) fSize = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[4].Name) fWidth = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[5].Name) fHeight = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[6].Name) fRelX = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[7].Name) fRelY = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[8].Name) fAbsX = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[9].Name) fAbsY = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[10].Name) fGV = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[11].Name) nChipIndexX = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == defectFieldInfos[12].Name) nChipIndexY = (int)dataRow.ItemArray[i];
                }

                Defect defect = new Defect(sInpectionID
                                            , nDefectCode
                                            , (float)fSize, (float)fGV
                                            , (float)fWidth, (float)fHeight
                                            , (float)fRelX, (float)fRelY
                                            , (float)fAbsX, (float)fAbsY
                                            , nChipIndexX, nChipIndexY);
                defect.SetDefectIndex(nDefectIndex);
                defects.Add(defect);
            }
            return defects;
        }

        public object GetDataGridItem(DataTable table, DataRowView selectedRow, string sColumnName)
        {
            object result;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == sColumnName)
                {
                    result = selectedRow.Row.ItemArray[i];
                    return result;
                }
            }
            return null;
        }
        private void DrawDefectGVGraph()
        {
            if (DefectGVHistogram == null)
            {
                DefectGVHistogram = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = " Defect Num",
                        Values = new ChartValues<int> {},
                        Stroke = Brushes.Red,
                        Fill = Brushes.Red,

                    }
                };
            }
            else
            {
                DefectGVHistogram[0].Values.Clear();
            }
            DataRow[] foundRows;
            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            string expression = defectFieldInfos[10].Name + " >= 0"; // m_fGV
            string sortOrder = defectFieldInfos[10].Name + " ASC"; // m_fGV
            foundRows = pDefect_Datatable.Select(expression, sortOrder);

            if (foundRows.Length == 0)
                return;

            int binSz = 128;
            if (gvHistogramMode != GVHistogramType.All)
                binSz = 128 / 2;

            int[] GVHistogram = new int[binSz];
            GVHistogram = Enumerable.Repeat<int>(0, binSz).ToArray<int>();

            if (gvHistogramMode == GVHistogramType.All)
            {
                foreach (DataRow table in foundRows)
                {
                    double gv = (double)(int)table[11];
                    GVHistogram[(int)gv/2]++;
                }
            }
            else
            {
                foreach (DataRow table in foundRows)
                {
                    double gv = (double)(int)table[11];
                    if (gvHistogramMode == GVHistogramType.Dark)
                    {
                        if ((int)gv / 2 < binSz)
                        {
                            GVHistogram[(int)gv / 2]++;
                        }
                    }
                    else
                        if ((int)gv / 2 >= binSz)
                            GVHistogram[((int)gv - 128) / 2]++;
                }
            }

            GVYMaxVal = GVHistogram.Max() + GVHistogram.Max() / 10 + 1;

            DefectGVHistogram[0].Values.AddRange(((IEnumerable)GVHistogram).Cast<object>());

            GVXLabel = new string[binSz];

            int yLabel = (gvHistogramMode == GVHistogramType.Bright) ? 128 : 0;
            for (int i = yLabel; i < binSz; i++)
                GVXLabel[i - yLabel] = (yLabel + (i - yLabel) * 2).ToString() + "~" + (yLabel + (i - yLabel) * 2 + 1).ToString();

            GVYLabel = value => value.ToString("N");
        }
        private void DrawDefectSizeGraph()
        {
            if (DefectSizeHistogram == null)
            {
                DefectSizeHistogram = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = " Defect Num",
                        Values = new ChartValues<int> {},
                        Stroke = Brushes.Green,
                        Fill = Brushes.Green,
                    }
                };
            }
            else
            {
                DefectSizeHistogram[0].Values.Clear();
            }
            DataRow[] foundRows;
            FieldInfo[] defectFieldInfos = null;
            Type defectType = typeof(Defect);
            defectFieldInfos = defectType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            string expression = defectFieldInfos[3].Name + " >= 0"; // m_fSize
            string sortOrder = defectFieldInfos[3].Name + " ASC"; // m_fSize
            foundRows = pDefect_Datatable.Select(expression, sortOrder);

            if (foundRows.Length == 0)
                return;

            int maxSz = 2001;
            int minSz = 1;
            int mergeBin = 10;

            switch (sizeHistogramMode)
            {
                case SizeHistogramType.All:
                    break;
                case SizeHistogramType.Small:
                    minSz = 1;
                    maxSz = 101;
                    mergeBin = 2;
                    break;
                case SizeHistogramType.Medium:
                    minSz = 101;
                    maxSz = 501;
                    mergeBin = 8;
                    break;
                case SizeHistogramType.Large:
                    minSz = 501;
                    maxSz = 3001;
                    mergeBin = 50;
                    break;
                default:
                    break;
            }
            int binCount = (maxSz - minSz) / mergeBin;

            int[] SzHistogram = new int[binCount];
            SzHistogram = Enumerable.Repeat<int>(0, binCount).ToArray<int>();

            foreach (DataRow table in foundRows)
            {
                if ((int)Math.Round((double)table[4]) > minSz && (int)Math.Round((double)table[4] - minSz) / mergeBin < binCount)
                    SzHistogram[(int)Math.Round((double)table[4] - minSz) / mergeBin]++;
            }

            SizeYMaxVal = SzHistogram.Max() + SzHistogram.Max() / 10 + 1;
            
            DefectSizeHistogram[0].Values.AddRange(((IEnumerable)SzHistogram).Cast<object>());

            SizeXLabel = new string[binCount];
            for (int i = 1; i <= binCount; i++)
                SizeXLabel[i - 1] = ((i - 1) * mergeBin + minSz).ToString() + "~" + ((i - 1) * mergeBin + minSz + mergeBin - 1).ToString();

            SizeYLabel = value => value.ToString("N");
        }
    }
}

public class ListViewItemTemplate
{
    private string _Title;
    public string Title
    {
        get { return this._Title; }
        set { this._Title = value; }
    }

    private BitmapImage _GoldenImgData;
    public BitmapImage GoldenImgData
    {
        get { return this._GoldenImgData; }
        set { this._GoldenImgData = value; }
    }
}