using System;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using EasyModbus;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;

namespace RootTools
{
    public class TK4SGroup : ObservableObject, ITool
    {

        #region ITool
        public UserControl p_ui
        {
            get
            {
                TK4SGourpUI ui = new TK4SGourpUI();
                ui.Init(this);
                return ui;
            }
        }
        public string p_id
        {
            get; set;
        }
        #endregion

        ModbusClient m_client = new ModbusClient();
        Thread m_threadCommunicate;
        bool m_bRunThread;

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
        int m_nTryConnect = 3;
        int m_nInterval = 1000;

        ObservableCollection<TK4S> m_aTK4S = new ObservableCollection<TK4S>();
        public ObservableCollection<TK4S> p_aTK4S
        {
            get
            {
                return m_aTK4S;
            }
            set
            {
                SetProperty(ref m_aTK4S, value);
            }
        }
        TK4S m_SelectedTK4S = null;

        public TK4S p_SelectedTK4S
        {
            get
            {
                return m_SelectedTK4S;
            }
            set
            {
                SetProperty(ref m_SelectedTK4S, value);
            }
        }


        bool m_bBusy = false;
        Stopwatch m_swWaitRecive = new Stopwatch();
        int m_nTimeout = 3000;
        Log m_log;
        string m_sFilePath = @"C:\WIND2\Init\FDCSetting.ini";
        int nCount;

        private readonly IDialogService m_DialogService;

        public TK4SGroup(string id, Log log, IDialogService dialogService = null)
        {
            p_id = id;
            m_log = log;
            m_DialogService = dialogService;
            LoadModule();
            Init();
        }

