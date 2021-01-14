using Microsoft.Win32;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Registry = RootTools.Registry;

namespace Root_CAMELLIA
{
    public class Dlg_Setting_ViewModel : ObservableObject, IDialogRequestClose
    {
        #region Property
        private MainWindow_ViewModel m_Main;
        public MainWindow_ViewModel p_Main
        {
            get
            {
                return m_Main;
            }
            set
            {
                SetProperty(ref m_Main, value);
            }
        }

        private string m_ConfigPath = "";
        public string p_ConfigPath
        {
            get
            {
                return m_ConfigPath;
            }
            set
            {
                m_ConfigPath = value;
                //m_reg.Write(BaseDefine.RegNanoViewConfig, m_ConfigPath);
                RaisePropertyChanged("p_ConfigPath");
                //SetProperty(ref m_ConfigPath, value);
            }
        }

        private string m_NanoviewPort = "-1";
        public string p_NanoviewPort
        {
            get
            {
                return m_NanoviewPort;
            }
            set
            {
                int port;
                if(int.TryParse(value, out port))
                {
                    m_NanoviewPort = port.ToString();
                }
                else
                {
                    m_NanoviewPort = m_reg.Read(BaseDefine.RegNanoViewPort, m_NanoviewPort);
                }
                RaisePropertyChanged("p_ConfigPath");
                //SetProperty(ref m_NanoviewPort, value);
                //m_reg.Write(BaseDefine.RegNanoViewPort, m_NanoviewPort);
            }
        }

        private int m_VISBGIntegrationTime = 50;
        public int p_VISBGIntegrationTime
        {
            get
            {
                return m_VISBGIntegrationTime;
            }
            set
            {
                SetProperty(ref m_VISBGIntegrationTime, value);
            }
        }

        private int m_NIRBGIntegrationTime = 150;
        public int p_NIRBGIntegrationTime
        {
            get
            {
                return m_NIRBGIntegrationTime;
            }
            set
            {
                SetProperty(ref m_NIRBGIntegrationTime, value);
            }
        }


        private int m_VISAverage = 5;
        public int p_VISAverage
        {
            get
            {
                return m_VISAverage;
            }
            set
            {
                SetProperty(ref m_VISAverage, value);
            }
        }

        private int m_NIRAverage = 2;
        public int p_NIRAverage
        {
            get
            {
                return m_NIRAverage;
            }
            set
            {
                SetProperty(ref m_NIRAverage, value);
            }
        }

        #endregion




        public Registry m_reg;
        public Dlg_Setting_ViewModel(MainWindow_ViewModel main)
        {
            p_Main = main;
            m_reg = new Registry(App.m_engineer.m_treeRoot.p_id + ".NanoView");
           // LoadParameter();
           
        }


        #region Function
        public void LoadParameter()
        {
            (LibSR_Met.SettingData, LibSR_Met.Nanoview.ERRORCODE_NANOVIEW) nanoViewParameter = App.m_nanoView.LoadSettingParameters();
            
            if (nanoViewParameter.Item2 == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
            {
                p_VISBGIntegrationTime = nanoViewParameter.Item1.nBGIntTime_VIS;
                p_NIRBGIntegrationTime = nanoViewParameter.Item1.nBGIntTime_NIR;
                p_VISAverage = nanoViewParameter.Item1.nAverage_VIS;
                p_NIRAverage = nanoViewParameter.Item1.nAverage_NIR;
            }
            else
            {
                MessageBox.Show("Parameter Load Error Check NanoView Initialize");
            }
        }

        public void LoadConfig()
        {
             p_ConfigPath = m_reg.Read(BaseDefine.RegNanoViewConfig, m_ConfigPath);
             p_NanoviewPort = m_reg.Read(BaseDefine.RegNanoViewPort, m_NanoviewPort);
        }

        #endregion

        public ICommand CmdInitNanoView
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (App.m_nanoView.InitializeSR(p_ConfigPath,int.Parse(p_NanoviewPort)) == LibSR_Met.Nanoview.ERRORCODE_NANOVIEW.SR_NO_ERROR)
                    {
                        LoadParameter();
                        p_Main.p_InitNanoview = true;
                    }
                });

            }
        }

        public ICommand CmdSettingPath
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "Config Files (*.cfg)|*.cfg";
                    if(dialog.ShowDialog() == true)
                    {
                        p_ConfigPath = dialog.FileName;
                    }
                });
            }
        }

        public ICommand CmdSaveConfig
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBox.Show("Save Config?", "Save", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        string path = "";
                        path = m_reg.Read(BaseDefine.RegNanoViewConfig, path);
                        int port = -1;
                        port = m_reg.Read(BaseDefine.RegNanoViewPort, port);
                        if(path == p_ConfigPath && port == int.Parse(p_NanoviewPort))
                        {
                            MessageBox.Show("The storage values are the same.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        m_reg.Write(BaseDefine.RegNanoViewConfig, m_ConfigPath);
                        m_reg.Write(BaseDefine.RegNanoViewPort, m_NanoviewPort);
                        m_Main.p_InitNanoview = false;
                    }
                });
            }
        }

        public ICommand CmdSaveParameter
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(MessageBox.Show("Save Parameter?", "Save", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        LibSR_Met.SettingData set = new LibSR_Met.SettingData();
                        set.nAverage_NIR = p_NIRAverage;
                        set.nAverage_VIS = p_VISAverage;
                        set.nBGIntTime_NIR = p_NIRBGIntegrationTime;
                        set.nBGIntTime_VIS = p_VISBGIntegrationTime;
                        App.m_nanoView.SaveSettingParameters(set);
                    }
                });
            }
        }

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
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
