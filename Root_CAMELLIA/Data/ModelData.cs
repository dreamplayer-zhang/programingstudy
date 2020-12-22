using NanoView;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Met = LibSR_Met;
namespace Root_CAMELLIA.Data
{
    public class ModelData
    {
        public ModelData()
        {
            NanoView = App.m_nanoView;
            MetDataManager = Met.DataManager.GetInstance();
        }

        private Met.Nanoview _nanoView;
        public Met.Nanoview NanoView
        {
            get
            {
                return _nanoView;
            }
            private set
            {
                _nanoView = value;
            }
        }
        private Met.DataManager _metDataManager;
        public Met.DataManager MetDataManager
        {
            get
            {
                return _metDataManager;
            }
            private set
            {
                _metDataManager = value;
            }
        }

        private List<string> _materialList = new List<string>();
        public List<string> MaterialList
        {
            get
            {
                return _materialList;
            }
            set
            {
                _materialList = value;
            }
        }

        public int GetLayerCount()
        {
            return App.m_nanoView.m_LayerList.Count;
        }

        public void AddLayer(int index = -1)
        {
            Layer layerData = new Layer();

            if(index == -1)
            {
                //MetDataManager.m_LayerData.Add(layerData);
                App.m_nanoView.m_LayerList.Add(layerData);
            }
            else
            {
               // MetDataManager.m_LayerData.Insert(index, layerData);
                App.m_nanoView.m_LayerList.Insert(index, layerData);
            }
        }

        public void DeleteLayer(int index = -1)
        {
            App.m_nanoView.m_LayerList.RemoveAt(index);
        }

        public class LayerData : ObservableObject
        {

            public LayerData(string layerHeader)
            {
                this.LayerHeader = layerHeader;
                this.Host1.Add(new PathEntity("None", "None"));
                SelectedHost1 = Host1[0].FullPath;
                this.Guest1.Add(new PathEntity("None", "None"));
                SelectedGuest1 = Guest1[0].FullPath;
                this.Guest2.Add(new PathEntity("None", "None"));
                SelectedGuest2 = Guest2[0].FullPath;
                this.Fv1 = 0.0f;
                this.Fv2 = 0.0f;
                this.Fv1Fit = false;
                this.Fv2Fit = false;
                this.Emm.Add("1. Bruggeman");
                this.Emm.Add("2. Maxwell Ganett-LL");
                this.Emm.Add("3. Maxwell Garnett");
                SelectedEmm = Emm[0];

            }

            #region Property
            private string _layerHeader;
            public string LayerHeader
            {
                get
                {
                    return _layerHeader;
                }
                set
                {
                    _layerHeader = value;
                }
            }

            private string _seletedHost1;
            public string SelectedHost1
            {
                get
                {
                    return _seletedHost1;
                }
                set
                {
                    _seletedHost1 = value;
                    RaisePropertyChanged("SelectedHost1");
                }
            }
            private List<PathEntity> _host1 = new List<PathEntity>();
            public List<PathEntity> Host1
            {
                get
                {
                    return _host1;
                }
                set
                {
                    _host1 = value;
                    RaisePropertyChanged("Host1");
                }
            }

            private string _seletedHost2;
            public string SelectedHost2
            {
                get
                {
                    return _seletedHost2;
                }
                set
                {
                    _seletedHost2 = value;
                    RaisePropertyChanged("SelectedHost2");
                }
            }
            private List<PathEntity> _host2 = new List<PathEntity>();
            public List<PathEntity> Host2
            {
                get
                {
                    return _host2;
                }
                set
                {
                    _host2 = value;
                    RaisePropertyChanged("Host2");
                }
            }


            private string _seletedGuest1;
            public string SelectedGuest1
            {
                get
                {
                    return _seletedGuest1;
                }
                set
                {
                    _seletedGuest1 = value;
                    RaisePropertyChanged("SelectedGuest1");
                }
            }
            private List<PathEntity> _guest1 = new List<PathEntity>();
            public List<PathEntity> Guest1
            {
                get
                {
                    return _guest1;
                }
                set
                {
                    _guest1 = value;
                    RaisePropertyChanged("Guest1");
                }
            }

            private string _seletedGuest2;
            public string SelectedGuest2
            {
                get
                {
                    return _seletedGuest2;
                }
                set
                {
                    _seletedGuest2 = value;
                    RaisePropertyChanged("SelectedGuest2");
                }
            }
            private List<PathEntity> _guest2 = new List<PathEntity>();
            public List<PathEntity> Guest2
            {
                get
                {
                    return _guest2;
                }
                set
                {
                    _guest2 = value;
                    RaisePropertyChanged("Guest2");
                }
            }

            private double _fv1 = 0.0f;
            public double Fv1
            {
                get
                {
                    return _fv1;
                }
                set
                {
                    _fv1 = value;
                    RaisePropertyChanged("Fv1");
                }
            }

            private double _fv2 = 0.0f;
            public double Fv2
            {
                get
                {
                    return _fv2;
                }
                set
                {
                    _fv2 = value;
                    RaisePropertyChanged("Fv2");
                }
            }

            private bool _fv1Fit = false;
            public bool Fv1Fit
            {
                get
                {
                    return _fv1Fit;
                }
                set
                {
                    _fv1Fit = value;
                    RaisePropertyChanged("Fv1Fit");
                }
            }

            private bool _fv2Fit = false;

            public bool Fv2Fit
            {
                get
                {
                    return _fv2Fit;
                }
                set
                {
                    _fv2Fit = value;
                    RaisePropertyChanged("Fv2Fit");
                }
            }

