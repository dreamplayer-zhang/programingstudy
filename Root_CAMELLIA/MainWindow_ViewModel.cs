using Root_CAMELLIA.Data;
using Root_CAMELLIA.Draw;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_CAMELLIA
{
    public class MainWindow_ViewModel : ObservableObject
    {
        public MainWindow_ViewModel(MainWindow main)
        {
            DataManager = main.DataManager;
            Init();
        }

        public void Init()
        {
            PointListItem.Columns.Add(new DataColumn("ListIndex"));
            PointListItem.Columns.Add(new DataColumn("ListX"));
            PointListItem.Columns.Add(new DataColumn("ListY"));
            PointListItem.Columns.Add(new DataColumn("ListRoute"));
        }

        public void UpdateListView()
        {
            PointListItem.Clear();
            int nCount = 0;
            int nSelCnt = DataManager.recipeDM.TeachingRD.DataSelectedPoint.Count;
            int[] MeasurementOrder = new int[nSelCnt];

            for (int i = 0; i < nSelCnt; i++)
            {
                MeasurementOrder[DataManager.recipeDM.TeachingRD.DataMeasurementRoute[i]] = i;
            }

            DataRow row;
            for (int i = 0; i < nSelCnt; i++, nCount++)
            {

                CCircle c = DataManager.recipeDM.TeachingRD.DataSelectedPoint[i];
                int nRoute = MeasurementOrder[i];
                row = PointListItem.NewRow();
                row["ListIndex"] = (nCount + 1).ToString();
                row["ListX"] = Math.Round(c.x, 3).ToString();
                row["ListY"] = Math.Round(c.y, 3).ToString();
                row["ListRoute"] = (nRoute + 1).ToString();
                PointListItem.Rows.Add(row);

            }
            PointCount = PointListItem.Rows.Count.ToString();
        }

        public DataManager DataManager { get; set; }

        private ObservableCollection<UIElement> m_MainDrawElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_MainDrawElement
        {
            get
            {
                return m_MainDrawElement;
            }
            set
            {
                m_MainDrawElement = value;
            }
        }

        public string PointCount { get; set; } = "0";

        public ObservableCollection<ShapeManager> Shapes = new ObservableCollection<ShapeManager>();
        public ObservableCollection<GeometryManager> Geometry = new ObservableCollection<GeometryManager>();

        DataTable pointListItem = new DataTable();
        public DataTable PointListItem
        {
            get
            {
                return pointListItem;
            }
            set
            {
                pointListItem = value;
                RaisePropertyChanged("PointListItem");
            }
        }



    }
}
