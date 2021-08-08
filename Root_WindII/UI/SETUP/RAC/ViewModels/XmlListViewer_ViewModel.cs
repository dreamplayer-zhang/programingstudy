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
    public class XmlListViewer_ViewModel : ObservableObject
    {
        #region [EVENT]
        public delegate void XmlSelectionHandler(object obj);
        public event XmlSelectionHandler OnSelectChanged;
        #endregion
        public XmlListViewer_ViewModel()
        {

        }


        #region [PROPERTY]
        private string FileRootPath = @"C:\Root\Setup\RAC";

        ObservableCollection<ListFileInfo> fileInfoList = new ObservableCollection<ListFileInfo>();
        public ObservableCollection<ListFileInfo> FileInfoList
        {
            get => fileInfoList;
            set
            {
                SetProperty(ref fileInfoList, value);
            }
        }

        string searchFileName = "";
        public string SearchFileName
        {
            get => searchFileName;
            set
            {
                SetProperty(ref searchFileName, value);
                Refresh();
            }
        }

        object selectItem;
        public object SelectedItem
        {
            get => selectItem;
            set
            {
                SetProperty(ref selectItem, value);
            }
        }
        #endregion

        #region [COMMAND]
        public ICommand CmdSelectedCellsChanged
        {
            get => new RelayCommand(() =>
            {
                try
                {
                    if (OnSelectChanged != null)
                        OnSelectChanged(SelectedItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        public ICommand CmdLoaded
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Refresh();
                });
            }
        }

        public ICommand CmdUnloaded
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        #endregion

        #region [FUNCTION]
        void Refresh()
        {
            FileInfoList.Clear();

            DirectoryInfo di = new DirectoryInfo(FileRootPath);
            if (!Directory.Exists(FileRootPath))
                di.Create();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    string fileName = file.Name.ToLower();
                    if (fileName.Contains(this.SearchFileName.ToLower()))
                    { 
                        FileInfoList.Add(new ListFileInfo(Path.GetFileNameWithoutExtension(file.Name), file.FullName, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                }
            });
        }
        #endregion

        #region [FileInfoClass]
        public class ListFileInfo 
        {
            public string FileName { get; private set; }
            public string FilePath { get; private set; }
            public string CreationTime { get; private set; }
            public string LastWriteTime { get; private set; }
            public ListFileInfo(string fileName, string filePath, string creationTime, string lastWriteTime)
            {
                FileName = fileName;
                FilePath = filePath;
                CreationTime = creationTime;
                LastWriteTime = lastWriteTime;
            }
        }
        #endregion
    }
}
