using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.ParticleCounter
{
    public class ParticleCounterBase : ITool
    {
        #region UI
        public UserControl p_ui
        {
            get
            {
                ParticleCounterBase_UI ui = new ParticleCounterBase_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion 

        #region Modbus
        public Modbus m_modbus;
        void InitModbus()
        {
            m_modbus = new Modbus(p_id + ".ParticleCounter", m_log);
            p_bConnect = true; 
        }

        public bool p_bConnect
        {
            get { return m_modbus.m_client.Connected; }
            set { if (value == true) ConnectModbus(); }
        }
        
        string ConnectModbus()
        {
            if (m_modbus.m_client.Connected) return "OK";
            m_modbus.Connect();
            return m_modbus.m_client.Connected ? "OK" : "Modbus Connect Error";
        }

        int _nUnit = 1;
        byte m_nUnit = 1;
        void RunTreeModbus(Tree tree)
        {
            _nUnit = tree.Set(_nUnit, _nUnit, "Unit", "Modbus Unit #");
            m_nUnit = (byte)_nUnit;
        }
        #endregion

        #region Sample
        public class Sample
        {
            public int m_secSample = 60;
            public int m_secHold = 10;

            public Sample Clone()
            {
                Sample sample = new Sample();
                sample.m_secSample = m_secSample;
                sample.m_secHold = m_secHold;
                return sample; 
            }

            public void RunTree(Tree tree, bool bVisible)
            {
                m_secSample = tree.Set(m_secSample, m_secSample, "Sample", "Sample Time (sec)", bVisible);
                m_secHold = tree.Set(m_secHold, m_secHold, "Hold", "Hold Time (sec)", bVisible);
            }
        }
        public Sample m_sample = new Sample(); 

        public string SetSample(Sample sample)
        {
            if (Run(m_modbus.WriteCoils(m_nUnit, 8, false))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 2, sample.m_secSample))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 3, sample.m_secHold))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 4, 1))) return p_sInfo;
            return "OK";
        }
        #endregion

        #region Read Count
        public string[] m_asParticleSize = null; 
        public List<int> m_aCount = new List<int>();

        List<int> m_aRead = new List<int>(); 
        public string ReadParticleCount()
        {
            try
            {
                if (Run(m_modbus.ReadInputRegister(m_nUnit, 228, m_aRead))) return p_sInfo;
                for (int n = 0; n < m_asParticleSize.Length; n++)
                {
                    m_aRead[2 * n] &= 0xffff;
                    m_aRead[2 * n + 1] &= 0xffff;
                    int nCount = (m_aRead[2 * n] << 16) + m_aRead[2 * n + 1];
                    m_aCount[n] = nCount; 
                }
                return "OK"; 
            }
            catch (Exception e) { return "ReadParticleCount Exception : " + e.Message; }
        }
        #endregion

        #region Start Ready
        StopWatch m_swRun = new StopWatch(); 
        bool _bRun = false; 
        bool p_bRun
        {
            get
            {
                m_modbus.ReadCoils(m_nUnit, 0, ref _bRun);
                return _bRun;
            }
            set 
            { 
                m_modbus.WriteCoils(m_nUnit, 0, value);
                m_swRun.Start(); 
            }
        }

        bool _bDone = false; 
        bool p_bDone
        {
            get
            {
                m_modbus.ReadCoils(m_nUnit, 1, ref _bDone);
                return _bDone;
            }
            set 
            { 
                m_modbus.WriteCoils(m_nUnit, 1, value); 
            }
        }

        string WaitDone(Sample sample)
        {
            int msTimeout = 1000 * (sample.m_secSample + sample.m_secHold + 10); 
            while (m_swRun.ElapsedMilliseconds < msTimeout)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_bDone) return "OK";
            }
            return "Read Data Timeout";
        }
        #endregion

        #region Run
        Sample m_sampleRun; 
        public string StartRun(Sample sample)
        {
            m_sampleRun = sample;
            if (IsBusy()) return "Busy"; 
            if (Run(ConnectModbus())) return p_sInfo;
            m_bgw.RunWorkerAsync(); 
            return "OK";
        }

        BackgroundWorker m_bgw = new BackgroundWorker();
        void InitBackgroundWorker()
        {
            m_bgw.DoWork += M_bgw_DoWork;
            m_bgw.RunWorkerCompleted += M_bgw_RunWorkerCompleted;
        }

        private void M_bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            p_sInfo = "Start Run";
            p_bRun = false;
            if (Run(SetSample(m_sampleRun))) return;
            Thread.Sleep(10);
            p_bRun = true;
            Thread.Sleep(1000 * m_sampleRun.m_secHold);
            p_bDone = false;
            Thread.Sleep(1000 * (m_sampleRun.m_secSample + 5));
            if (Run(WaitDone(m_sampleRun))) return;
            if (Run(ReadParticleCount())) return;
            p_bDone = false;
            Thread.Sleep(10);
            p_bRun = false;
            p_sInfo = "OK";
        }

        private void M_bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        public bool IsBusy()
        {
            return m_bgw.IsBusy; 
        }

        public void Reset()
        {
            p_bRun = false;
            p_bDone = false;
        }

        public string p_sInfo { get; set; }
        bool Run(string sInfo)
        {
            p_sInfo = sInfo;
            return sInfo != "OK";
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeModbus(m_treeRoot.GetTree("Modbus"));
            m_sample.RunTree(m_treeRoot.GetTree("Sample"), true); 
        }
        #endregion

        public string p_id { get; set; }
        Log m_log; 
        protected void Init(string id, Log log, string[] asParticleSize)
        {
            p_id = id;
            m_log = log;
            m_asParticleSize = asParticleSize;
            for (int n = 0; n < asParticleSize.Length; n++)
            {
                m_aCount.Add(0);
                m_aRead.Add(0);
                m_aRead.Add(0);
            }
            InitModbus();
            InitTree();
            InitBackgroundWorker();
            Reset(); 
        }

        public void ThreadStop()
        {
            m_modbus.ThreadStop(); 
        }
    }
}