        string sSectionFDC = "FDC";
        string sSectionPort = "COMPort";
        string sSectionCount = "Count";
        string sSectionModule = "TK4S ";
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
            GeneralFunction.WriteINIFile(sSectionFDC, sSectionPort, p_sSerialPort, m_sFilePath);
            GeneralFunction.WriteINIFile(sSectionFDC, sSectionCount, p_aTK4S.Count.ToString(), m_sFilePath);
            for (int i = 0; i < p_aTK4S.Count; i++)
            {
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleName, p_aTK4S[i].p_sID, m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleAddress, p_aTK4S[i].p_nAddress.ToString(), m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleDP, p_aTK4S[i].p_nDecimalPoint.ToString(), m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMax, p_aTK4S[i].p_dMaxValue.ToString(), m_sFilePath);
                GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMin, p_aTK4S[i].p_dMinValue.ToString(), m_sFilePath);
            }
        }

        public void LoadModule()
        {
            if (File.Exists(m_sFilePath))
            {
                p_aTK4S.Clear();
                p_aTK4S = new ObservableCollection<TK4S>();

                int nCount = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionFDC, sSectionCount, m_sFilePath));
                for (int i = 0; i < nCount; i++)
                {
                    p_aTK4S.Add(new TK4S());
                    p_aTK4S[i].p_sID = GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleName, m_sFilePath);
                    p_aTK4S[i].p_nAddress = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleAddress, m_sFilePath));
                    p_aTK4S[i].p_nDecimalPoint = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleDP, m_sFilePath));
                    p_aTK4S[i].p_dMaxValue = Convert.ToDouble(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMax, m_sFilePath));
                    p_aTK4S[i].p_dMinValue = Convert.ToDouble(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMin, m_sFilePath));
                }
            }
        }

        public bool Init()
        {
            m_client.SerialPort = m_sSerialPort;
            m_client.Baudrate = m_nBaudrate;
            m_client.Parity = m_eParity;
            m_client.StopBits = m_eStopBit;
            //for (int n = 0; n < m_nTryConnect; n++)
            //{
            m_client.Connect();
            //    if (m_client.Connected)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(100);
            //}
            if (m_client.Connected)
            {
                m_client.SendDataChanged += m_client_SendDataChanged;
                m_client.ReceiveDataChanged += m_client_ReceiveDataChanged;
                m_threadCommunicate = new Thread(RunThread);
                m_threadCommunicate.Start();
            }
            return m_client.Connected;
        }

        void RunThread()
        {
            m_bRunThread = true;
            Thread.Sleep(5000);
            while (m_bRunThread)
            {
                Thread.Sleep(10);

                for (int n = 0; n < m_aTK4S.Count; n++)
                {
                    if (m_bRunThread == false)
                        return;
                    try
                    {
                        m_bBusy = true;
                        //m_client.UnitIdentifier = (byte)m_aTK4S[n].p_nAddress;
                        m_client.ReadInputRegisters(1000, 2);
                        if (WaitReciveOK() == false)
                        {
                            m_bBusy = false;
                        }
                    }
                    catch (Exception)
                    {
                        m_bBusy = false;
                        // Log추가
                    }
                    Thread.Sleep(m_nInterval);
                }
            }
        }

        bool WaitReciveOK()
        {
            m_swWaitRecive.Start();
            while (m_bBusy && m_swWaitRecive.ElapsedMilliseconds < m_nTimeout)
            {
                Thread.Sleep(10);
            }
            if (m_bBusy)
                return false;
            return true;
        }

        void m_client_SendDataChanged(object sender)
        {
            string strHex;
            strHex = BitConverter.ToString(m_client.sendData);
            //if (m_bUseLog) m_log.Add(m_id + "--> : " + strHex);
        }

        void m_client_ReceiveDataChanged(object sender)
        {
            try
            {
                string strHex;
                strHex = BitConverter.ToString(m_client.receiveData);
                //if (m_bUseLog) m_log.Add(m_id + "<-- : " + strHex);
                int nAddress = (int)m_client.receiveData[0];
                int nFunc = (int)m_client.receiveData[1];
                int nLength = (int)m_client.receiveData[2];
                Int16 nValue = (Int16)(m_client.receiveData[3] * 256 + m_client.receiveData[4]);
                int nDecimal = (int)(m_client.receiveData[3] * 256 + m_client.receiveData[4]);
                double dValue = nValue / ((nDecimal + 1) * 10);
                //m_aTK4S[nAddress].p_dValue = dValue;
            }
            catch (Exception)
            {

            }
            m_bBusy = false;
        }

        public void ThreadStop()
        {
            m_bRunThread = false;
            if (m_threadCommunicate != null)
                m_threadCommunicate.Join();
        }

        public void AddModule()
        {
            p_aTK4S.Add(new TK4S(p_aTK4S.Count));
            SaveModule();
        }

        public void RemoveModule()
        {
            p_aTK4S.RemoveAt(p_aTK4S.Count - 1);
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
            var viewModel = m_SelectedTK4S;
            Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            if (result.HasValue)
            {
                if (result.Value)
                {
                }
            }
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
    public class TK4S : ObservableObject, IDialogRequestClose
    {
        string m_sID = "";
        int m_nAddress = 0;
        int m_nDecimalPoint = 0;
        double m_dValue = 0;
        double m_dMaxValue = 0;
        double m_dMinValue = 0;

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

        public int p_nAddress
        {
            get
            {
                return m_nAddress;
            }
            set
            {
                SetProperty(ref m_nAddress, value);
            }
        }

        public int p_nDecimalPoint
        {
            get
            {
                return m_nDecimalPoint;
            }
            set
            {
                SetProperty(ref m_nDecimalPoint, value);
            }
        }

        public double p_dValue
        {
            get
            {
                return m_dValue;
            }
            set
            {
                SetProperty(ref m_dValue, value);
            }
        }
        public double p_dMaxValue
        {
            get
            {
                return m_dMaxValue;
            }
            set
            {
                SetProperty(ref m_dMaxValue, value);
            }
        }
        public double p_dMinValue
        {
            get
            {
                return m_dMaxValue;
            }
            set
            {
                SetProperty(ref m_dMinValue, value);
            }
        }

        public TK4S()
        {
        }

        public TK4S(int nAddress)
        {
            //p_nAddress = nAddress;
            //p_sID = nAddress.ToString();
        }

        [field: NonSerialized]
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

}
