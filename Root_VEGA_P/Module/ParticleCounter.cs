using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class ParticleCounter : ModuleBase
    {
        #region ToolBox
        RS232 m_rs232;
        Modbus m_modbus;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_modbus, this, "Modbus");
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
                ConnectModbus(); 
            }
        }
        #endregion

        #region RS232
        string ConnectRS232()
        {
            m_rs232.p_bConnect = true;
            return m_rs232.p_bConnect ? "OK" : "RS232 Connect Error"; 
        }

        private void M_rs232_OnReceive(string sRead)
        {
            //forget
        }
        #endregion

        #region Vacuum Pump
        public string RunPump(bool bOn)
        {
            if (Run(ConnectRS232())) return p_sInfo;
            //RunPump
            return "OK"; 
        }
        #endregion

        #region Modbus
        string ConnectModbus()
        {
            if (m_modbus.m_client.Connected) return "OK"; 
            p_sInfo = m_modbus.Connect();
            return m_modbus.m_client.Connected ? "OK" : "Modbus Connect Error";
        }
        #endregion

        #region Property
        int _nUnit = 1; 
        byte m_nUnit = 1; 
        void RunTreeProperty(Tree tree)
        {
            _nUnit = tree.Set(_nUnit, _nUnit, "Unit", "Modbus Unit #");
            m_nUnit = (byte)_nUnit; 
        }
        #endregion

        #region Sample
        int m_secSample = 1;
        int m_secHold = 1;
        int m_nRepeat = 3; 

        string SetSample()
        {
            if (Run(m_modbus.WriteCoils(m_nUnit, 09, false))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 40003, m_secSample))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 40004, m_secHold))) return p_sInfo;
            if (Run(m_modbus.WriteHoldingRegister(m_nUnit, 40005, m_nRepeat))) return p_sInfo;
            return "OK";
        }

        void RunTreeSample(Tree tree)
        {
            m_secSample = tree.Set(m_secSample, m_secSample, "Sample", "Sample Time (sec)");
            m_secHold = tree.Set(m_secHold, m_secHold, "Hold", "Hold Time (sec)");
            m_nRepeat = tree.Set(m_nRepeat, m_nRepeat, "Repeat", "Repeat Count");
        }
        #endregion

        #region Count
        List<int> m_aRead = new List<int>();
        ObservableCollection<int> p_aCount { get; set; }
        void InitCount()
        {
            p_aCount = new ObservableCollection<int>();
            for (int n = 0; n < 16; n++) m_aRead.Add(0);
            for (int n = 0; n < 8; n++) p_aCount.Add(0); 
        }

        string ReadParticle()
        {
            try
            {
                if (Run(RunPump(true))) return p_sInfo;
                if (Run(ConnectModbus())) return p_sInfo;
                if (Run(SetSample())) return p_sInfo;
                p_bDone = false;
                Thread.Sleep(10);
                if (Run(m_modbus.WriteCoils(m_nUnit, 1, true))) return p_sInfo;
                m_sw.Start();
                Thread.Sleep(200);
                if (Run(WaitDone())) return p_sInfo;
                if (Run(m_modbus.ReadInputRegister(m_nUnit, 30229, m_aRead))) return p_sInfo;
                return "OK";
            }
            finally { RunPump(false); }
        }

        public bool p_bDone
        {
            get
            {
                bool bDone = false;
                m_modbus.ReadCoils(m_nUnit, 2, ref bDone);
                return bDone;
            }
            set
            {
                m_modbus.WriteCoils(m_nUnit, 2, value);
            }
        }

        StopWatch m_sw = new StopWatch(); 
        string WaitDone()
        {
            int msTimeout = 1000 * m_nRepeat * (m_secHold + m_secSample); 
            while (m_sw.ElapsedMilliseconds < msTimeout)
            {
                Thread.Sleep(100); 
                if (EQ.IsStop()) return "EQ Stop";
                if (p_bDone) return "OK"; 
            }
            return "Read Data Timeout"; 
        }

        void RunTreeCount(Tree tree)
        {
            for (int n = 0; n < 8; n++) 
            {
                tree.Set(p_aCount[n], p_aCount[n], n.ToString(), "Particle Count", true, true);
            }
        }
        #endregion

        #region Background
        BackgroundWorker m_bgw = new BackgroundWorker(); 
        void InitBackgroundWorker()
        {
            m_bgw.DoWork += M_bgw_DoWork;
            m_bgw.RunWorkerCompleted += M_bgw_RunWorkerCompleted;
        }

        public string StartReadParticle()
        {
            m_bgw.RunWorkerAsync();
            return "OK"; 
        }

        string m_sReadParticle = ""; 
        private void M_bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            m_sReadParticle = ReadParticle(); 
        }

        private void M_bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_sReadParticle != "OK") return; 
            for (int n = 0; n < 8; n++)
            {
                p_aCount[n] = 65536 * m_aRead[2 * n] + m_aRead[2 * n + 1]; //forget018 높음 낮음 ??
            }
            RunTree(Tree.eMode.Init); 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeProperty(tree.GetTree("Property"));
            RunTreeSample(tree.GetTree("Sample"));
            RunTreeCount(tree.GetTree("Count")); 
        }
        #endregion

        public ParticleCounter(string id, IEngineer engineer)
        {
            InitCount(); 
            p_id = id;
            InitBase(id, engineer);
            InitBackgroundWorker(); 
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Pump(this), false, "Run Vacuum Pump");
            AddModuleRunList(new Run_Run(this), false, "Run Particle Counter");
        }

        public class Run_Delay : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Delay(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_secDelay = 1; 
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)"); 
            }

            public override string Run()
            {
                Thread.Sleep(1000 * m_secDelay); 
                return "OK";
            }
        }

        public class Run_Pump : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Pump(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_secDelay = 1;
            public override ModuleRunBase Clone()
            {
                Run_Pump run = new Run_Pump(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)");
            }

            public override string Run()
            {
                if (m_module.Run(m_module.RunPump(true))) return p_sInfo; 
                Thread.Sleep(1000 * m_secDelay);
                if (m_module.Run(m_module.RunPump(false))) return p_sInfo;
                return "OK";
            }
        }

        public class Run_Run : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Run(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                m_module.StartReadParticle();
                return m_module.WaitDone(); 
            }
        }
        #endregion
    }
}
