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
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Linq;

namespace Root_WindII
{
    public class RACProduct_ViewModel : ObservableObject
    {
        #region [Properites]
        private MapFileListViewer_ViewModel mapFileListViewerVM = new MapFileListViewer_ViewModel();
        public MapFileListViewer_ViewModel MapFileListViewerVM
        {
            get => this.mapFileListViewerVM;
            set
            {
                SetProperty<MapFileListViewer_ViewModel>(ref this.mapFileListViewerVM, value);
            }
        }

        private MapViewer_ViewModel mapViewerVM = new MapViewer_ViewModel();
        public MapViewer_ViewModel MapViewerVM
        {
            get => this.mapViewerVM;
            set
            {
                SetProperty<MapViewer_ViewModel>(ref this.mapViewerVM, value);
            }
        }

        private ObservableCollection<Rectangle> chipItems;
        public ObservableCollection<Rectangle> ChipItems
        {
            get => this.chipItems;
            set
            {
                SetProperty<ObservableCollection<Rectangle>>(ref this.chipItems, value);
            }
        }

        private XMLParser xmlParser;
        public XMLParser XmlParser
        {
            get => this.xmlParser;
            set
            {
                SetProperty<XMLParser>(ref this.xmlParser, value);
            }
        }

        private string currentFilePath = "";
        public string CurrentFilePath
        {
            get => this.currentFilePath;
            set
            {
                SetProperty<string>(ref this.currentFilePath, value);
            }
        }

        private bool isBacksideChecked = false;
        public bool IsBacksideChecked
        {
            get => this.isBacksideChecked;
            set
            {
                SetProperty<bool>(ref this.isBacksideChecked, value);
            }
        }

        private string dataGrossDie = "";
        public string DataGrossDie
        {
            get => this.dataGrossDie;
            set
            {
                SetProperty<string>(ref this.dataGrossDie, value);
            }
        }

        private string dataDescription = "";
        public string DataDescription
        {
            get => this.dataDescription;
            set
            {
                SetProperty<string>(ref this.dataDescription, value);
            }
        }

        private string dataDevice = "";
        public string DataDevice
        {
            get => this.dataDevice;
            set
            {
                SetProperty<string>(ref this.dataDevice, value);
            }
        }

        private string dataSizeX = "";
        public string DataSizeX
        {
            get => this.dataSizeX;
            set
            {
                SetProperty<string>(ref this.dataSizeX, value);
            }
        }

        private string dataSizeY = "";
        public string DataSizeY
        {
            get => this.dataSizeY;
            set
            {
                SetProperty<string>(ref this.dataSizeY, value);
            }
        }

        private string dataDiePitchX = "";
        public string DataDiePitchX
        {
            get => this.dataDiePitchX;
            set
            {
                SetProperty<string>(ref this.dataDiePitchX, value);
            }
        }

        private string dataDiePitchY = "";
        public string DataDiePitchY
        {
            get => this.dataDiePitchY;
            set
            {
                SetProperty<string>(ref this.dataDiePitchY, value);
            }
        }

        private string dataScribeLineX = "";
        public string DataScribeLineX
        {
            get => this.dataScribeLineX;
            set
            {
                SetProperty<string>(ref this.dataScribeLineX, value);
            }
        }

        private string dataScribeLineY = "";
        public string DataScribeLineY
        {
            get => this.dataScribeLineY;
            set
            {
                SetProperty<string>(ref this.dataScribeLineY, value);
            }
        }

        private string dataShotOffsetX = "";
        public string DataShotOffsetX
        {
            get => this.dataShotOffsetX;
            set
            {
                SetProperty<string>(ref this.dataShotOffsetX, value);
            }
        }

        private string dataShotOffsetY = "";
        public string DataShotOffsetY
        {
            get => this.dataShotOffsetY;
            set
            {
                SetProperty<string>(ref this.dataShotOffsetY, value);
            }
        }

        private string dataMapOffsetX = "";
        public string DataMapOffsetX
        {
            get => this.dataMapOffsetX;
            set
            {
                SetProperty<string>(ref this.dataMapOffsetX, value);
            }
        }

        private string dataMapOffsetY = "";
        public string DataMapOffsetY
        {
            get => this.dataMapOffsetY;
            set
            {
                SetProperty<string>(ref this.dataMapOffsetY, value);
            }
        }

