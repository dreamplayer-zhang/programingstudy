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
        MainWindow m_MainWindow;
        RecipeManager m_reviewRecipeManager;
        Recipe m_reviewRecipe;
        List<ChipData> m_ListWaferMap;
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


        public Review_ViewModel(MainWindow main, Review review)
        {
            init(main);
            ReviewWaferCanvas = review.ReviewWaferCanvas;
        }
        public void init(MainWindow main = null)
        {
            m_MainWindow = main;
            m_reviewRecipeManager = new RecipeManager();
            m_reviewRecipe = m_reviewRecipeManager.GetRecipe();
            p_Element.Add(m_DefectView);
        }

        #region Command Btn
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_MainWindow.MainPanel.Children.Clear();
                    m_MainWindow.MainPanel.Children.Add(m_MainWindow.ModeUI);
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
                if(selectedRow != null)
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
                    catch(Exception ex)
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

        public void DisplayDefectData(string sInspectionID)
        {
            SearchDefectData(sInspectionID);            // Draw Defect Wafer Map
            m_ReviewDefectlist = GetDefectFromDataTable(Defect_Datatable);
            //DisplayDefectWaferMap();            // Display Defect DataGrid
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

            RecipeData_Origin originData = m_reviewRecipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
            RecipeInfo_MapData mapdata = m_reviewRecipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;

            WaferMapInfo LoadwaferMapInfo = mapdata.m_WaferMap;
            m_ListWaferMap = LoadwaferMapInfo.ListWaferMap;

            double dWaferRaius = wafer_circle.Width / (double)2;
            double dSamplingRatio = dWaferRaius / (double)originData.Backside_Radius;

            int nOriginRelx = originData.OriginX - originData.Backside_CenterX;
            int nOriginRely = originData.OriginY - originData.Backside_CenterY;

            double dPitchx = (double)originData.DiePitchX * dSamplingRatio;
            double dPitchy = (double)originData.DiePitchY * dSamplingRatio;

            // 
            //double dPitchx = (double)ReviewWaferCanvas.Width / (double)LoadwaferMapInfo.MapSizeX;
            //double dPitchy = (double)ReviewWaferCanvas.Height / (double)LoadwaferMapInfo.MapSizeY;

            double dRelx = (double)nOriginRelx * dSamplingRatio;
            double dRely = (double)nOriginRely * dSamplingRatio;

            double dCanvasWaferCenterX = ReviewWaferCanvas.Width / 2;
            double dCanvasWaferCenterY = ReviewWaferCanvas.Height / 2;


            double Left = dCanvasWaferCenterX + dRelx;
            double Top = (dCanvasWaferCenterY + dRely) - (dPitchy * (mapdata.m_WaferMap.MasterDieY + 1));
            double Right = dCanvasWaferCenterX + dRelx + dPitchx;
            double Bottom = dCanvasWaferCenterY + dRely - (dPitchy * (mapdata.m_WaferMap.MasterDieY + 1)) + dPitchy;


            foreach (Defect defect in m_ReviewDefectlist)
            {
                int i = (int)defect.nChipIndexX;
                int j = (int)defect.nCHipIndexY;


                Rectangle ellipse = new Rectangle();
                ellipse.Stroke = Brushes.Transparent;
                ellipse.Fill = Brushes.Red;
                ellipse.Width = 3;
                ellipse.Height = 3;
                ellipse.Opacity = 0.7;

                //ellipse.StrokeThickness = 0.2;
                Canvas.SetZIndex(ellipse, 99);

                //Canvas.SetLeft(ellipse, Left + (dPitchx * i) + defect.fRelX * dSamplingRatio); // 
                //Canvas.SetTop(ellipse, Top + (dPitchy * j) + defect.fRelY * dSamplingRatio);
                //Canvas.SetRight(ellipse, Right + (dPitchx * i) + (defect.fRelX + 10) * dSamplingRatio);
                //Canvas.SetBottom(ellipse, Bottom + (dPitchy * j) + (defect.fRelY + 10) * dSamplingRatio);

                Canvas.SetLeft(ellipse, dCanvasWaferCenterX + defect.fRelX * dSamplingRatio); // 
                Canvas.SetTop(ellipse, dCanvasWaferCenterY + defect.fRelY * dSamplingRatio);
               // Canvas.SetRight(ellipse, dCanvasWaferCenterX + (defect.fRelX + 10) * dSamplingRatio);
               // Canvas.SetBottom(ellipse, dCanvasWaferCenterY + (defect.fRelY + 10) * dSamplingRatio);

                ReviewWaferCanvas.Children.Add(ellipse);
                ellipse.MouseLeftButtonDown += Defect_MouseLeftButtonDown;
            }

            #region Backside ChipMap 그리기

            foreach (ChipData chipData in m_ListWaferMap)
            {
                int i = (int)chipData.MapIndex.X;
                int j = (int)chipData.MapIndex.Y;

                Rectangle crect = new Rectangle();
                crect.Width = dPitchx;
                crect.Height = dPitchy;

                Canvas.SetLeft(crect, Left + (dPitchx * i));
                Canvas.SetTop(crect, Top + (dPitchy * j));
                Canvas.SetRight(crect, Right + (dPitchx * i));
                Canvas.SetBottom(crect, Bottom + (dPitchy * j));

                // 
                if (chipData.chipinfo == ChipInfo.Normal_Chip)
                {
                    crect.Stroke = Brushes.Transparent;
                    crect.Fill = Brushes.Green;
                    chipData.chipinfo = ChipInfo.Normal_Chip;
                    crect.Opacity = 0.5;
                    crect.StrokeThickness = 0.2;
                    Canvas.SetZIndex(crect, 99);
                    // ReviewWaferCanvas.Children.Add(crect);

                    //Ellipse ellipse = new Ellipse();
                    //ellipse.Stroke = Brushes.Transparent;
                    //ellipse.Fill = Brushes.Red;
                    //ellipse.Width = 2;
                    //ellipse.Height = 2;
                    //ellipse.Opacity = 0.7;
                    //ellipse.StrokeThickness = 0.2;
                    //Canvas.SetZIndex(ellipse, 99);

                    //Canvas.SetLeft(ellipse, Left + (dPitchx * i) + 10);
                    //Canvas.SetTop(ellipse, Top + (dPitchy * j) + 10);
                    //Canvas.SetRight(ellipse, Left + (dPitchx * i) + 30);
                    //Canvas.SetBottom(ellipse, Top + (dPitchx * i) + 30);
                    //ReviewWaferCanvas.Children.Add(ellipse);
                    //ellipse.MouseLeftButtonDown += Defect_MouseLeftButtonDown;
                }
                //else
                //{
                //    //chipInfo.chipinfo = ChipInfo.No_Chip;
                //    crect.Stroke = Brushes.Transparent;
                //    crect.Fill = Brushes.DimGray;
                //    crect.Opacity = 0.7;
                //    crect.StrokeThickness = 0.2;
                //    Canvas.SetZIndex(crect, 99);
                //}


                //ReviewWaferCanvas.Children.Add(crect);
            }

            #endregion

            #region Front Chip Map 그리기

            // Front 그리기
            //if (LoadwaferMapInfo != null)
            //{
            //    //m_ListWaferMap.Clear();
            //    nMapsizeX = LoadwaferMapInfo.nMapSizeX;
            //    nMapsizeY = LoadwaferMapInfo.nMapSizeY;
            //    ListWaferMap = LoadwaferMapInfo.ListWaferMap;

            //    /////////
            //    ReviewWaferCanvas.Children.Clear(); // 초기화
            //    int waferSize = nWaferSize;
            //    int r = waferSize / 2;

            //    double dChipX = (double)ReviewWaferCanvas.Width / (double)nMapsizeX;
            //    double dChipY = (double)ReviewWaferCanvas.Height / (double)nMapsizeY;

            //    int x = 0;
            //    int y = 0;
            //    int nChip_Left = 0;
            //    int nChip_Top = 0;
            //    int nChip_Right = nMapsizeX;
            //    int nChip_Bottom = nMapsizeY;

            //    Size chipSize = new Size(dChipX, dChipY);
            //    Point originPt = new Point(0, 0); // ???

            //    foreach (ChipData chipData in ListWaferMap)
            //    {
            //        int i = (int)chipData.MapIndex.X;
            //        int j = (int)chipData.MapIndex.Y;
            //        Rectangle crect = new Rectangle();
            //        crect.Width = chipSize.Width;
            //        crect.Height = chipSize.Height;
            //        Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i));
            //        Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i) + chipSize.Width);
            //        Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j));
            //        Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j) + chipSize.Height);

            //        if (chipData.chipinfo == ChipInfo.Normal_Chip)
            //        {
            //            crect.Tag = chipData;
            //            crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
            //            crect.Stroke = Brushes.Transparent;
            //            crect.Fill = Brushes.Green;
            //            chipData.chipinfo = ChipInfo.Normal_Chip;
            //            crect.Opacity = 0.7;
            //            crect.StrokeThickness = 2;
            //            Canvas.SetZIndex(crect, 99);
            //            //m_ListWaferMap.Add(chipData);
            //        }
            //        //else
            //        //{
            //        //    //chipInfo.chipinfo = ChipInfo.No_Chip;
            //        //    crect.Tag = chipData;
            //        //    crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
            //        //    crect.Stroke = Brushes.Transparent;
            //        //    crect.Fill = Brushes.DimGray;
            //        //    crect.Opacity = 0.7;
            //        //    crect.StrokeThickness = 2;
            //        //    Canvas.SetZIndex(crect, 99);
            //        //}
            //        ReviewWaferCanvas.Children.Add(crect);
            //        //crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
            //    }
            //}


            #endregion

        }

        private void Defect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            //ChipData chipData = (ChipData)selected.Tag;
            //int stride = (int)m_MapData.PartialMapSize.Height;
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

            m_DefectView.Clear();
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
                Defect defect = new Defect(sInpectionID, nDefectCode, (float)fSize, fGV, (float)fWidth, (float)fHeight, (float)fRelX, (float)fRelY, (float)fAbsX,
                    (float)fAbsY, nChipIndexX, nCHipIndexY);
                defect.SetDefectIndex(nDefectIndex);
                defects.Add(defect);

                int nNotch = 0;
                double nTheta = 0;
                if (fAbsY > nNotch)
                    nTheta = (fAbsY - nNotch) / 540000 * 360;
                if (fAbsY < nNotch)
                    nTheta = (nNotch - fAbsY) / 540000 * 360;

                if (fSize > 150)
                {
                    count++;
                    m_DefectView.AddDefectFront(nTheta);
                }
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
