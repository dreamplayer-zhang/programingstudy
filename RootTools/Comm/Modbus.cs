using EasyModbus;
using RootTools.Trees;
using System;
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
            m_client.SendDataChanged += M_client_SendDataChanged;
            m_client.ReceiveDataChanged += M_client_ReceiveDataChanged;
            OnPropertyChanged("p_bConnect"); 
            return "OK";
        }

        private void M_client_SendDataChanged(object sender)
        {
            m_commLog.Add(CommLog.eType.Send, BitConverter.ToString(m_client.sendData));
        }

        private void M_client_ReceiveDataChanged(object sender)
        {
            m_commLog.Add(CommLog.eType.Receive, BitConverter.ToString(m_client.receiveData)); 
        }
        #endregion

        #region Send
        public string Send(string sMsg)
        {
            return "Not Enable"; 
        }
        #endregion

        #region Protocol
        public string ReadCoils(byte nUnit, int nAddress, ref bool bOn)
        {
            m_client.UnitIdentifier = nUnit;
            bool[] abRead = m_client.ReadCoils(nAddress, 1);
            if (abRead.Length > 0) bOn = abRead[0]; 
            return "OK"; 
        }

        public string ReadCoils(byte nUnit, int nAddress, ref List<bool> abOn)
        {
            m_client.UnitIdentifier = nUnit;
            bool[] abRead = m_client.ReadCoils(nAddress, abOn.Count);
            for (int n = 0; n < Math.Min(abRead.Length, abOn.Count); n++) abOn[n] = abRead[n]; 
            return "OK"; 
        }

        public void WriteCoils(byte nUnit, int nAddress, bool bOn)
        {
            m_client.UnitIdentifier = nUnit;
            m_client.WriteSingleCoil(nAddress, bOn); 
        }

        public void WriteCoils(byte nUnit, int nAddress, List<bool> abOn)
        {
            m_client.UnitIdentifier = nUnit;
            m_client.WriteMultipleCoils(nAddress, abOn.ToArray()); 
        }

        public string ReadDiscreateInputs(byte nUnit, int nAddress, ref bool bOn)
        {
            m_client.UnitIdentifier = nUnit;
            bool[] abRead = m_client.ReadDiscreteInputs(nAddress, 1);
            if (abRead.Length > 0) bOn = abRead[0];
            return "OK";
        }

        public string ReadDiscreateInputs(byte nUnit, int nAddress, ref List<bool> abOn)
        {
            m_client.UnitIdentifier = nUnit;
            bool[] abRead = m_client.ReadDiscreteInputs(nAddress, abOn.Count);
            for (int n = 0; n < Math.Min(abRead.Length, abOn.Count); n++) abOn[n] = abRead[n];
            return "OK";
        }

        public string ReadHoldingRegister(byte nUnit, int nAddress, ref int nValue)
        {
            m_client.UnitIdentifier = nUnit;
            int[] anRead = m_client.ReadHoldingRegisters(nAddress, 1);
            if (anRead.Length > 0) nValue = anRead[0];
            return "OK";
        }

        public string ReadHoldingRegister(byte nUnit, int nAddress, ref List<int> anValue)
        {
            m_client.UnitIdentifier = nUnit;
            int[] anRead = m_client.ReadHoldingRegisters(nAddress, anValue.Count);
            for (int n = 0; n < Math.Min(anRead.Length, anValue.Count); n++) anValue[n] = anRead[n];
            return "OK";
        }

        public void WriteHoldingRegister(byte nUnit, int nAddress, int nValue)
        {
            m_client.UnitIdentifier = nUnit;
            m_client.WriteSingleRegister(nAddress, nValue); 
        }

        public void WriteHoldingRegister(byte nUnit, int nAddress, List<int> anValue)
        {
            m_client.UnitIdentifier = nUnit;
            m_client.WriteMultipleRegisters(nAddress, anValue.ToArray());
        }

        public string ReadInputRegister(byte nUnit, int nAddress, ref int nValue)
        {
            m_client.UnitIdentifier = nUnit;
            int[] anRead = m_client.ReadInputRegisters(nAddress, 1);
            if (anRead.Length > 0) nValue = anRead[0];
            return "OK";
        }

        public string ReadInputRegister(byte nUnit, int nAddress, ref List<int> anValue)
        {
            m_client.UnitIdentifier = nUnit;
            int[] anRead = m_client.ReadInputRegisters(nAddress, anValue.Count);
            for (int n = 0; n < Math.Min(anRead.Length, anValue.Count); n++) anValue[n] = anRead[n];
            return "OK";
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
        }

        public void ThreadStop()
        {
            if (m_client.Connected) m_client.Disconnect(); 
        }
    }
}