        private string dataSmiOffsetX = "";
        public string DataSmiOffsetX
        {
            get => this.dataSmiOffsetX;
            set
            {
                SetProperty<string>(ref this.dataSmiOffsetX, value);
            }
        }

        private string dataSmiOffsetY = "";
        public string DataSmiOffsetY
        {
            get => this.dataSmiOffsetY;
            set
            {
                SetProperty<string>(ref this.dataSmiOffsetY, value);
            }
        }

        private string dataOriginDieX = "";
        public string DataOriginDieX
        {
            get => this.dataOriginDieX;
            set
            {
                SetProperty<string>(ref this.dataOriginDieX, value);
            }
        }

        private string dataOriginDieY = "";
        public string DataOriginDieY
        {
            get => this.dataOriginDieY;
            set
            {
                SetProperty<string>(ref this.dataOriginDieY, value);
            }
        }

        private string dataShotSizeX = "";
        public string DataShotSizeX
        {
            get => this.dataShotSizeX;
            set
            {
                SetProperty<string>(ref this.dataShotSizeX, value);
            }
        }

        private string dataShotSizeY = "";
        public string DataShotSizeY
        {
            get => this.dataShotSizeY;
            set
            {
                SetProperty<string>(ref this.dataShotSizeY, value);
            }
        }

        private double dataEdgeExclusion = 3.0;
        public double DataEdgeExclusion
        {
            get => this.dataEdgeExclusion;
            set
            {
                SetProperty<double>(ref this.dataEdgeExclusion, value);
            }
        }
        #endregion

        public RACProduct_ViewModel()
        {
            this.MapFileListViewerVM.Refresh();
            this.MapFileListViewerVM.SelectedCellsChanged += DataViewerVM_SelectedCellsChanged;
        }

        #region [Event]
        private void MapFileSelect(string path)
        {

        }

        private void SaveMapFile()
        {

        }

        private void CreateMapFile(string path)
        {

        }

        private void UpdateProductInfo(string path)
        {
            XMLData data = GlobalObjects.Instance.Get<XMLData>();

            if (path.ToLower().Contains(".xml"))
            {
                XMLParser.ParseMapInfo(path, data);
            }
            else if (path.ToLower().Contains(".smf") || path.ToLower().Contains(".001"))
            {
                StreamReader sr = new StreamReader(path);
                KlarfFileReader.OpenKlarfMapData(sr, ref data);
            }

            int[] tempMap = data.GetWaferMap();
            this.MapViewerVM.CreateMap((int)data.GetUnitSize().Width, (int)data.GetUnitSize().Height, tempMap.Select(d => (int)d).ToArray());

            this.DataGrossDie = XMLData.MakeWaferMap(data.GetUnitDieList(), data.GetUnitSize()).Length.ToString();
            this.DataDescription = data.Description;

            this.DataDevice = data.Device;
            this.DataSizeX = data.GetUnitSize().Width.ToString();
            this.DataSizeY = data.GetUnitSize().Height.ToString();
            this.DataDiePitchX = data.DiePitchX.ToString();
            this.DataDiePitchY = data.DiePitchY.ToString();
            this.DataScribeLineX = data.ScribeLineX.ToString();
            this.DataScribeLineY = data.ScribeLineY.ToString();

            if (this.IsBacksideChecked == false)
            {
                this.DataShotOffsetX = Math.Round(data.ShotOffsetX, 1).ToString();
                this.DataMapOffsetX = Math.Round(data.MapOffsetX, 1).ToString();
                this.DataSmiOffsetX = Math.Round(data.SMIOffsetX, 1).ToString();
            }
            else
            {
                this.DataShotOffsetX = Math.Round(data.ShotOffsetX_Backside, 1).ToString();
                this.DataMapOffsetX = Math.Round(data.MapOffsetX_Backside, 1).ToString();
                this.DataSmiOffsetX = Math.Round(data.SMIOffsetX_Backside, 1).ToString();
            }
            this.DataShotOffsetY = Math.Round(data.ShotOffsetY, 1).ToString();
            this.DataMapOffsetY = Math.Round(data.MapOffsetY, 1).ToString();
            this.DataSmiOffsetY = Math.Round(data.SMIOffsetY, 1).ToString();
            this.DataOriginDieX = data.OriginDieX.ToString();
            this.DataOriginDieY = data.OriginDieY.ToString();
            this.DataShotSizeX = data.ShotX.ToString();
            this.DataShotSizeY = data.ShotY.ToString();
        }

