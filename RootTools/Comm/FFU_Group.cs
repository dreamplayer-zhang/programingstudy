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
        StopBits m_eStopBit = StopBits.One;
        int m_nInterval = 1000;

        ObservableCollection<FFU> m_aFFU = new ObservableCollection<FFU>();
        public ObservableCollection<FFU> p_aFFU
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
            //GeneralFunction.WriteINIFile(sSectionFDC, sSectionPort, p_sSerialPort, m_sFilePath);
            //GeneralFunction.WriteINIFile(sSectionFDC, sSectionCount, p_aTK4S.Count.ToString(), m_sFilePath);
            //for (int i = 0; i < p_aTK4S.Count; i++)
            //{
            //    GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleName, p_aTK4S[i].p_sID, m_sFilePath);
            //    GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleAddress, p_aTK4S[i].p_nAddress.ToString(), m_sFilePath);
            //    GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleDP, p_aTK4S[i].p_nDecimalPoint.ToString(), m_sFilePath);
            //    GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMax, p_aTK4S[i].p_dMaxValue.ToString(), m_sFilePath);
            //    GeneralFunction.WriteINIFile(sSectionModule + i, sSectionModuleMin, p_aTK4S[i].p_dMinValue.ToString(), m_sFilePath);
            //}
        }

        public void LoadModule()
        {
            if (File.Exists(m_sFilePath))
            {
                //p_aTK4S.Clear();
                //p_aTK4S = new ObservableCollection<TK4S>();
                //p_sSerialPort = GeneralFunction.ReadINIFile(sSectionFDC, sSectionPort, m_sFilePath);

                //int nCount = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionFDC, sSectionCount, m_sFilePath));

                //for (int i = 0; i < nCount; i++)
                //{
                //    TK4S temp = new TK4S();
                //    temp.p_sID = GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleName, m_sFilePath);
                //    temp.p_nAddress = Convert.ToInt32(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleAddress, m_sFilePath));
                //    temp.p_nDecimalPoint = Convert.ToDouble(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleDP, m_sFilePath));
                //    temp.p_dMaxValue = Convert.ToDouble(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMax, m_sFilePath));
                //    temp.p_dMinValue = Convert.ToDouble(GeneralFunction.ReadINIFile(sSectionModule + i, sSectionModuleMin, m_sFilePath));
                //    p_aTK4S.Add(temp);
                //    p_aTK4S[p_aTK4S.Count - 1].OnDetectLimit += TK4SGroup_OnDetectLimit;
                //}
            }
        }

        public bool Init()
        {

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
                //m_client.SendDataChanged += m_client_SendDataChanged;
                //m_client.ReceiveDataChanged += m_client_ReceiveDataChanged;
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
                    for (int num = 0; num < p_aFFU.Count; num++)
                    {
                        Thread.Sleep(10);
                        List<int> aTemp = new List<int>();
                        for (int i = 0; i < p_aFFU[num].p_nNumUnit; i++)
                            aTemp.Add(0);

                        Thread.Sleep(10);
                        m_Modbus.ReadHoldingRegister(Convert.ToByte(num), 0, aTemp);
                        for (int i = 0; i < p_aFFU[num].p_nNumUnit; i++)
                            p_aFFU[num].p_aRPM[i] = aTemp[i];

                        Thread.Sleep(10);
                        m_Modbus.ReadHoldingRegister(Convert.ToByte(num), 128, aTemp);
                        for (int i = 0; i < p_aFFU[num].p_nNumUnit; i++)
                            p_aFFU[num].p_aPressure[i] = aTemp[i];
                    }
                }
                catch (Exception)
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
            //p_aTK4S.Add(new TK4S(p_aTK4S.Count + 1));
            //p_aTK4S[p_aTK4S.Count - 1].OnDetectLimit += TK4SGroup_OnDetectLimit;
            //SaveModule();
        }

        private void TK4SGroup_OnDetectLimit(string str)
        {
            OnDetectLimit(str);
        }

        public void RemoveModule()
        {
            //p_aTK4S.RemoveAt(p_aTK4S.Count - 1);
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
            //var viewModel = m_SelectedTK4S;
            //Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
            //if (result.HasValue)
            //{
            //    if (result.Value)
            //    {
            //    }
            //}
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
    public class FFU : ObservableObject, IDialogRequestClose
    {
        string m_sID = "";
        int m_nAddress = 0;
        int m_nNumUnit = 0;
        ObservableCollection<int> m_nPressure = new ObservableCollection<int>();
        ObservableCollection<int> m_nRPM = new ObservableCollection<int>();
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

        public int p_nNumUnit
        {
            get
            {
                return m_nNumUnit;
            }
            set
            {
                SetProperty(ref m_nNumUnit, value);
            }
        }

        public ObservableCollection<int> p_aRPM
        {
            get
            {
                return m_nRPM;
            }
            set
            {
                SetProperty(ref m_nRPM, value);
            }
        }

        public ObservableCollection<int> p_aPressure
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


        public FFU()
        {
        }

        public FFU(int nAddress, int nUnit)
        {
            p_nAddress = nAddress;
            p_nNumUnit = nUnit;
            //p_sID = nAddress.ToString();
        }

        [field: NonSerialized]
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

}


