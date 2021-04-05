using EasyModbus;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Controls;

namespace RootTools.Comm
{
    public class Modbus : NotifyProperty, ITool, IComm
    {
        #region Property
        public string p_id { get; set; }
        string _sInfo = "";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_commLog.Add(CommLog.eType.Info, value);
            }
        }

        public bool p_bConnect
        {
            get { return m_client.Connected; }
            set { }
        }

        public UserControl p_ui
        {
            get
            {
                Modbus_UI ui = new Modbus_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Client
        public ModbusClient m_client = new ModbusClient();
        void InitClient()
        {
            m_client.SerialPort = "COM50";
            m_client.Baudrate = 9600;
            m_client.Parity = Parity.None;
            m_client.StopBits = StopBits.Two;
            m_client.IPAddress = "192.0.0.1";
            m_client.Port = 7700; 
        }
        #endregion

        #region Comm Type
        enum eComm
        {
            RS232,
            TCPIP
        }
        eComm _eComm = eComm.RS232;
        eComm p_eComm
        {
            get { return _eComm; }
            set
            {
                _eComm = value;
                m_reg.Write("p_eComm", (int)value);
            }
        }
        Registry m_reg;
        void InitCommType(string id)
        {
            m_reg = new Registry(id + ".Comm");
            p_eComm = (eComm)m_reg.Read("p_eComm", p_eComm);
        }

        void RunTreeCommunication(Tree tree)
        {
            p_eComm = (eComm)tree.Set(p_eComm, p_eComm, "Type", "Communication Type"); 
        }
        #endregion

        #region RS232

        string oldSerialPort = "";
        int oldBaudrate = 0;
        Parity oldParity = Parity.None;
        StopBits oldStopBits = StopBits.None;
        void RunTreeRS232(Tree tree)
        {
            if (p_eComm != eComm.RS232) return;
            if (m_client.SerialPort == null) m_client.SerialPort = "COM50";
            lock (m_csLock)
            {
                try
                {
                    m_client.SerialPort = tree.Set(m_client.SerialPort, m_client.SerialPort, "Port ID", "RS232 Port Name (COM5)");
                    m_client.Baudrate = tree.Set(m_client.Baudrate, m_client.Baudrate, "Baud Rate", "Baud Rate (bit/sec), 9600, 19200, 38400 ...");
                    m_client.Parity = (Parity)tree.Set(m_client.Parity, m_client.Parity, "Parity", "Parity");
                    m_client.StopBits = (StopBits)tree.Set(m_client.StopBits, m_client.StopBits, "Stop Bit", "Stop Bit");
                }
                catch (Exception e) { p_sInfo = e + " Modbus RS232 Error"; }
                if (tree.m_bUpdated && m_client.Connected && ((oldSerialPort != m_client.SerialPort) || (oldBaudrate != m_client.Baudrate) || (oldParity != m_client.Parity) || (oldStopBits != m_client.StopBits)))
                {
                    m_client.Disconnect();
                    System.Threading.Thread.Sleep(3000);
                    m_client.Connect();
                }
            }
            oldSerialPort = m_client.SerialPort;
            oldBaudrate = m_client.Baudrate;
            oldParity = m_client.Parity;
            oldStopBits = m_client.StopBits;
        }
        #endregion

        #region TCPIP
        void RunTreeTCPIP(Tree tree)
        {
            if (p_eComm != eComm.TCPIP) return;
            if (m_client.IPAddress == null) m_client.IPAddress = "192.0.0.1";
            m_client.IPAddress = tree.Set(m_client.IPAddress, m_client.IPAddress, "IP", "IP Address");
            m_client.Port = tree.Set(m_client.Port, m_client.Port, "Port", "Port numver");
            if (tree.m_bUpdated && m_client.Connected) m_client.Disconnect();
        }
        #endregion

        #region Connect
        public string Connect()
        {
            if (m_client.Connected) return "OK";
            try 
            { 
                switch (p_eComm)
                {
                    case eComm.RS232: m_client.IPAddress = null; break;
                    case eComm.TCPIP: m_client.SerialPort = null; break; 
                }
                m_client.Connect(); 
            }
            catch (Exception) { return "Connection Error"; }
            if (m_client.Connected == false) return "Connection False";
            m_client.SendDataChanged += M_client_SendDataChanged;
            m_client.ReceiveDataChanged += M_client_ReceiveDataChanged;
            OnPropertyChanged("p_bConnect");
            p_sInfo = "Connect OK"; 
            return "OK";
        }

        bool m_bLogSend = false; 
        private void M_client_SendDataChanged(object sender)
        { 
            if (m_bLogSend) m_commLog.Add(CommLog.eType.Send, BitConverter.ToString(m_client.sendData));
        }

        bool m_bLogReceive = false; 
        private void M_client_ReceiveDataChanged(object sender)
        {
           if (m_bLogReceive) m_commLog.Add(CommLog.eType.Receive, BitConverter.ToString(m_client.receiveData)); 
          }

        void RunTreeLog(Tree tree)
		{
            m_bLogSend = tree.Set(m_bLogSend, m_bLogSend, "Send", "Send Log Enable");
            m_bLogReceive = tree.Set(m_bLogReceive, m_bLogReceive, "Receive", "Receive Log Enable");

        }
        #endregion

        #region Send
        public string Send(string sMsg)
        {
            return "Not Enable"; 
        }
        #endregion

        #region Protocol
        static readonly object m_csLock = new object();
        static readonly object m_csLockFFU = new object();

        public string ReadCoils(byte nUnit, int nAddress, ref bool bOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {
                bool[] abRead = m_client.ReadCoils(nAddress, 1);
                if (abRead.Length > 0) bOn = abRead[0];
            }
            catch { }
            return "OK"; 
        }

        public string ReadCoils(byte nUnit, int nAddress, List<bool> abOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {
                bool[] abRead = m_client.ReadCoils(nAddress, abOn.Count);
                for (int n = 0; n < Math.Min(abRead.Length, abOn.Count); n++) abOn[n] = abRead[n];
            }
            catch { }
            return "OK"; 
        }

        public string WriteCoils(byte nUnit, int nAddress, bool bOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try { m_client.WriteSingleCoil(nAddress, bOn); }
            catch { }
            return "OK";
        }

        public string WriteCoils(byte nUnit, int nAddress, List<bool> abOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try { m_client.WriteMultipleCoils(nAddress, abOn.ToArray()); }
            catch { }
            return "OK";
        }

        public string ReadDiscreateInputs(byte nUnit, int nAddress, ref bool bOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {
                bool[] abRead = m_client.ReadDiscreteInputs(nAddress, 1);
                if (abRead.Length > 0) bOn = abRead[0];
            }
            catch { }
            return "OK";
        }

        public string ReadDiscreateInputs(byte nUnit, int nAddress, List<bool> abOn)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {
                bool[] abRead = m_client.ReadDiscreteInputs(nAddress, abOn.Count);
                for (int n = 0; n < Math.Min(abRead.Length, abOn.Count); n++) abOn[n] = abRead[n];
            }
            catch { }
            return "OK";
        }

        public string ReadHoldingRegister(byte nUnit, int nAddress, ref int nValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {

                int[] anRead = m_client.ReadHoldingRegisters(nAddress, 1);
                if (anRead.Length > 0) nValue = anRead[0];
            }
            catch(Exception ex) { p_sInfo = ex.ToString(); }
            return "OK";
        }

        public string ReadHoldingRegister(byte nUnit, int nAddress, List<int> anValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID"; 
            m_client.UnitIdentifier = nUnit;
            try
			{
				int[] anRead = m_client.ReadHoldingRegisters(nAddress, anValue.Count);
				for (int n = 0; n < Math.Min(anRead.Length, anValue.Count); n++) anValue[n] = anRead[n];
			}
			catch (Exception ex) 
            {
                string sTemp = string.Format("{0}", ex.ToString());
            }
			return "OK";
        }

        public string WriteHoldingRegister(byte nUnit, int nAddress, int nValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try { m_client.WriteSingleRegister(nAddress, nValue); }
            catch { }
            return "OK";
        }

        public string WriteHoldingRegister(byte nUnit, int nAddress, List<int> anValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try { m_client.WriteMultipleRegisters(nAddress, anValue.ToArray()); }
            catch { }
            return "OK";
        }
        public string ReadInputRegister(byte nUnit, int nAddress, ref int nValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            try
            {
                lock (m_csLock)
                {
                    int[] anRead = m_client.ReadInputRegisters(nAddress, 1);
                    if (anRead.Length > 0) nValue = anRead[0];
                }
            }
            catch (Exception e) { m_log.Error("ReadInputRegister Error : " + e.Message); }
            return "OK";
        }

        public string ReadInputRegister(byte nUnit, int nAddress, List<int> anValue)
        {
            if (m_client.Connected == false) return "Not Connect";
            if (nUnit == 0) return "Invalid Unit ID";
            m_client.UnitIdentifier = nUnit;
            int[] anRead = m_client.ReadInputRegisters(nAddress, anValue.Count);
            for (int n = 0; n < Math.Min(anRead.Length, anValue.Count); n++) anValue[n] = anRead[n];
            return "OK";
        }
        #endregion

        #region Data for UI Display
        byte _nViewUnit = 1;
        public byte p_nViewUnit
        {
            get { return _nViewUnit; }
            set
            {
                _nViewUnit = value;
                OnPropertyChanged();
            }
        }

        public class DataGroup : NotifyProperty
        {
            #region Property
            public enum eType
            {
                Coil,
                DiscreateInput,
                HoldingRegister,
                InputRegister
            }
            public eType p_eType { get; set; }
            
            bool _bUse = false; 
            public bool p_bUse
            {
                get { return _bUse; }
                set
                {
                    _bUse = value;
                    OnPropertyChanged(); 
                }
            }

            int _nStart = 0;
            public int p_nStart
            {
                get { return _nStart; }
                set
                {
                    _nStart = value;
                    ReAllocate();
                    OnPropertyChanged(); 
                }
            }

            int _nEnd = 1;
            public int p_nEnd
            {
                get { return _nEnd; }
                set
                {
                    _nEnd = value;
                    ReAllocate();
                    OnPropertyChanged();
                }
            }
            #endregion

            public class Data : NotifyProperty
            {
                public int p_nAddress { get; set; }

                int _nData = 0; 
                public int p_nData
                {
                    get { return _nData; }
                    set
                    {
                        _nData = value;
                        OnPropertyChanged(); 
                    }
                }

                public Data(int nAddress)
                {
                    p_nAddress = nAddress; 
                }
            }
            public List<bool> m_abData = new List<bool>();
            public List<int> m_anData = new List<int>(); 
            public ObservableCollection<Data> m_aData = new ObservableCollection<Data>(); 
            void ReAllocate()
            {
                m_aData.Clear();
                m_abData.Clear();
                m_anData.Clear();
                for (int n = p_nStart; n <= p_nEnd; n++)
                {
                    m_aData.Add(new Data(n));
                    switch (p_eType)
                    {
                        case eType.Coil:
                        case eType.DiscreateInput: m_abData.Add(false); break;
                        case eType.HoldingRegister:
                        case eType.InputRegister: m_anData.Add(0); break;
                    }
                }
            }

            public void ReadData(byte nUnit)
            {
                if (p_bUse == false) return; 
                switch (p_eType)
                {
                    case eType.Coil: 
                        m_modbus.ReadCoils(nUnit, p_nStart, m_abData);
                        for (int n = 0; n < m_aData.Count; n++) m_aData[n].p_nData = m_abData[n] ? 1 : 0;
                        break;
                    case eType.DiscreateInput: 
                        m_modbus.ReadDiscreateInputs(nUnit, p_nStart, m_abData);
                        for (int n = 0; n < m_aData.Count; n++) m_aData[n].p_nData = m_abData[n] ? 1 : 0;
                        break;
                    case eType.HoldingRegister: 
                        m_modbus.ReadHoldingRegister(nUnit, p_nStart, m_anData);
                        for (int n = 0; n < m_aData.Count; n++) m_aData[n].p_nData = m_anData[n];
                        break;
                    case eType.InputRegister: 
                        m_modbus.ReadInputRegister(nUnit, p_nStart, m_anData);
                        for (int n = 0; n < m_aData.Count; n++) m_aData[n].p_nData = m_anData[n];
                        break; 
                }
            }

            Modbus m_modbus; 
            public DataGroup(eType eType, Modbus modbus)
            {
                p_eType = eType;
                m_modbus = modbus; 
            }
        }
        public List<DataGroup> m_aDataGroup = new List<DataGroup>(); 
        void InitDataGroup()
        {
            m_aDataGroup.Add(new DataGroup(DataGroup.eType.Coil, this));
            m_aDataGroup.Add(new DataGroup(DataGroup.eType.DiscreateInput, this));
            m_aDataGroup.Add(new DataGroup(DataGroup.eType.HoldingRegister, this));
            m_aDataGroup.Add(new DataGroup(DataGroup.eType.InputRegister, this));
        }
        public void ReadDataGroup(int nUnit)
        {
            foreach (DataGroup group in m_aDataGroup) group.ReadData((byte)nUnit);
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
           // RunTree(Tree.eMode.Init);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeCommunication(m_treeRoot.GetTree("Type"));
            RunTreeLog(m_treeRoot.GetTree("Log"));
            if (p_eComm == eComm.RS232)
            {
                RunTreeRS232(m_treeRoot.GetTree("RS232", p_eComm == eComm.RS232));
            }
            else if (p_eComm == eComm.TCPIP)
            {
                RunTreeTCPIP(m_treeRoot.GetTree("TCPIP", p_eComm == eComm.TCPIP));
            }
        }
        #endregion

        Log m_log;
        public CommLog m_commLog = null;
        public TreeRoot m_treeRoot;
        
        public Modbus(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_commLog = new CommLog(this, m_log);
            InitCommType(id); 
            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            InitClient();
            InitDataGroup(); 
            RunTree(Tree.eMode.RegRead);
            p_sInfo = Connect(); 
        }

        public void ThreadStop()
        {
            if (m_client.Connected) m_client.Disconnect(); 
        }
    }
}
