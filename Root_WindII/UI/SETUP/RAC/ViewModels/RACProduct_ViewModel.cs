using RootTools;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Data;
using System.IO;
using System.Windows.Input;

namespace Root_WindII
{
    public class RACProduct_ViewModel : ObservableObject
    {
        #region [Properites]
        private MapFileSelectionViewer_ViewModel mapFileSelectionViewerVM = new MapFileSelectionViewer_ViewModel();
        public MapFileSelectionViewer_ViewModel MapFileSelectionViewerVM
        {
            get => this.mapFileSelectionViewerVM;
            set
            {
                SetProperty(ref this.mapFileSelectionViewerVM, value);
            }
        }
        #endregion

        public RACProduct_ViewModel()
        {
            MapFileSelectionViewerVM.MapFileSelected += MapFileSelect;
            MapFileSelectionViewerVM.MapFileCreated += MapFileCreated;
        }

        #region [Event]
        public void MapFileSelect(string path)
        {

        }

        public void MapFileCreated(string path)
        {
            CreateMapFile(path);
        }

        void SaveMapFile()
        {

        }

        void CreateMapFile(string path)
        {

        }
        #endregion

        #region [Command]
        public ICommand CmdNew
        {
            get => new RelayCommand(() =>
            {
                MapFileSelectionViewerVM.CreateStepFolder();
            });
        }

        public ICommand CmdSave
        {
            get => new RelayCommand(() =>
            {
                //SaveRecipe();
            });
        }

        public ICommand CmdOpen
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    //dlg.InitialDirectory = recipeSelectionViewerVM.CurrentPath;
                    dlg.InitialDirectory = Constants.RootPath.RootSetupRACPath;
                    dlg.Title = "Load File";
                    dlg.Filter = "xml file (*.xml)|*.xml|Klarf file (*.001,*.smf)|*.001;*.smf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = System.IO.Path.GetDirectoryName(dlg.FileName);
                        string sFileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                        string sFileName = System.IO.Path.GetFileName(dlg.FileName);
                        string sFullPath = System.IO.Path.Combine(sFolderPath, sFileName);
                    }
                    //XmlRead(path);
                });
            }
        }
        #endregion

        #region [Callback]
        #endregion
    }
}