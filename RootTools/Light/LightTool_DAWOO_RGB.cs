using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_DAWOO_RGB : NotifyProperty, ILightTool
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Error(value);
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LightTool_DAWOO_RGB_UI ui = new LightTool_DAWOO_RGB_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Serial
        public enum eSerial
        {
            RS232,
            RS485,
        }
        public eSerial m_eSerial = eSerial.RS232;
        public eSerial p_eSerial
        {
            get { return m_eSerial; }
            set 
            {
                if (value == eSerial.RS232) p_nDeviceID = 0;
                m_eSerial = value;
            }
        }
        int m_nDeviceID = 0;
        public int p_nDeviceID
        {
            get { return m_nDeviceID; }
            set 
            {
                if (value >= 0 && value <= 15) m_nDeviceID = value;
                else m_nDeviceID = 0;
            }
        }

        void RunTreeSerial(Tree tree)
        {
            p_eSerial = (eSerial)tree.Set(p_eSerial, p_eSerial, "Type", "Serial Communication Type");
            p_nDeviceID = tree.Set(p_nDeviceID, p_nDeviceID, "Device ID", "Device ID (No. 0~15)", m_eSerial == eSerial.RS485);
        }
        #endregion

        #region RS232
        public RS232byte m_rs232;
        void InitRS232()
        {
            m_rs232 = new RS232byte(p_id, m_log);
            m_rs232.OnReceive += M_rs232_OnReceive;
        }

        private void M_rs232_OnReceive(byte[] aRead, int nRead)
        {
            if (m_protocolSend == null) return;
            m_protocolSend.OnReceive(aRead, nRead);
            m_protocolSend = null;
        }
        #endregion

        #region Protocol 
        //forget ========================
        public enum eCmd
        {
            GetRedPower = 0xA0,         // 0xA0
            GetGreenPower,              // 0xA1
            GetBluePower,               // 0xA2
            GetRedOnOff,                // 0xA3
            GetGreenOnOff,              // 0xA4
            GetBlueOnOff,               // 0xA5
            GetRedTemp,                 // 0xA6
            GetGreenTemp,               // 0xA7
            GetBlueTemp,                // 0xA8
            GetRedIntensityFeedback,    // 0xA9
            GetGreenIntensityFeedback,  // 0xAA
            GetBlueIntensityFeedback,   // 0xAB
            SetRedOnOff = 0xB1,         // 0xB1
            SetGreenOnOff,              // 0xB2
            SetBlueOnOff,               // 0xB3
            SetRedPower = 0xC3,         // 0xC3
            SetGreenPower,              // 0xC4
            SetBluePower,               // 0xC5
            SetErrorResponse = 0xF7,    // 0xF7
        }
        public class Protocol
        {

            bool m_bSend = false;
            public string Send()
            {
                bool bIsGetCmd = m_eCmd <= eCmd.GetBlueIntensityFeedback;

                byte[] aSend = new byte[8];
                int i = 0;
                aSend[i++] = 0xAA;                      // Head
                aSend[i++] = (byte)(bIsGetCmd ? 6 : 8); // Len
                aSend[i++] = (byte)m_nDeviceID;         // ID
                aSend[i++] = (byte)m_eCmd;              // CMD
                aSend[i++] = 0X00;                      // Parameter
                if (!bIsGetCmd)
                {
                    aSend[i++] = (byte)(m_nValue >> 8);
                    aSend[i++] = (byte)m_nValue;        // Data
                }

                // XOR Data
                byte nXor = 0;
                for (int n = 0; n < i; n++) nXor ^= aSend[n];
                aSend[i++] = nXor;
                m_rs232.Send(aSend, i);
                
                m_bSend = true;
                return "OK";
            }

            public int m_nReadValue;
            int m_nRead = 0;
            public string OnReceive(byte[] aRead, int nRead)
            {
                m_nRead = nRead;
                int i = 0;
                if (aRead[i++] != 0xAA) return "Invalid Header";
                if (aRead[i++] != 8) return "Invalid Length";
                if (aRead[i++] != m_nDeviceID) return "Invalid Device ID";
                if (aRead[i++] != (byte)m_eCmd) return m_eCmd.ToString() + " Command Error : " + aRead[i].ToString();
                i++;    // Paramter
                switch (m_eCmd)
                {
                    case eCmd.GetRedPower:
                    case eCmd.GetGreenPower:
                    case eCmd.GetBluePower:
                        m_nReadValue = (aRead[i] << 8) | aRead[i + 1];
                        m_light.SetGetPower(m_nReadValue);
                        break;
                    case eCmd.GetRedOnOff:
                    case eCmd.GetGreenOnOff:
                    case eCmd.GetBlueOnOff:
                    case eCmd.GetRedTemp:
                    case eCmd.GetGreenTemp:
                    case eCmd.GetBlueTemp:
                    case eCmd.GetRedIntensityFeedback:
                    case eCmd.GetGreenIntensityFeedback:
                    case eCmd.GetBlueIntensityFeedback:
                        m_nReadValue = (aRead[i] << 8) | aRead[i + 1];
                        break;
                }
                return "OK";
            }

            public string WaitReply(int msWait)
            {
                while (m_bSend == false)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_lightTool.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                    Thread.Sleep(10);
                }
                m_bSend = false;
                m_nRead = 0;
                StopWatch sw = new StopWatch();
                while (true)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_lightTool.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                    if (m_nRead > 0) return "OK";
                    if (sw.Elapsed.TotalMilliseconds > msWait) return "Timeover";
                    Thread.Sleep(10);
                }
            }

            public eCmd m_eCmd;
            int m_nValue;
            LightTool_DAWOO_RGB m_lightTool;
            Light m_light;
            int m_nChannel = 0;
            RS232byte m_rs232;
            int m_nDeviceID = 0;
            public Protocol(eCmd eCmd, int nValue, int nDeviceID, LightTool_DAWOO_RGB lightTool, Light light)
            {
                m_eCmd = eCmd;
                m_lightTool = lightTool;
                m_light = light;
                m_rs232 = lightTool.m_rs232;
                m_nValue = nValue;
                m_nDeviceID = nDeviceID;
            }
        }
        Protocol m_protocolSend = null;
        //forget =====================================
        #endregion

        #region Thread Send
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        bool m_bRunSend = false;
        Thread m_threadSend;
        void InitThread()
        {
            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
        }

        void RunThreadSend()
        {
            m_bRunSend = true;
            Thread.Sleep(1000);
            while (m_bRunSend)
            {
                Thread.Sleep(10);
                if ((m_protocolSend == null) && (m_qProtocol.Count > 0))
                {
                    m_protocolSend = m_qProtocol.Dequeue();
                    p_sInfo = m_protocolSend.Send();
                    Thread.Sleep(10);
                    if (p_sInfo != "OK")
                    {
                        m_protocolSend = null;
                        m_qProtocol.Clear();
                    }
                }
            }
        }

        Light m_light;
        public Protocol AddProtocol(eCmd eCmd, int nSendValue, Light light)
        {
            m_light = light;
            Protocol protocol = new Protocol(eCmd, nSendValue, m_nDeviceID, this, light);
            m_qProtocol.Enqueue(protocol);
            return protocol;
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower()
            {
                Protocol protocol = null;

                int nSendCount = 1;
                do
                {
                    switch (m_eChannel)
                    {
                        case eChannel.Red: protocol = m_lightTool.AddProtocol(eCmd.GetRedPower, 0, this); break;
                        case eChannel.Green: protocol = m_lightTool.AddProtocol(eCmd.GetGreenPower, 0, this); break;
                        case eChannel.Blue: protocol = m_lightTool.AddProtocol(eCmd.GetBluePower, 0, this); break;
                        default:
                            break;
                    }

                    if (protocol != null)
                    {
                        string sResult = protocol.WaitReply(m_msWaitReply);
                        if (sResult != "OK")
                        {
                            m_lightTool.m_protocolSend = null;
                            nSendCount++;
                        }
                        else
                            break;
                    }
                }
                while (nSendCount <= m_nResendCount);
            }

            public void SetGetPower(int nPower)
            {
                double fPower = 100.0 * nPower / c_maxPower;
                p_fGetPower = fPower / p_fScalePower;
            }

            const int c_maxPower = 4000;
            const int m_msWaitReply = 1000;
            int m_nResendCount = 3;
            public override void SetPower()
            {
                Protocol protocol = null;
                double fPower = p_bOn ? p_fSetPower : 0;
                fPower *= p_fScalePower;
                fPower = Math.Round(fPower);

                int nSendCount = 1;
                do
                {
                    switch (m_eChannel)
                    {
                        case eChannel.Red: protocol = m_lightTool.AddProtocol(eCmd.SetRedPower, (int)fPower, this); break;
                        case eChannel.Green: protocol = m_lightTool.AddProtocol(eCmd.SetGreenPower, (int)fPower, this); break;
                        case eChannel.Blue: protocol = m_lightTool.AddProtocol(eCmd.SetBluePower, (int)fPower, this); break;
                        default:
                            break;
                    }

                    if (protocol != null)
                    {
                        string sResult = protocol.WaitReply(m_msWaitReply);
                        if (sResult != "OK")
                        {
                            m_lightTool.m_protocolSend = null;
                            nSendCount++;
                        }
                        else
                            break;
                    }
                }
                while (nSendCount <= m_nResendCount);
                
            }
            protected override void GetOnOff() { }
            public override void SetOnOff()
            {
                Protocol protocol = null;
                int nVal = p_bOn ? 1 : 0;

                int nSendCount = 1;
                do
                {
                    switch (m_eChannel)
                    {
                        case eChannel.Red: protocol = m_lightTool.AddProtocol(eCmd.SetRedOnOff, nVal, this); break;
                        case eChannel.Green: protocol = m_lightTool.AddProtocol(eCmd.SetGreenOnOff, nVal, this); break;
                        case eChannel.Blue: protocol = m_lightTool.AddProtocol(eCmd.SetBlueOnOff, nVal, this); break;
                        default:
                            break;
                    }

                    if (protocol != null)
                    {
                        string sResult = protocol.WaitReply(m_msWaitReply);
                        if (sResult != "OK")
                        {
                            m_lightTool.m_protocolSend = null;
                            nSendCount++;
                        }
                        else
                            break;
                    }
                }
                while (nSendCount <= m_nResendCount);
            }

            public int m_nCh = 0;
            public eChannel m_eChannel = eChannel.Red; 
            LightTool_DAWOO_RGB m_lightTool;
            public Light(string id, eChannel eChannel, LightTool_DAWOO_RGB lightTool)
            {
                m_eChannel = eChannel;
                m_nCh = (int)eChannel; 
                m_lightTool = lightTool;
                Init(id + "." + m_nCh.ToString("00"), m_nCh);
            }
        }

        const int c_lLight = 3; 
        public enum eChannel
        {
            Red = 0,
            Green,
            Blue,
        }
        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {
            m_rs232.m_eHandshake = System.IO.Ports.Handshake.None;
            m_rs232.p_bConnect = true;
            foreach (eChannel eChannel in Enum.GetValues(typeof(eChannel)))
            {
                Light light = new Light(p_id, eChannel, this);
                p_aLight.Add(light);

                light.p_bOn = true;
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= c_lLight) return null;
            if (p_aLight[nCh].p_sID != p_aLight[nCh].p_id) return null;
            p_aLight[nCh].p_sID = sNewID; 
            if (OnChangeTool != null) OnChangeTool();
            return p_aLight[nCh];
        }

        public void Deselect(LightBase light)
        {
            light.Deselect();
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRootSetup;
        void InitTreeSetup()
        {
            m_treeRootSetup = new TreeRoot(p_id + ".Setup", m_log);
            m_treeRootSetup.UpdateTree += M_treeRootSetup_UpdateTree; ;
            RunTreeSetup(Tree.eMode.RegRead);
        }

        private void M_treeRootSetup_UpdateTree()
        {
            RunTreeSetup(Tree.eMode.Update);
            RunTreeSetup(Tree.eMode.Init);
            RunTreeSetup(Tree.eMode.RegWrite);
        }

        public void RunTreeSetup(Tree.eMode eMode)
        {
            m_treeRootSetup.p_eMode = eMode;
            RunTreeSerial(m_treeRootSetup.GetTree("Serial"));
        }
        #endregion

        IEngineer m_engineer;
        Log m_log;
        public LightTool_DAWOO_RGB(string id, IEngineer engineer)
        {
            p_aLight = new List<LightBase>();
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);

            InitThread();

            InitRS232();
            InitTreeSetup();
            InitLight();
        }

        public void ThreadStop()
        {
            if (m_bRunSend)
            {
                m_bRunSend = false;
                m_threadSend.Join();
            }
            m_rs232.ThreadStop();
        }

    }
}
