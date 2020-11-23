using EasyModbus;
using RootTools.Trees;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
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

        //forget
        public UserControl p_ui => throw new System.NotImplementedException();

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
                m_reg.Write("p_eComm", (int)p_eComm);
            }
        }

        Registry m_reg;
        void InitCommType(string id)
        {
            m_reg = new Registry(id + ".Comm");
            p_eComm = (eComm)m_reg.Read("p_eComm", p_eComm);
        }
        #endregion

        #region RS232
        void RunTreeRS232(Tree tree)
        {
            if (p_eComm != eComm.RS232) return;
            m_client.SerialPort = tree.Set(m_client.SerialPort, m_client.SerialPort, "Port ID", "RS232 Port Name (COM5)");
            m_client.Baudrate = tree.Set(m_client.Baudrate, m_client.Baudrate, "Baud Rate", "Baud Rate (bit/sec), 9600, 19200, 38400 ...");
            m_client.Parity = (Parity)tree.Set(m_client.Parity, m_client.Parity, "Parity", "Parity");
            m_client.StopBits = (StopBits)tree.Set(m_client.StopBits, m_client.StopBits, "Stop Bit", "Stop Bit");
            if (tree.m_bUpdated && m_client.Connected) m_client.Disconnect(); 
        }
        #endregion

        #region TCPIP
        void RunTreeTCPIP(Tree tree)
        {
            if (p_eComm != eComm.TCPIP) return;
            m_client.IPAddress = tree.Set(m_client.IPAddress, m_client.IPAddress, "IP", "IP Address");
            m_client.Port = tree.Set(m_client.Port, m_client.Port, "Port", "Port numver");
            if (tree.m_bUpdated && m_client.Connected) m_client.Disconnect();
        }
        #endregion

        #region Connect
        public string Connect()
        {
            if (m_client.Connected) return "OK"; 
            m_client.Connect();
            m_client.NumberOfRetries = 0; 
            if (m_client.Connected == false) return "Connection Error";
            m_client.ReceiveDataChanged += M_client_ReceiveDataChanged;
            return "OK";
        }

        private void M_client_ReceiveDataChanged(object sender)
        {
            byte[] aByte = m_client.receiveData; 
            
        }
        #endregion

        #region Send
        public string Send(string sMsg)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region eData
        public enum eDataType
        {
            Coils,              // bool, Read, Write
            DiscreateInputs,    // bool, Read
            HoldingRegister,    // int16, Read, Write
            InputRegister,      // int16, Read
        }

        public class Data : NotifyProperty
        {
            public eDataType m_eType;
            public byte m_nUnit; 
            public int m_nAddress; 

            public void ReadRequest()
            {
                m_modbus.m_client.UnitIdentifier = m_nUnit; 
                switch (m_eType)
                {
                    case eDataType.Coils: 
                        m_modbus.m_client.ReadCoils(m_nAddress, 1); 
                        break;
                    case eDataType.DiscreateInputs:
                        m_modbus.m_client.ReadDiscreteInputs(m_nAddress, 1);
                        break;
                    case eDataType.HoldingRegister:
                        m_modbus.m_client.ReadHoldingRegisters(m_nAddress, 1);
                        break;
                    case eDataType.InputRegister:
                        m_modbus.m_client.ReadInputRegisters(m_nAddress, 1); 
                        break;
                }
            }



            dynamic _value; 
            public dynamic p_value
            {
                get { return _value; }
                set
                {
                    if (_value == value) return;
                    _value = value;
                    OnPropertyChanged(); 
                }
            }

            public void RunTree(Tree tree)
            {
                tree.Set(m_eType, m_eType, "Type", "Modbus Data Type", true, true);
                tree.Set(m_nUnit, m_nUnit, "Unit ID", "Modbus Unit ID", true, true);
                tree.Set(m_nAddress, m_nAddress, "Address", "Modbus Address", true, true); 
            }

            Modbus m_modbus; 
            public Data(Modbus modbus, eDataType eDataType, byte nUnit, int nAddress)
            {
                m_modbus = modbus;
                m_eType = eDataType;
                m_nUnit = nUnit;
                m_nAddress = nAddress; 
                switch (m_eType)
                {
                    case eDataType.Coils:
                    case eDataType.DiscreateInputs:
                        p_value = false;
                        break;
                    case eDataType.HoldingRegister:
                    case eDataType.InputRegister:
                        p_value = (int)0;
                        break; 
                }
            }
        }
        List<Data> m_aData = new List<Data>(); 
        public Data GetData(eDataType eDataType, byte nUnit, int nAddress)
        {
            Data data = new Data(this, eDataType, nUnit, nAddress);
            m_aData.Add(data);
            return data;
        }
        #endregion

        #region Thread
        bool m_bThread = false; 
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        Data m_dataRead = null; 
        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            int nIndex = 0; 
            while (m_bThread)
            {
                Thread.Sleep(1);
                if (m_dataRead == null)
                {
                    if (m_aData.Count >= nIndex) nIndex = 0;
                    m_dataRead = m_aData[nIndex];
                    m_dataRead.ReadRequest();
                    nIndex++;
                }
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeRS232(m_treeRoot.GetTree("RS232"));
            RunTreeTCPIP(m_treeRoot.GetTree("TCPIP"));
            //            RunProtocolTree(m_treeRoot.GetTree("Protocol"));
            //            RunTimeoutTree(m_treeRoot.GetTree("Timeout"));
        }
        #endregion

        Log m_log;
        public CommLog m_commLog = null;
        public TreeRoot m_treeRoot;
        ModbusClient m_client = new ModbusClient();
        public Modbus(string id, Log log)
        {
            p_id = id;
            m_log = log;
            m_commLog = new CommLog(this, m_log);
            InitCommType(id); 
            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
            InitThread(); 
        }

        public void ThreadStop()
        {
            if (m_client.Connected) m_client.Disconnect(); 
        }
    }
}