            private double _thickness = 0.0f;
            public double Thickness
            {
                get
                {
                    return _thickness;
                }
                set
                {
                    _thickness = value;
                    RaisePropertyChanged("Thickness");
                }
            }

            private bool _thicknessFit = false;
            public bool ThicknessFit
            {
                get
                {
                    return _thicknessFit;
                }
                set
                {
                    _thicknessFit = value;
                    RaisePropertyChanged("ThicknessFit");
                }
            }

            private string _seletedEmm;
            public string SelectedEmm
            {
                get
                {
                    return _seletedEmm;
                }
                set
                {
                    _seletedEmm = value;
                    RaisePropertyChanged("SelectedEmm");
                }
            }
            private List<string> _Emm = new List<string>();
            public List<string> Emm
            {
                get
                {
                    return _Emm;
                }
                set
                {
                    _Emm = value;
                    RaisePropertyChanged("Emm");
                }
            }
            #endregion

            #region Function
            public void UpdateGridLayer(int currentLayer)
            {
                Layer layerData;
                layerData = App.m_nanoView.m_LayerList.ElementAt(currentLayer);
                if(layerData.m_Host == null)
                {
                    SelectedHost1 = Host1[0].FullPath;
                }
                else
                {
                    SelectedHost1 = string.Concat(layerData.m_Host.m_Path);
                }
                if(layerData.m_Guest1 == null)
                {
                    SelectedGuest1 = Guest1[0].FullPath;
                }
                else
                {
                    SelectedGuest1 = string.Concat(layerData.m_Guest1.m_Path);
                }
                if(layerData.m_Guest2 == null)
                {
                    SelectedGuest2 = Guest2[0].FullPath;
                }
                else
                {
                    SelectedGuest2 = string.Concat(layerData.m_Guest2.m_Path);
                }

                this.Fv1 = layerData.m_fv1;
                this.Fv2 = layerData.m_fv2;

                if(layerData.m_bFitfv1)
                {
                      this.Fv1Fit = true;
                }
                else
                {
                    this.Fv1Fit = false;
                }
                if(layerData.m_bFitfv2)
                {
                    this.Fv2Fit = true;
                }
                else
                {
                    this.Fv2Fit = false;
                }

                this.Thickness = layerData.m_Thickness;
                if(layerData.m_bFitThickness)
                {
                    this.ThicknessFit = true;
                }
                else
                {
                    this.ThicknessFit = false;
                }

                switch (layerData.m_Emm)
                {
                    case 1:
                        this.SelectedEmm = Emm[0];
                        break;
                    case 2:
                        this.SelectedEmm = Emm[1];
                        break;
                    case 3:
                        this.SelectedEmm = Emm[2];
                        break;
                }
            }

            public bool CheckLayerHost(int currentLayer)
            {
                Layer layerData;
                layerData = App.m_nanoView.m_LayerList.ElementAt(currentLayer);

                if(layerData.m_Host == null)
                {
                    return false;
                }
                return true;
            }

            public void UpdateModelLayer(int currentLayer)
            {
                Layer layerData;
                layerData = App.m_nanoView.m_LayerList.ElementAt(currentLayer);

                layerData.m_Host = App.m_nanoView.GetMaterialFromName(Path.GetFileNameWithoutExtension(SelectedHost1));
                layerData.m_Guest1 = App.m_nanoView.GetMaterialFromName(Path.GetFileNameWithoutExtension(SelectedGuest1));
                layerData.m_Guest2 = App.m_nanoView.GetMaterialFromName(Path.GetFileNameWithoutExtension(SelectedGuest2));

                layerData.m_fv1 = layerData.m_fv1After = Convert.ToDouble(Fv1);
                layerData.m_fv2 = layerData.m_fv2After = Convert.ToDouble(Fv2);
                if (Convert.ToBoolean(Fv1Fit))
                {
                    layerData.m_bFitfv1 = true;
                }
                else
                {
                    layerData.m_bFitfv1 = false;
                }
                if(Convert.ToBoolean(Fv2Fit))
                {
                    layerData.m_bFitfv2 = true;
                }
                else
                {
                    layerData.m_bFitfv2 = false;
                }

                layerData.m_Thickness = layerData.m_ThicknessAfter = Convert.ToDouble(Thickness);
                if (Convert.ToBoolean(ThicknessFit))
                {
                    layerData.m_bFitThickness = true;
                }
                else
                {
                    layerData.m_bFitThickness = false;
                }
                switch (SelectedEmm)
                {
                    case "1. Bruggeman":
                        layerData.m_Emm = 1;
                        break;
                    case "2. Maxwell Ganett-LL":
                        layerData.m_Emm = 2;
                        break;
                    case "3. Maxwell Garnett":
                        layerData.m_Emm = 3;
                        break;
                }
            }
            #endregion

            public class PathEntity : ObservableObject
            {
                public PathEntity(string fullPath, string name)
                {
                    FullPath = fullPath;
                    Name = name;
                }
                private string _fullPath;
                public string FullPath
                {
                    get
                    {
                        return _fullPath;
                    }
                    set
                    {
                        _fullPath = value;
                        RaisePropertyChanged("FullPath");
                    }
                }

                private string _name;
                public string Name
                {
                    get
                    {
                        return _name;
                    }
                    set
                    {
                        _name = value;
                        RaisePropertyChanged("Name");
                    }
                }
            }
        }
    }
}
