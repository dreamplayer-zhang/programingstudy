using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using EasyModbus;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using RootTools.Comm;

namespace RootTools
{
    public delegate void delegateString(string str);
    public class FFU_Group : ObservableObject, ITool
    {
        #region ITool
        public UserControl p_ui
        {
            get
            {
                FFU_Group_UI ui = new FFU_Group_UI();
                ui.Init(this);
                return ui;
            }
        }
        public string p_id
        {
            get; set;
        }
        #endregion

        public Modbus m_Modbus;
        Thread m_threadCommunicate;
        bool m_bRunThread;

        public event delegateString OnDetectLimit;

        string m_sSerialPort = "COM0";
        public string p_sSerialPort
        {
            get
            {
                return m_sSerialPort;
            }
            set
            {
                SetProperty(ref m_sSerialPort, value);
            }
        }
        int m_nBaudrate = 9600;
        Parity m_eParity = Parity.None;
        StopBits m_eStopBit = StopBits.Two;
        int m_nInterval = 1000;

        ObservableCollection<FFUModule> m_aFFU = new ObservableCollection<FFUModule>();
        public ObservableCollection<FFUModule> p_aFFU
        {
            get
            {
                return m_aFFU;
            }
            set
            {
                SetProperty(ref m_aFFU, value);
            }
        }

        FFUModule m_aSelectedModule;
        public FFUModule p_aSelectedModule
        {
            get
            {
                return m_aSelectedModule;
            }
            set
            {
                SetProperty(ref m_aSelectedModule, value);
            }
        }

        bool m_bBusy = false;
        Stopwatch m_swWaitRecive = new Stopwatch();
        int m_nTimeout = 3000;
        Log m_log;
        string m_sFilePath = @"C:\WIND2\Init\FFUSetting.ini";
        private readonly IDialogService m_DialogService;

        public FFU_Group(string id, Log log, IDialogService dialogService = null)
        {
            p_id = id;
            m_log = log;
            m_DialogService = dialogService;
            LoadModule();
            Init();
        }

        string sSectionFFU = "FFUGROUP";
        string sSectionPort = "COMPort";
        string sSectionCount = "Count";
        string sSectionModule = "FFU ";
        string sSectionModuleName = "Name";
        string sSectionModuleAddress = "Address";
        string sSectionModuleDP = "Point";
        string sSectionModuleMax = "Max";
        string sSectionModuleMin = "Min";

        public void SaveModule()
        {
            FileInfo file = new FileInfo(m_sFilePath);
            DirectoryInfo dir = file.Directory;
            if (!dir.Exists)
                dir.Create();
            GeneralFunction.WriteINIFile(sSectionFFU, sSectionPort, p_sSerialPort, m_sFilePath);
            GeneralFunction.WriteINIFile(sSectionFFU, sSectionCount, p_aFFU.Count.ToString(), m_sFilePath);
            for (int i = 0; i < p_aFFU.Count; i++)
            {
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleName, p_aFFU[i].p_sID, m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMax, p_aFFU[i].p_nMaxRPM.ToString(), m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMin, p_aFFU[i].p_nMinRPM.ToString(), m_sFilePath);
            }
        }

        public void LoadModule()
        {
            if (File.Exists(m_sFilePath))
            {
                p_aFFU.Clear();
                p_aFFU = new ObservableCollection<FFUModule>();
                p_sSerialPort = GeneralFunction.ReadINIFile(sSectionFFU, sSectionPort, m_sFilePath);

                int nCount = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionFFU, sSectionCount, m_sFilePath));

