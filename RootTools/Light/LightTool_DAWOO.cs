using RootTools.Comm;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_DAWOO : NotifyProperty, ILightTool
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
                LightTool_DAWOO_UI ui = new LightTool_DAWOO_UI();
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
        public eSerial m_eSeial = eSerial.RS232;
        int m_lLight = 1;

        void RunTreeSerial(Tree tree)
        {
            m_eSeial = (eSerial)tree.Set(m_eSeial, m_eSeial, "Type", "Serial Communication Type");
            m_lLight = tree.Set(m_lLight, m_lLight, "Channel", "Channel Count", m_eSeial == eSerial.RS485);
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
            m_protocolSend.OnRecieve(aRead, nRead);
            m_protocolSend = null; 
        }
        #endregion

        #region Protocol
        public enum eCmd
        {
            Dimming = 1,
            OnOff,
            SaveDimming,
            Remote,
            GetDimming = 11,
            GetOnOff,
            GetTemp,
            GetCurrent = 16,
            GetIllumination,
        }
        public class Protocol
        {

            bool m_bSend = false;
            public string Send()
            {
                byte[] aSend = new byte[8];
                int i = 0;
                aSend[i++] = 0xff;
                aSend[i++] = (byte)m_eCmd;
                if (m_lightTool.m_eSeial == eSerial.RS485) aSend[i++] = (byte)m_nChannel;
                switch (m_eCmd)
                {
                    case eCmd.Dimming:
                        aSend[i++] = (byte)(m_nValue / 256);
                        aSend[i++] = (byte)(m_nValue % 256);
                        break;
                    case eCmd.OnOff:
                        aSend[i++] = (byte)m_nValue;
                        break;
                    case eCmd.Remote:
                        aSend[i++] = (byte)((m_nValue > 0) ? 0 : 1);
                        break;
                }
                byte nXor = 0;
                for (int n = 0; n < i; n++) nXor ^= aSend[n];
                aSend[i++] = nXor;
                m_rs232.Send(aSend, i); 
                return "OK"; 
            }

            public int m_nReadValue;
            int m_nRead = 0; 
            public string OnRecieve(byte[] aRead, int nRead)
            {
                m_nRead = nRead;
                int i = 0; 
                if (aRead[i++] != 0xff) return "Invalid Header"; 
                switch (m_eCmd)
                {
                    case eCmd.Dimming:
                    case eCmd.OnOff:
                    case eCmd.SaveDimming:
                    case eCmd.Remote:
                        if (m_lightTool.m_eSeial == eSerial.RS485) i++; 
                        if (aRead[i] != 0x06) return m_eCmd.ToString() + " Command Error : " + aRead[i].ToString();
                        break;
                    case eCmd.GetDimming:
                        if (aRead[i++] != (byte)m_eCmd) return "Invalid Command";
                        if (m_lightTool.m_eSeial == eSerial.RS485) i++;
                        int nDimming = aRead[i++];
                        m_light.SetGetPower(256 * nDimming + aRead[i]);
                        break;
                    case eCmd.GetOnOff:
                        if (aRead[i++] != (byte)m_eCmd) return "Invalid Command";
                        if (m_lightTool.m_eSeial == eSerial.RS485) i++;
                        m_nReadValue = (aRead[i] != 0) ? 1 : 0;
                        break;
                    case eCmd.GetTemp:
                        if (aRead[i++] != (byte)m_eCmd) return "Invalid Command";
                        m_nReadValue = aRead[i];
                        break;
                    case eCmd.GetCurrent:
                        if (aRead[i++] != (byte)m_eCmd) return "Invalid Command";
                        m_nReadValue = (aRead[i] != 0) ? 1 : 0;
                        break;
                    case eCmd.GetIllumination:
                        if (aRead[i++] != (byte)m_eCmd) return "Invalid Command";
                        int nIllumination = aRead[i++];
                        m_nReadValue = 256 * nIllumination + aRead[i];
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
                m_nRead = 0;
                StopWatch sw = new StopWatch();
                while (true)
                {
                    if (EQ.IsStop()) return "EQ Stop";
                    if (m_lightTool.m_rs232.p_bConnect == false) return "RS232 Connection Lost !!";
                    if (m_nRead > 0) return "OK";
                    if (sw.Elapsed.TotalSeconds > msWait) return "Timeover";
                    Thread.Sleep(10);
                }
            }

            public eCmd m_eCmd;
            int m_nValue; 
            LightTool_DAWOO m_lightTool;
            Light m_light;
            int m_nChannel = 0; 
            RS232byte m_rs232; 
            public Protocol(eCmd eCmd, int nValue, LightTool_DAWOO lightTool, Light light)
            {
                m_eCmd = eCmd;
                m_lightTool = lightTool;
                m_light = light; 
                m_rs232 = lightTool.m_rs232;
                m_nValue = nValue;
            }
        }
        Protocol m_protocolSend = null;
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
            Protocol protocol = new Protocol(eCmd, nSendValue, this, light);
            m_qProtocol.Enqueue(protocol); 
            return protocol; 
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower() 
            {
                m_lightTool.AddProtocol(eCmd.GetDimming, 0, this);
            }

            public void SetGetPower(int nPower)
            {
                double fPower = 100.0 * nPower / c_maxPower;
                p_fGetPower = fPower / p_fScalePower;
            }

            const int c_maxPower = 3500;
            const int m_msWaitReply = 2000; 
            public override void SetPower()
            {
                double fPower = p_fSetPower ;
                fPower *= p_fScalePower;
                m_lightTool.AddProtocol(eCmd.Dimming, (int)(c_maxPower * fPower / 100), this);
            }

            public int m_nCh = 0;
            LightTool_DAWOO m_lightTool; 
            public Light(string id, int nCh, LightTool_DAWOO lightTool)
            {
                m_nCh = nCh;
                m_lightTool = lightTool;
                Init(id + "." + nCh.ToString("00"), nCh);
            }
        }

        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {
            m_rs232.p_bConnect = true;
            for (int n = 0; n < m_lLight; n++)
            {
                Light light = new Light(p_id, n, this);
                p_aLight.Add(light);
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= m_lLight) return null;
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
        public LightTool_DAWOO(string id, IEngineer engineer)
        {
            p_aLight = new List<LightBase>();
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);

            InitRS232();
            InitTreeSetup(); 
            InitLight();

            InitThread(); 
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
