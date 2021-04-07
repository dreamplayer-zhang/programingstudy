using Root_CAMELLIA.Data;
using Root_CAMELLIA.Draw;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Review_ViewModel : ObservableObject, IDialogRequestClose
    {
        private MainWindow_ViewModel mainWindow_ViewModel;

        Explorer_ViewModel m_summary = new Explorer_ViewModel(InitPath : @"C:\Camellia2\Summary");
        public Explorer_ViewModel p_summary
        {
            get
            {
                return m_summary;
            }
            set
            {
                SetProperty(ref m_summary, value);
            }
        }

        Explorer_ViewModel m_history = new Explorer_ViewModel(InitPath: @"C:\Camellia2\History");
        public Explorer_ViewModel p_history
        {
            get
            {
                return m_history;
            }
            set
            {
                SetProperty(ref m_history, value);
            }
        }



        public Dlg_Review_ViewModel(MainWindow_ViewModel mainWindow_ViewModel)
        {
            this.mainWindow_ViewModel = mainWindow_ViewModel;

            m_summary.ItemClicked += ItemClick;
            m_summary.ItemRefresh += ItemRefresh;
            m_history.ItemClicked += ItemClick;
            m_history.ItemRefresh += ItemRefresh;
            InitExplorer();

            InitDataGrid();
        }

        string m_currentPath = @"C:\Camellia2\Summary\";
        public string p_currentPath
        {
            get
            {
                return m_currentPath;
            }
            set
            {
                SetProperty(ref m_currentPath, value);
            }
        }

        private void ItemClick(object sender, EventArgs e)
        {
            string path = (string)sender;
            if(p_currentPath == path)
            {
                return;
            }
            p_currentPath = path;
            ReadCSV(path);
        }

        private void ItemRefresh(object sender, EventArgs e)
        {
            ClearData();
        }

        private void InitDataGrid()
        {
            PointListItem.Columns.Add(new DataColumn("ListSite"));
            PointListItem.Columns.Add(new DataColumn("ListX"));
            PointListItem.Columns.Add(new DataColumn("ListY"));
        }

        public DataTable PointListItem
        {
            get
            {
                return _PointListItem;
            }
            set
            {
                SetProperty(ref _PointListItem, value);
            }
        }
        private DataTable _PointListItem = new DataTable();

        
        public List<string> p_reflectanceList
        {
            get
            {
                return m_reflectanceList;
            }
            set
            {
                SetProperty(ref m_reflectanceList, value);
            }
        }
        private List<string> m_reflectanceList = new List<string>();

        public List<string> p_transmittanceList
        {
            get
            {
                return m_transmittanceList;
            }
            set
            {
                SetProperty(ref m_transmittanceList, value);
            }
        }
        private List<string> m_transmittanceList = new List<string>();
        List<Point> pointList = new List<Point>();

        private void ReadCSV(string path)
        {
            pointList.Clear();
            try
            {
                var reader = new StreamReader(File.OpenRead(path));
               
                List<int> dataGridList = new List<int>();
                List<string> reflectanceList = new List<string>();
                List<string> transmittanceList = new List<string>();

                int idx = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    double x = 0, y = 0;
                    if (double.TryParse(values[0], out x) && double.TryParse(values[1], out y)){
                        pointList.Add(new Point(x, y));
                    }
                    //listB.Add(values[1]);
                    //listC.Add(values[4]);
                    int site = 0;
                    if(int.TryParse(values[4], out site))
                    {
                        dataGridList.Add(site);
                    }

                    if(idx == 0)
                    {
                        for (int i = 0; i < values.Length; i++) {
                            if (values[i].Contains("R_"))
                            {
                                var val = values[i].Split('_');
                                reflectanceList.Add(val[1]);
                            }
                            else if(values[i].Contains("T_"))
                            {
                                var val = values[i].Split('_');
                                transmittanceList.Add(val[1]);
                            }
                        }
                    }
                    idx++;
                }
                foreach (var xy in pointList)
                {
                    Console.WriteLine(xy.X + " ," + xy.Y);
                }
                foreach(var site in dataGridList){
                    Console.WriteLine("Site : " + site);
                }

                StageDrawPoint(pointList);
                SetDataGrid(pointList, dataGridList);
                p_reflectanceList = reflectanceList;
                p_transmittanceList = transmittanceList;
                //{
                //    Console.WriteLine(coloumn1);
                //}
                //foreach (var coloumn2 in listB)
                //{
                //    Console.WriteLine(coloumn2);
                //}
                //foreach (var coloumn3 in listC)
                //{
                //    Console.WriteLine(coloumn3);
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ClearData();
            }
            
        }

        private void ClearData()
        {
            PointListItem.Clear();
            listSelectedPoint.Clear();
            p_PointElement.Clear();
            p_reflectanceList = new List<string>();
            p_transmittanceList = new List<string>();
            p_SelectedItemIndex = -1;
        }

        private void SetDataGrid(List<Point> pt, List<int> site)
        {
            PointListItem.Clear();
            DataRow row;
            for(int i = 0; i < pt.Count; i++)
            {
                row = PointListItem.NewRow();
                row["ListSite"] = site[i];
                row["ListX"] = Math.Round(pt[i].X, 3).ToString();
                row["ListY"] = Math.Round(pt[i].Y, 3).ToString();
                PointListItem.Rows.Add(row);
            }
            
        }

        private List<ShapeEllipse> listSelectedPoint = new List<ShapeEllipse>();
        private void StageDrawPoint(List<Point> pt)
        {
            listSelectedPoint.Clear();
            double RatioX = 1000 / BaseDefine.ViewSize;
            double RatioY = 1000 / BaseDefine.ViewSize;
            int CenterX = (int)(1000 * 0.5f);
            int CenterY = (int)(1000 * 0.5f);
            DrawGeometryManager drawGeometryManager = new DrawGeometryManager();
            ObservableCollection<UIElement> temp = new ObservableCollection<UIElement>();

            for (int i = 0; i < pt.Count; i++)
            {
                ShapeManager dataPoint = new ShapeEllipse(GeneralTools.GbHole);
                ShapeEllipse dataSelectedPoint = dataPoint as ShapeEllipse;
                CCircle circle = new CCircle(pt[i].X, pt[i].Y, 8,
                  8, 0, 0);
                circle.Transform(RatioX, RatioY);

                Circle c = drawGeometryManager.GetRect(circle, CenterX, CenterY);
                dataSelectedPoint.SetData(c, (int)(circle.width), (int)(circle.height), 95);
                temp.Add(dataSelectedPoint.UIElement);
                listSelectedPoint.Add(dataSelectedPoint);
            }

            p_PointElement = temp;
        }

        int m_tabIdx = 0;
        public int p_tabIdx
        {
            get
            {
                return m_tabIdx;
            }
            set
            {
                SetProperty(ref m_tabIdx, value);
                if(value == 1)
                {
                    p_IsSummary = false;
                    ClearData();
                }
                else
                {
                    p_IsSummary = true;
                }
            }
        }

        bool m_IsSummary = true;
        public bool p_IsSummary
        {
            get
            {
                return m_IsSummary;
            }
            set
            {
                SetProperty(ref m_IsSummary, value);
            }
        }

        ObservableCollection<UIElement> m_PointElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_PointElement
        {
            get
            {
                return m_PointElement;
            }
            set
            {
                SetProperty(ref m_PointElement, value);
            }
        }

        private void InitExplorer()
        {
            //p_explorer.p_treeView = new System.Windows.Controls.TreeView();
            //m_explorer.p_treeView.SelectedItemChanged += ExplorerItemDoubleClick;
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #region Command
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }

        public ICommand CmdRefresh
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(p_tabIdx == 0)
                    {
                        p_summary.RebuildTree(pIncludeFileChildren : true, InitPath: @"C:\Camellia2\Summary");
                    }
                    else
                    {
                        p_history.RebuildTree(pIncludeFileChildren: true, InitPath: @"C:\Camellia2\History");
                    }

                });
            }
        }


        #endregion

        int nMinIndex = -1;
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if(listSelectedPoint.Count <= 0)
            {
                return;
            }
            Point pt = e.GetPosition((UIElement)sender);

            double dMin = 9999;
            int nIndex = 0;


            foreach (ShapeEllipse se in listSelectedPoint)
            {
                double dDistance = GetDistance(se, new System.Windows.Point(pt.X, pt.Y));

                if (dDistance < dMin)
                {
                    dMin = dDistance;
                    nMinIndex = nIndex;
                }
                nIndex++;
            }

            foreach (ShapeEllipse se in listSelectedPoint)
            {
                if (se.Equals(listSelectedPoint[nMinIndex]))
                {
                    //bSelected = true;
                    se.SetBrush(GeneralTools.GbSelect);
                }
                else if (p_SelectedItemIndex != -1 && se.Equals(listSelectedPoint[p_SelectedItemIndex]))
                {
                    listSelectedPoint[p_SelectedItemIndex].SetBrush(GeneralTools.SelectBrush);
                }
                else
                {
                    se.SetBrush(GeneralTools.GbHole);
                }
            }
        }

        private double GetDistance(ShapeEllipse eg, Point pt)
        {
            double dResult = Math.Sqrt(Math.Pow(eg.CenterX - pt.X, 2) + Math.Pow(eg.CenterY - pt.Y, 2));

            return Math.Round(dResult, 3);
        }

        public void OnMouseLeave(object sender, MouseEventArgs e)
        {
            foreach (ShapeEllipse se in listSelectedPoint)
            {
                if (p_SelectedItemIndex != -1 && !se.Equals(listSelectedPoint[p_SelectedItemIndex]))
                {
                    se.SetBrush(GeneralTools.GbHole);
                }
                else if (p_SelectedItemIndex != -1 && se.Equals(listSelectedPoint[p_SelectedItemIndex]))
                {
                    se.SetBrush(GeneralTools.SelectBrush);
                }
            }
            nMinIndex = -1;
        }


        public int p_SelectedItemIndex
        {
            get
            {
                return m_SelectedItemIndex;
            }
            set
            {
                SetProperty(ref m_SelectedItemIndex, value);
            }
        }
        int m_SelectedItemIndex = -1;

        public void OnMouseMoveDataGrid(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                return;
            }
        }

        public void OnMouseLeftButtonUpDataGrid(object sender, MouseButtonEventArgs e)
        {
            if(p_SelectedItemIndex == -1) 
            {
                return;
            }

            foreach (ShapeEllipse se in listSelectedPoint)
            {
                se.SetBrush(GeneralTools.GbHole);
            }
            listSelectedPoint[p_SelectedItemIndex].SetBrush(GeneralTools.SelectBrush);
        }

        //public void OnSelectedCellChange(object sender, SelectedCellsChangedEventArgs e)
        //{
        //    int t = 10;
        //    foreach (ShapeEllipse se in listSelectedPoint)
        //    {
        //        se.SetBrush(GeneralTools.GbHole);
        //    }
        //    listSelectedPoint[p_SelectedItemIndex].SetBrush(GeneralTools.SelectBrush);
        //}
        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(pointList.Count <= 0 || nMinIndex == p_SelectedItemIndex)
            {
                return;
            }

            double X = pointList[nMinIndex].X;
            double Y = pointList[nMinIndex].Y;

            // Point 계산
            foreach (ShapeEllipse se in listSelectedPoint)
            {
                se.SetBrush(GeneralTools.GbHole);
            }
            listSelectedPoint[nMinIndex].SetBrush(GeneralTools.SelectBrush);
            p_SelectedItemIndex = nMinIndex;
            //if (listRealPos.Count <= 0)
            //{
            //    return;
            //}

            //if (ModuleCamellia.p_eState != ModuleBase.eState.Ready)
            //{
            //    MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
            //    return;
            //}

            //if (EnableBtn)
            //{
            //    return;
            //}

            //double centerX;
            //double centerY;
            //if (DataManager.Instance.m_waferCentering.m_ptCenter.X == 0 && DataManager.Instance.m_waferCentering.m_ptCenter.Y == 0)
            //{
            //    centerX = StageCenterPos.X;
            //    centerY = StageCenterPos.Y;
            //}
            //else // 나중에 centering 값 추가 테스트 진행 예정
            //{
            //    centerX = StageCenterPos.X;
            //    centerY = StageCenterPos.Y;
            //}

            //double x = listRealPos[nMinIndex].x;
            //double y = listRealPos[nMinIndex].y;
            //double dX = centerX - x * 10000;
            //double dY = centerY - y * 10000;
            //Thread thread = new Thread(() =>
            //{
            //    EnableBtn = false;
            //    string str;
            //    str = ModuleCamellia.p_axisXY.StartMove(new RPoint(dX, dY));
            //    if (str != "OK")
            //    {
            //        MessageBox.Show(str);
            //        return;
            //    }
            //    ModuleCamellia.p_axisXY.WaitReady();
            //    EnableBtn = true;
            //});
            //thread.Start();
            //MessageBox.Show(listRealPos[nMinIndex].x.ToString() + " " + listRealPos[nMinIndex].y.ToString());
        }
    }
}
