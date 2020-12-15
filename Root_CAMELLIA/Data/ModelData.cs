using NanoView;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            return NanoView.m_Model.m_LayerList.Count;
        }

        public void AddLayer(int index = -1)
        {
            Layer layer = new Layer();
            if(index == -1)
            {
                App.m_nanoView.m_Model.m_LayerList.Add(layer);
            }
            else
            {
                App.m_nanoView.m_Model.m_LayerList.Insert(index, layer);
            }
        }

        public void DeleteLayer(int index = -1)
        {
            App.m_nanoView.m_Model.m_LayerList.RemoveAt(index);
        }

        public class LayerData : ObservableObject
        {

            public LayerData(string layerHeader)
            {
                this.LayerHeader = layerHeader;
                this.Host1.Add("None");
                SelectedHost1 = Host1[0];
                this.Guest1.Add("None");
                SelectedGuest1 = Guest1[0];
                this.Guest2.Add("None");
                SelectedGuest2 = Guest2[0];
                this.Fv1 = 0.0f;
                this.Fv2 = 0.0f;
                this.Fv1Fit = false;
                this.Fv2Fit = false;
                this.Emm.Add("1.Bruggeman");
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
            private ObservableCollection<string> _host1 = new ObservableCollection<string>();
            public ObservableCollection<string> Host1
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
            private ObservableCollection<string> _host2 = new ObservableCollection<string>();
            public ObservableCollection<string> Host2
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
            private ObservableCollection<string> _guest1 = new ObservableCollection<string>();
            public ObservableCollection<string> Guest1
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
            private ObservableCollection<string> _guest2 = new ObservableCollection<string>();
            public ObservableCollection<string> Guest2
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
            private ObservableCollection<string> _Emm = new ObservableCollection<string>();
            public ObservableCollection<string> Emm
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
                Layer layer;
                layer = App.m_nanoView.m_Model.m_LayerList.ElementAt(currentLayer);
                if(layer.m_Host == null)
                {
                    SelectedHost1 = Host1[0];
                }
                else
                {
                    SelectedHost1 = layer.m_Host.m_Name;
                }
                if(layer.m_Guest1 == null)
                {
                    SelectedGuest1 = Guest1[0];
                }
                else
                {
                    SelectedGuest1 = layer.m_Guest1.m_Name;
                }
                if(layer.m_Guest2 == null)
                {
                    SelectedGuest2 = Guest2[0];
                }
                else
                {
                    SelectedGuest2 = layer.m_Guest2.m_Name;
                }
                this.Fv1 = layer.m_fv1After;
                this.Fv2 = layer.m_fv2After;
                if(layer.m_bFitfv1 == true)
                {
                      this.Fv1Fit = true;
                }
                else
                {
                    this.Fv1Fit = false;
                }
                if(layer.m_bFitfv2 == true)
                {
                    this.Fv2Fit = true;
                }
                else
                {
                    this.Fv2Fit = false;
                }
                switch (layer.m_Emm)
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

            public void UpdateModelLayer(int currentLayer)
            {
                Layer layer;
                Model model = App.m_nanoView.m_Model;
                layer = App.m_nanoView.m_Model.m_LayerList.ElementAt(currentLayer);

                layer.m_Host = model.GetMaterialFromName(SelectedHost1);
                layer.m_Guest1 = model.GetMaterialFromName(SelectedGuest1);
                layer.m_Guest2 = model.GetMaterialFromName(SelectedGuest2);
                layer.m_fv1 = layer.m_fv1After = Convert.ToDouble(Fv1);
                layer.m_fv2 = layer.m_fv2After = Convert.ToDouble(Fv2);
                if (Convert.ToBoolean(Fv1Fit) == true)
                {
                    layer.m_bFitfv1 = true;
                }
                else
                {
                    layer.m_bFitfv1 = false;
                }
                if(Convert.ToBoolean(Fv2Fit) == true)
                {
                    layer.m_bFitfv2 = true;
                }
                else
                {
                    layer.m_bFitfv2 = false;
                }
                switch (SelectedEmm)
                {
                    case "1. Bruggeman":
                        layer.m_Emm = 1;
                        break;
                    case "2. Maxwell Ganett-LL":
                        layer.m_Emm = 2;
                        break;
                    case "3. Maxwell Garnett":
                        layer.m_Emm = 3;
                        break;
                }
            }
            #endregion

        }
    }
}
