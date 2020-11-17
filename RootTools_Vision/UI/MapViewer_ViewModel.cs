using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RootTools_Vision
{
    public class MapViewer_ViewModel : ObservableObject
    {
        private int rowCount;
        public int RowCount
        {
            get { return this.rowCount; }
            set 
            {
                //this.rowCount = value;
                SetProperty(ref rowCount, value);
                //OnPropertyChanged("RowCount");
            }
        }

        private int columnCount;
        public int ColumnCount
        {
            get { return this.columnCount; }
            set
            {
                //this.columnCount = value;
                SetProperty(ref columnCount, value);
                //OnPropertyChanged("ColumnCount");
            }
        }

        private Point mapSize;

        public Point MapSize
        {
            get { return mapSize; }
            set 
            { 
                //mapSize = value;
                SetProperty(ref mapSize, value);
                //OnPropertyChanged("MapSize");
            }
        }


        //private ObservableCollection<UIElement> cellitems;
        //public ObservableCollection<UIElement> CellItems
        //{
        //    get { return this.cellitems; }
        //    set 
        //    {
        //        this.cellitems = value;
        //        OnPropertyChanged("CellItems");
        //    }
        //}

        private ObservableCollection<UIElement> cellitems = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> CellItems
        {
            get { return this.cellitems; }
            set
            {
                //this.cellitems = value;
                SetProperty(ref cellitems, value);

                //OnPropertyChanged("CellItems");
            }
        }
    }
}
