using RootTools;
using RootTools.Database;
using RootTools_Vision;
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

namespace Root_WIND2
{
    class Review_ViewModel : ObservableObject
    {
        Recipe recipe;
        List<Defect> m_ReviewDefectlist;

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

        public Review_ViewModel(Review review)
        {
            init();
            ReviewWaferCanvas = review.ReviewWaferCanvas;
        }
        public void init()
        {
            p_Element.Add(m_DefectView);
        }

        #region Command Btn
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UIManager.Instance.ChangUIMode();
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
        #endregion


        #region GET / SET

        private Canvas reviewcanvas;
        public Canvas ReviewWaferCanvas { get => reviewcanvas; set => reviewcanvas = value; }

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
        public Recipe Recipe { get => recipe; set => recipe = value; }

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

                    try
                    {
                        string sInspectionID = (string)GetDataGridItem(lotinfo_Datatable, selectedRow, "INSPECTIONID");
                        string sReicpePath = @"C:\Root\Recipe";
                        string sRecipeID = (string)GetDataGridItem(lotinfo_Datatable, selectedRow, "RECIPEID");
                        string sReicpeFileName = sRecipeID + ".rcp";
                        //m_reviewRecipe.LoadRecipeInfo(Path.Combine(sReicpePath, sRecipeID, sReicpeFileName));
                        //m_reviewRecipe.LoadRecipeData(Path.Combine(sReicpePath, sRecipeID, sReicpeFileName));

                        DisplayDefectData(sInspectionID);
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
                    // BMP Open Event
                    int nIndex = (int)GetDataGridItem(Defect_Datatable, selectedRow, "nDefectIndex");
                    string sInspectionID = (string)GetDataGridItem(Defect_Datatable, selectedRow, "sInspectionID");
                    string sFileName = nIndex.ToString() + ".bmp";
                    DisplayDefectImage(sInspectionID, sFileName);
                    DisplaySelectedDefect(selectedRow); // To-do edge 전용으로만 보이니까 추후에 구조 수정필요
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
                RaisePropertyChanged("GVHistogramAnimation");
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
                    RaisePropertyChanged("SizeHistogramAnimation");
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
                    RaisePropertyChanged("SizeHistogramAnimation");
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
                    RaisePropertyChanged("SizeHistogramAnimation");
                    RaisePropertyChanged("Slider_Size");
                    DrawDefectSizeGraph();
                }
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

        private void DisplaySelectedDefect(DataRowView selectedRow)
		{
            if (m_ReviewDefectlist == null)
                return;

            int index = (int)GetDataGridItem(Defect_Datatable, selectedRow, "nDefectIndex");
            double absY = (double)GetDataGridItem(Defect_Datatable, selectedRow, "fAbsY");
            double theta = CalculateEdgeDefectTheta(absY);

            m_DefectView.DisplaySelectedDefect(m_ReviewDefectlist.Count, index, theta);
        }

        public void DisplayDefectData(string sInspectionID)
        {
            SearchDefectData(sInspectionID);            // Draw Defect Wafer Map
            m_ReviewDefectlist = GetDefectFromDataTable(Defect_Datatable);
            DisplayEdgeDefectWaferMap();    // To-do edge 전용으로만 보이니까 추후에 구조 수정필요
            DrawDefectSizeGraph();              // Draw Defect Size Distribution Histogram
            DrawDefectGVGraph();                // Draw Defect GV Distribution Histogram
            //DisplayDefectWaferMap();            // Display Defect DataGrid

        }

