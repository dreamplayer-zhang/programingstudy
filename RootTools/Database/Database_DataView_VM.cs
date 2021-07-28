using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using RootTools.Database;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RootTools
{
    public delegate void SelectedCellsChangedHandler(object obj);
    public class Database_DataView_VM : ObservableObject
    {
        public event SelectedCellsChangedHandler SelectedCellsChanged;

        public ICommand SelectedCellsChangedCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedCellsChanged != null)
                    SelectedCellsChanged(selectedItem);
            });
        }

        DataTable _dataTable;
        public DataTable pDataTable 
        { 
            get => _dataTable; 
            set => SetProperty(ref _dataTable, value); 
        }

        //DataGrid _dataGrid;
        //public DataGrid pDataGrid { get => _dataGrid; set => SetProperty(ref _dataGrid, value); }
        private object selectedItem;
        public object pSelectedItem
        {
            get => selectedItem;
            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        private int _test;
        public int test
        {
            get => _test;
            set
            {
                SetProperty(ref _test, value);
            }
        }
        public Database_DataView_VM()
        {
        }

        public Database_DataView_VM(DataTable _table)
        {
            pDataTable = _table;
        }

        public void DisplayDataTable(DataTable table)
        {
            pDataTable = table;
        }
    }
}