                for (int i = 0; i < nCount; i++)
                {
                    FFUModule temp = new FFUModule();
                    temp.p_sID = GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleName, m_sFilePath);
                    temp.p_nMaxRPM = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMax, m_sFilePath));
                    temp.p_nMinRPM = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMin, m_sFilePath));
                    p_aFFU.Add(temp);
                    p_aFFU[p_aFFU.Count - 1].OnDetectLimit += FFUGroup_OnDetectLimit;
                }
            }
        }

        public bool Init()
        {

            m_Modbus = new Modbus(p_id,m_log);
            m_Modbus.m_client.SerialPort = m_sSerialPort;
            m_Modbus.m_client.Baudrate = m_nBaudrate;
            m_Modbus.m_client.Parity = m_eParity;
            m_Modbus.m_client.StopBits = m_eStopBit;
            try
            {

                m_Modbus.Connect();
            }
            catch
            {

            }
            if (m_Modbus.p_bConnect)
            {
                m_threadCommunicate = new Thread(RunThread);
                m_threadCommunicate.Start();
            }
            return m_Modbus.p_bConnect;
        }

        void RunThread()
        {
            m_bRunThread = true;
            Thread.Sleep(5000);
            while (m_bRunThread)
            {
                Thread.Sleep(10);

                if (m_bRunThread == false)
                    return;
                try
                {
                    List<int> aTemp = new List<int>();
                    for (int i = 0; i < p_aFFU.Count; i++)
                        aTemp.Add(0);

                    Thread.Sleep(10);
                    m_Modbus.ReadHoldingRegister(1, 0, aTemp);
                    for (int i = 0; i < p_aFFU.Count; i++)
                        p_aFFU[i].p_nRPM = aTemp[i];

                    Thread.Sleep(10);
                    m_Modbus.ReadHoldingRegister(1, 128, aTemp);
                    for (int i = 0; i < p_aFFU.Count; i++)
                        p_aFFU[i].p_nPressure = aTemp[i];
                }
                catch (Exception e)
                {
                    // Log추가
                }
                Thread.Sleep(m_nInterval);
            }
        }

        public void ThreadStop()
        {
            m_bRunThread = false;
            if (m_threadCommunicate != null)
                m_threadCommunicate.Join();
        }

        public void AddModule()
        {
            p_aFFU.Add(new FFUModule());
            p_aFFU[p_aFFU.Count - 1].OnDetectLimit += FFUGroup_OnDetectLimit;
            SaveModule();
        }

        private void FFUGroup_OnDetectLimit(string str)
        {
            if(OnDetectLimit != null)
                OnDetectLimit(str);
        }

        public void RemoveModule()
        {
            p_aFFU.RemoveAt(p_aFFU.Count - 1);
            SaveModule();
            LoadModule();
        }


        public RelayCommand CommandAddModule
        {
            get
            {
                return new RelayCommand(AddModule);
            }
        }

        public RelayCommand CommandRemoveModule
        {
            get
            {
                return new RelayCommand(RemoveModule);
            }
        }

        public void DoubleClickAction()
        {
            var viewModel = p_aSelectedModule;
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
        }

        public RelayCommand MyDoubleClickCommand
        {
            get
            {
                return new RelayCommand(DoubleClickAction);
            }
        }

    }

    [Serializable]
    public class FFUModule : ObservableObject, IDialogRequestClose
    {
        string m_sID = "";
        int m_nUnit = 0;
        int m_nPressure = 0;
        int m_nRPM = 0;
        int m_nMaxRPM = 0;
        int m_nMinRPM = 0;
        public event delegateString OnDetectLimit;

        public string p_sID
        {
            get
            {
                return m_sID;
            }
            set
            {
                SetProperty(ref m_sID, value);
            }
        }

        public int p_nUnit
        {
            get
            {
                return m_nUnit;
            }
            set
            {
                SetProperty(ref m_nUnit, value);
            }
        }

        public int p_nRPM
        {
            get
            {
                return m_nRPM;
            }
            set
            {
                SetProperty(ref m_nRPM, value);
                if (p_nMaxRPM < value || p_nMinRPM > value)
                {
                    OnDetectLimit("FDC : " + m_sID + " Value : " + m_nRPM + " Limit ( " + p_nMinRPM + ", " + p_nMaxRPM + " )");
                }
            }
        }

        public int p_nPressure
        {
            get
            {
                return m_nPressure;
            }
            set
            {
                SetProperty(ref m_nPressure, value);
            }
        }

        public int p_nMaxRPM
        {
            get
            {
                return m_nMaxRPM;
            }
            set
            {
                SetProperty(ref m_nMaxRPM, value);
            }
        }
        public int p_nMinRPM
        {
            get
            {
                return m_nMinRPM;
            }
            set
            {
                SetProperty(ref m_nMinRPM, value);
            }
        }


        public FFUModule()
        {
        }

        public FFUModule(int nUnit)
        {
            p_nUnit = nUnit;
        }

        [field: NonSerialized]
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

}


