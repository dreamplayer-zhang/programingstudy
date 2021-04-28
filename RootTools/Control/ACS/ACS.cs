using RootTools.Trees;
using SPIIPLUSCOM660Lib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.ACS
{
    public class ACS : IToolSet, IControl
    {
        #region Axis
        void GetAxisCount()
        {
            if (p_bConnect == false) return; 
            try
            {
                string sAxis = m_channel.Transaction("?SYSINFO(13)");
                m_listAxis.m_lAxisDetect = Convert.ToInt32(sAxis.Trim());
            }
            catch (Exception e) { m_log.Error("Get Axis Count Error : " + e.Message); }
        }
        #endregion

        #region Connect
        public Channel m_channel = new Channel();
        bool m_bSimul = true; 
        string m_sIP = "10.0.0.100";
        int m_nPort = 701;

        bool _bConnect = false; 
        public bool p_bConnect
        {
            get { return _bConnect; }
            set
            {
                if (_bConnect == value) return;
                m_listAxis.m_lAxisDetect = 0;
                _bConnect = value;
                if (value)
                {
                    try
                    {
                        if (m_bSimul) m_channel.OpenCommDirect();
                        else m_channel.OpenCommEthernetTCP(m_sIP, m_nPort);
                    }
                    catch (Exception e) 
                    { 
                        m_log.Error("ACS Open Error : " + e.Message);
                        _bConnect = false;
                    }
                    GetAxisCount();
                    InitBuffer(); 
                }
                else m_channel.CloseComm();
                RunTree(Tree.eMode.Init);
            }
        }

        void RunTreeConnect(Tree tree)
        {
            m_bSimul = tree.Set(m_bSimul, m_bSimul, "Simulation", "Simulation Connect");
            m_sIP = tree.Set(m_sIP, m_sIP, "IP Address", "ACS Remote IP Address", !m_bSimul);
            m_nPort = tree.Set(m_nPort, m_nPort, "Port", "ACS Remote Port Number", !m_bSimul); 
        }
        #endregion

        #region Buffer Command
        public class Buffer
        {
            public int m_nBuffer;
            public bool m_bRun = false;

            public void CheckState()
            {
                if (m_acs.p_bConnect == false) return; 
                try { m_bRun = ((m_acs.m_channel.GetProgramState(m_nBuffer) & m_acs.m_channel.ACSC_PST_RUN) != 0); }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
            }

            public string Run()
            {
                if (m_acs.p_bConnect == false) return m_acs.p_id + " not Connected";
                if (m_bRun) return m_acs.p_id + " Buffer State == Run"; 
                try
                {
                    m_acs.m_channel.RunBuffer(m_nBuffer);
                    m_acs.m_log.Info(p_id + " Run");
                }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
                return "OK";
            }

            public string Stop()
            {
                if (m_acs.p_bConnect == false) return m_acs.p_id + " not Connected";
                try
                {
                    m_acs.m_channel.StopBuffer(m_nBuffer);
                    m_acs.m_log.Info(p_id + " Stop");
                }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
                return "OK";
            }

            string _sComment = ""; 
            public string p_sComment 
            {
                get { return _sComment; }
                set
                {
                    if (_sComment == value) return;
                    _sComment = value;
                    m_acs.m_reg.Write(p_id, value); 
                } 
            }

            public string p_id { get; set; }
            ACS m_acs = null; 
            public Buffer(ACS acs, int nBuffer)
            {
                m_acs = acs; 
                m_nBuffer = nBuffer;
                p_id = acs.p_id + ".Buffer" + nBuffer.ToString("00");
                _sComment = acs.m_reg.Read(p_id, "Comment"); 
            }
        }

        public List<Buffer> m_aBuffer = new List<Buffer>(); 
        void InitBuffer()
        {
            if (p_bConnect == false) return;
            try
            {
                string sBuffer = m_channel.Transaction("?SYSINFO(10)");
                int nBuffer = Convert.ToInt32(sBuffer.Trim());
                while (m_aBuffer.Count < nBuffer) m_aBuffer.Add(new Buffer(this, m_aBuffer.Count));
                while (m_aBuffer.Count > nBuffer) m_aBuffer.RemoveAt(m_aBuffer.Count - 1);
            }
            catch (Exception e) { m_log.Error("Get Axis Count Error : " + e.Message); }
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

        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(1);
                if (p_bConnect)
                {
                    m_dio.RunThreadCheck();
                    m_listAxis.RunThreadCheck();
                    foreach (Buffer buffer in m_aBuffer) buffer.CheckState(); 
                }
                //if(m_bThread ==false)
                //p_bConnect = true;
            }
        }
        #endregion

        #region ITool
        public string p_id { get; set; }
        #endregion

        #region IControl
        public Axis GetAxis(string id, Log log)
        {
            return m_listAxis.GetAxis(id, log);
        }

        public AxisXY GetAxisXY(string id, Log log)
        {
            return m_listAxis.GetAxisXY(id, log);
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            m_listAxis.RunTree(m_treeRoot.GetTree("Axis"));
            m_dio.RunTree(m_treeRoot);
            RunTreeConnect(m_treeRoot.GetTree("Connect")); 
        }
        public double GetAnalogData(int portnum)
        {
			double Data;
                m_channel.GetAnalogInputNT(portnum, out Data);
            return Data;
        }
        #endregion

        IEngineer m_engineer;
        public Log m_log;
        public Registry m_reg; 
        public TreeRoot m_treeRoot;
        public ACSDIO m_dio = new ACSDIO();
        public ACSListAxis m_listAxis = new ACSListAxis();

        public void Init(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            m_reg = new Registry(p_id + "BufferComment"); 
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            m_dio.Init(id + ".DIO", this);
            m_listAxis.Init(id + ".Axis", engineer, this);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            p_bConnect = true; 
            InitThread();

            // Enable Event 
            //m_channel.EnableEvent(m_channel.ACSC_INTR_ACSPL_PROGRAM);       // 프로그램 시작
            //m_channel.EnableEvent(m_channel.ACSC_INTR_ACSPL_PROGRAM_EX);    // 프로그램 시작
            //m_channel.EnableEvent(m_channel.ACSC_INTR_LOGICAL_MOTION_END);  // 논리적 모션 종료
            //m_channel.EnableEvent(m_channel.ACSC_INTR_PHYSICAL_MOTION_END); // 물리적 모션 종료
            //m_channel.EnableEvent(m_channel.ACSC_INTR_PROGRAM_END);         // 스크립트 프로그램 종료
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join(); 
            }
            m_listAxis.ThreadStop();
            m_dio.ThreadStop();
        }

    }
}