        private void ClearProductInfo()
        {
            this.MapViewerVM.CreateMap(1, 1, null);

            this.IsBacksideChecked = false;
            this.DataGrossDie = "";
            this.DataDescription = "";

            this.DataDevice = "";
            this.DataSizeX = "";
            this.DataSizeY = "";
            this.DataDiePitchX = "";
            this.DataDiePitchY = "";
            this.DataScribeLineX = "";
            this.DataScribeLineY = "";
            this.DataShotOffsetX = "";
            this.DataShotOffsetY = "";
            this.DataMapOffsetX = "";
            this.DataMapOffsetY = "";
            this.DataSmiOffsetX = "";
            this.DataSmiOffsetY = "";
            this.DataOriginDieX = "";
            this.DataOriginDieY = "";
            this.DataShotSizeX = "";
            this.DataShotSizeY = "";
        }

        private void DataViewerVM_SelectedCellsChanged(object obj)
        {
            MapFileListViewerItem row = (MapFileListViewerItem)obj;
            if (row == null)
                return;

            this.CurrentFilePath = row.MapFilePath;
            UpdateProductInfo(this.CurrentFilePath);
        }
        #endregion

        #region [Command]
        public ICommand CmdNew
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public ICommand CmdLoad
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RootSetupRACPath;
                    dlg.Title = "Load File";
                    dlg.Filter = "xml file (*.xml)|*.xml|Klarf file (*.001,*.smf)|*.001;*.smf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = System.IO.Path.GetDirectoryName(dlg.FileName);
                        string sFileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                        string sFileName = System.IO.Path.GetFileName(dlg.FileName);
                        string sFullPath = System.IO.Path.Combine(sFolderPath, sFileName);
                        string sFileNameNoExtCopy = sFileNameNoExt;
                        string sFileNameCopy = "";
                        string sFullPathCopy = "";
                        bool isDuplicatedName = false;
                        bool isCopied = false;

                        if (sFileName.Contains(".xml") == true)
                        {
                            DirectoryInfo di = new DirectoryInfo(this.MapFileListViewerVM.MapFileRootPath);
                            di.Create();

                            foreach (FileInfo file in di.GetFiles())
                            {
                                string fileName = file.Name.ToLower();
                                if (fileName.Contains(sFileName.ToLower()) && file.FullName.ToLower() != sFullPath.ToLower())
                                {
                                    while (true)
                                    {
                                        sFileNameNoExtCopy = sFileNameNoExtCopy + "_Copy";
                                        sFileNameCopy = sFileNameNoExtCopy + System.IO.Path.GetExtension(dlg.FileName);

                                        foreach (FileInfo file2 in di.GetFiles())
                                        {
                                            fileName = file2.Name.ToLower();
                                            if (fileName.Contains(sFileNameCopy.ToLower()) == false)
                                            {
                                                isDuplicatedName = false;
                                            }
                                            else
                                            {
                                                isDuplicatedName = true;
                                                break;
                                            }
                                        }
                                        if (isDuplicatedName == false)
                                        {
                                            sFullPathCopy = di.FullName + "\\" + sFileNameCopy;
                                            System.IO.File.Copy(sFullPath, sFullPathCopy, true);
                                            Application.Current.Dispatcher.Invoke((Action)delegate
                                            {
                                                this.MapFileListViewerVM.MapFileListViewerItems.Add(new MapFileListViewerItem() { MapFileName = sFileNameCopy, MapFilePath = sFullPathCopy });
                                            });
                                            isCopied = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (isCopied == false)
                            {
                                sFullPathCopy = di.FullName + "\\" + sFileName;
                                System.IO.File.Copy(sFullPath, sFullPathCopy, true);
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    this.MapFileListViewerVM.MapFileListViewerItems.Add(new MapFileListViewerItem() { MapFileName = sFileName, MapFilePath = sFullPathCopy });
                                });
                            }
                        }
                        this.CurrentFilePath = sFullPath;
                        UpdateProductInfo(this.CurrentFilePath);
                    }
                });
            }
        }

        public ICommand CmdSave
        {
            get => new RelayCommand(() =>
            {
                SaveMapFile();
            });
        }

        public ICommand CmdClear
        {
            get => new RelayCommand(() =>
            {
                ClearProductInfo();
            });
        }
        #endregion
    }
}