        private void DisplayEdgeDefectWaferMap()
		{
            m_DefectView.Clear();
            foreach (Defect defect in m_ReviewDefectlist)
            {
                double theta = CalculateEdgeDefectTheta(defect.m_fAbsY);
                m_DefectView.AddDefectFront(theta);
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

        public void DisplayDefectWaferMap()
        {
            ReviewWaferCanvas.Children.Clear();
            int nWaferSize = 300;

            float ratio_wafer_to_canvas_x = (float)ReviewWaferCanvas.Width / nWaferSize;
            float ratio_wafer_to_canvas_y = (float)ReviewWaferCanvas.Height / nWaferSize;

            //Fill = "Gainsboro"
            Ellipse wafer_circle = new Ellipse();
            wafer_circle.Width = nWaferSize * ratio_wafer_to_canvas_x;
            wafer_circle.Height = nWaferSize * ratio_wafer_to_canvas_y;
            Canvas.SetLeft(wafer_circle, 0);
            Canvas.SetRight(wafer_circle, nWaferSize * ratio_wafer_to_canvas_x);
            Canvas.SetTop(wafer_circle, 0);
            Canvas.SetBottom(wafer_circle, nWaferSize * ratio_wafer_to_canvas_y);

            wafer_circle.Stroke = Brushes.Black;
            wafer_circle.Fill = Brushes.Silver;
            wafer_circle.StrokeThickness = 0.5;
            ReviewWaferCanvas.Children.Add(wafer_circle);

            BacksideRecipe backsideRecipe = Recipe.GetRecipe<BacksideRecipe>();
            RecipeType_WaferMap mapdata = Recipe.WaferMap;
            
            double dWaferRaius = wafer_circle.Width / (double)2;
            double dSamplingRatio = dWaferRaius / (double)backsideRecipe.Radius;

            int nOriginRelx = backsideRecipe.OriginX - backsideRecipe.CenterX;
            int nOriginRely = backsideRecipe.OriginY - backsideRecipe.CenterY;

            double dPitchx = (double)backsideRecipe.DiePitchX * dSamplingRatio;
            double dPitchy = (double)backsideRecipe.DiePitchY * dSamplingRatio;

            // 
            //double dPitchx = (double)ReviewWaferCanvas.Width / (double)LoadwaferMapInfo.MapSizeX;
            //double dPitchy = (double)ReviewWaferCanvas.Height / (double)LoadwaferMapInfo.MapSizeY;

            double dRelx = (double)nOriginRelx * dSamplingRatio;
            double dRely = (double)nOriginRely * dSamplingRatio;

            double dCanvasWaferCenterX = ReviewWaferCanvas.Width / 2;
            double dCanvasWaferCenterY = ReviewWaferCanvas.Height / 2;


            double Left = dCanvasWaferCenterX + dRelx;
            double Top = (dCanvasWaferCenterY + dRely) - (dPitchy * (mapdata.MasterDieY + 1));
            double Right = dCanvasWaferCenterX + dRelx + dPitchx;
            double Bottom = dCanvasWaferCenterY + dRely - (dPitchy * (mapdata.MasterDieY + 1)) + dPitchy;


            foreach (Defect defect in m_ReviewDefectlist)
            {
                int i = (int)defect.m_nChipIndexX;
                int j = (int)defect.m_nCHipIndexY;


                Rectangle ellipse = new Rectangle();
                ellipse.Stroke = Brushes.Transparent;
                ellipse.Fill = Brushes.Red;
                ellipse.Width = 3;
                ellipse.Height = 3;
                ellipse.Opacity = 0.7;

                Canvas.SetZIndex(ellipse, 99);

                Canvas.SetLeft(ellipse, dCanvasWaferCenterX + defect.m_fRelX * dSamplingRatio); // 
                Canvas.SetTop(ellipse, dCanvasWaferCenterY + defect.m_fRelY * dSamplingRatio);


                ReviewWaferCanvas.Children.Add(ellipse);
                ellipse.MouseLeftButtonDown += Defect_MouseLeftButtonDown;
            }

            #region Backside ChipMap 그리기

            for(int y = 0; y < this.Recipe.WaferMap.MapSizeY; y++)
            {
                for (int x = 0; x < this.Recipe.WaferMap.MapSizeX; x++)
                {
                    Rectangle crect = new Rectangle();
                    crect.Width = dPitchx;
                    crect.Height = dPitchy;

                    Canvas.SetLeft(crect, Left + (dPitchx * x));
                    Canvas.SetTop(crect, Top + (dPitchy * y));
                    Canvas.SetRight(crect, Right + (dPitchx * x));
                    Canvas.SetBottom(crect, Bottom + (dPitchy * y));

                    // 

                    if(this.Recipe.WaferMap.GetChipType(x, y) == CHIP_TYPE.NORMAL)
                    {
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.Green;
                        crect.Opacity = 0.5;
                        crect.StrokeThickness = 0.2;
                        Canvas.SetZIndex(crect, 99);
                    }
                    else
                    {
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.DimGray;
                        crect.Opacity = 0.5;
                        crect.StrokeThickness = 0.2;
                        Canvas.SetZIndex(crect, 99);
                    }
                }
            }

            #endregion
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
            pLotinfo_Datatable = DatabaseManager.Instance.SelectTable(sLotInfo);
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
            int fGV = 0;
            int nChipIndexX = 0;
            int nCHipIndexY = 0;

            int count = 0;
            foreach (DataRow dataRow in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (table.Columns[i].ColumnName == "nDefectIndex") nDefectIndex = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "sInspectionID") sInpectionID = (string)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "nDefectCode") nDefectCode = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fSize") fSize = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fWidth") fWidth = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fHeight") fHeight = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fRelX") fRelX = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fRelY") fRelY = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fAbsX") fAbsX = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fAbsY") fAbsY = (double)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "fGV") fGV = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "nChipIndexX") nChipIndexX = (int)dataRow.ItemArray[i];
                    else if (table.Columns[i].ColumnName == "nCHipIndexY") nCHipIndexY = (int)dataRow.ItemArray[i];
                }

                Defect defect = new Defect(sInpectionID
                                            , nDefectCode
                                            , (float)fSize, fGV
                                            , (float)fWidth, (float)fHeight
                                            , (float)fRelX, (float)fRelY
                                            , (float)fAbsX, (float)fAbsY
                                            , nChipIndexX, nCHipIndexY);
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
            string expression = "fGV >= 0";
            string sortOrder = "fGV ASC";
            foundRows = pDefect_Datatable.Select(expression, sortOrder);

            if (foundRows.Length == 0)
                return;

            int binSz = 256;
            if (gvHistogramMode != GVHistogramType.All)
                binSz = 128 / 2;

            int[] GVHistogram = new int[binSz];
            GVHistogram = Enumerable.Repeat<int>(0, binSz).ToArray<int>();

            if (gvHistogramMode == GVHistogramType.All)
            {
                foreach (DataRow table in foundRows)
                    GVHistogram[(int)table[11]]++;
            }
            else
            {
                foreach (DataRow table in foundRows)
                {
                    if (gvHistogramMode == GVHistogramType.Dark)
                        if ((int)table[11] / 2 < binSz)
                            GVHistogram[(int)table[11] / 2]++;
                        else
                        if ((int)table[11] / 2 >= binSz)
                            GVHistogram[((int)table[11] - 128) / 2]++;
                }
            }

            GVYMaxVal = GVHistogram.Max() + GVHistogram.Max() / 10 + 1;

            for (int i = 0; i < binSz; i++)
                DefectGVHistogram[0].Values.Add(GVHistogram[i]);

            GVXLabel = new string[binSz];

            if (gvHistogramMode != GVHistogramType.All)
            {
                int yLabel = (gvHistogramMode == GVHistogramType.Bright) ? 128 : 0;
                for (int i = yLabel; i < binSz; i++)
                    GVXLabel[i - yLabel] = (yLabel + (i - yLabel) * 2).ToString() + "~" + (yLabel + (i - yLabel) * 2 + 1).ToString();
            }
            else
            {
                int yLabel = (gvHistogramMode == GVHistogramType.Bright) ? 128 : 0;
                for (int i = yLabel; i < binSz; i++)
                    GVXLabel[i - yLabel] = i.ToString();
            }

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
            string expression = "fSize >= 0";
            string sortOrder = "fSize ASC";
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

            for (int i = 0; i < binCount; i++)
                DefectSizeHistogram[0].Values.Add(SzHistogram[i]);

            SizeXLabel = new string[binCount];
            for (int i = 1; i <= binCount; i++)
            {
                SizeXLabel[i - 1] = ((i - 1) * mergeBin + minSz).ToString() + "~" + ((i - 1) * mergeBin + minSz + mergeBin - 1).ToString();
            }

            SizeYLabel = value => value.ToString("N");
        }
    }
}
