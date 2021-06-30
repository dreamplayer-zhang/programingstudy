﻿using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WindII
{

    public class MapFileListViewerItem : ObservableObject
    {
        public string MapFileName { get; set; }
        public string MapFilePath { get; set; }
    }

    public class MapFileListViewer_ViewModel : ObservableObject
    {

        #region [Properties]
        private ObservableCollection<MapFileListViewerItem> mapFileListViewerItems = new ObservableCollection<MapFileListViewerItem>();
        public ObservableCollection<MapFileListViewerItem> MapFileListViewerItems
        {
            get => this.mapFileListViewerItems;
            set
            {
                SetProperty(ref this.mapFileListViewerItems, value);
            }
        }
        private string mapFileRootPath = @"C:\Root\Setup\RAC";
        public string MapFileRootPath
        {
            get => this.mapFileRootPath;
            set
            {
                SetProperty(ref this.mapFileRootPath, value);
            }
        }

        private string searchName = "";
        public string SearchName
        {
            get => this.searchName;
            set
            {
                SetProperty(ref this.searchName, value);
                Refresh();
            }
        }
        #endregion

        /*public MapFIleListViewer_ViewModel()
        {

        }*/

        public void Refresh()
        {
            this.MapFileListViewerItems.Clear();

            DirectoryInfo di = new DirectoryInfo(MapFileRootPath);
            di.Create();

            foreach (FileInfo file in di.GetFiles())
            {
                string fileName = file.Name.ToLower();
                if (fileName.Contains(this.SearchName.ToLower()))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        this.MapFileListViewerItems.Add(new MapFileListViewerItem() { MapFileName = file.Name, MapFilePath = file.FullName });
                    });
                }
            }
        }


        #region [Command]
        //public ICommand LoadedCommand
        //{
        //    get => new RelayCommand(() =>
        //    {
        //        Refresh();
        //    });
        //}
        #endregion
    }
}
