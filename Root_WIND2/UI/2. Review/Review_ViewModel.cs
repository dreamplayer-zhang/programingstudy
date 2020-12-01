using RootTools;
using RootTools.Database;
using RootTools_Vision;
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
    }
}
