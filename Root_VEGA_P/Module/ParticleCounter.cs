using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
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
        Modbus m_mdCounter;
        Modbus m_mbFlow; 
        public override void GetTools(bool bInit)
        {
            for (int n = 0; n < m_aSolValve.Count; n++)
            {
                p_sInfo = m_toolBox.GetDIO(ref m_aSolValve[n].m_do, this, m_aSolValve[n].m_id);
            }
            p_sInfo = m_toolBox.GetComm(ref m_mdCounter, this, "Modbus");
            p_sInfo = m_toolBox.GetComm(ref m_rs232, this, "RS232");
            p_sInfo = m_toolBox.GetComm(ref m_mbFlow, this, "Flow"); 
            if (bInit)
            {
                m_rs232.OnReceive += M_rs232_OnReceive;
                m_rs232.p_bConnect = true;
                ConnectModbus(m_mdCounter);
                ConnectModbus(m_mbFlow);
            }
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

        #region SolValve
        public class SolValve
        {
            public DIO_O m_do = null;
            public string m_id = "";
            public string p_sName { get; set; }

            public void Write(bool bOpen)
            {
                m_do?.Write(bOpen); 
            }

            public SolValve(string id)
            {
                m_id = id;
                p_sName = id; 
            }
        }
        List<SolValve> m_aSolValve = new List<SolValve>(); 

        Registry m_reg; 
        void InitSolValve(string id)
        {
            m_reg = new Registry(id);
            p_nSolValve = m_reg.Read("nSolValve", 1);
            p_nSolValveSet = m_reg.Read("nSolValveSet", 1);
        }

        int _nSolValve = 0; 
        public int p_nSolValve
        {
            get { return _nSolValve; }
            set
            {
                if (_nSolValve == value) return;
                _nSolValve = value;
                m_aSolValve.Clear(); 
                for (int n = 0; n < value; n++)
                {
                    m_aSolValve.Add(new SolValve("SolValve " + n.ToString("00")));
                }
            }
        }

        void RunTreeSolValve(Tree tree)
        {
            p_nSolValve = tree.Set(p_nSolValve, p_nSolValve, "Count", "SolValve Count");
            Tree treeSol = tree.GetTree("Name");
            foreach (SolValve sol in m_aSolValve) sol.p_sName = treeSol.Set(sol.p_sName, sol.p_sName, sol.m_id, "Solvalve Name"); 
        }
        #endregion

        #region SolValue Set
        public class SolValveSet
        {
            public string m_id;
            public string p_sName { get; set; }
            List<bool> m_aSet = new List<bool>();

            public void RunTree(Tree tree)
            {
                p_sName = tree.Set(p_sName, p_sName, "Name", "SolValve Set Name"); 
                for (int n = 0; n < m_aSet.Count; n++)
                {
                    m_aSet[n] = tree.Set(m_aSet[n], m_aSet[n], n.ToString("00") + "." + m_pc.m_aSolValve[n].p_sName, "SolValve Set On/Off"); 
                }
            }

            public string Run()
            {
                for (int n = 0; n < m_aSet.Count; n++)
                {
                    m_pc.m_aSolValve[n].Write(m_aSet[n]); 
                }
                return "OK"; 
            }

            ParticleCounter m_pc = null; 
            public SolValveSet(string id, ParticleCounter pc)
            {
                m_id = id;
                p_sName = id;
                m_pc = pc;
                m_aSet.Clear(); 
                for (int n = 0; n < pc.m_aSolValve.Count; n++) m_aSet.Add(false); 
            }
        }
        List<SolValveSet> m_aSolValveSet = new List<SolValveSet>(); 
        public List<string> p_asSolValveSet
        {
            get
            {
                List<string> asSet = new List<string>();
                foreach (SolValveSet set in m_aSolValveSet) asSet.Add(set.p_sName);
                return asSet; 
            }
        }

        string RunSolValveSet(string sSolValveSet)
        {
            foreach (SolValveSet set in m_aSolValveSet)
            {
                if (set.p_sName == sSolValveSet) return set.Run(); 
            }
            return "Invalid SolValveSet Name"; 
        }

        int _nSolValveSet = 0;
        public int p_nSolValveSet
        {
            get { return _nSolValveSet; }
            set
            {
                if (_nSolValveSet == value) return;
                _nSolValveSet = value;
                m_aSolValveSet.Clear();
                for (int n = 0; n < value; n++)
                {
                    m_aSolValveSet.Add(new SolValveSet("SolValveSet " + n.ToString("00"), this));
                }
            }
        }

        void RunTreeSolValveSet(Tree tree)
        {
            p_nSolValveSet = tree.Set(p_nSolValveSet, p_nSolValveSet, "Count", "SolValveSet Count");
            foreach (SolValveSet sol in m_aSolValveSet) sol.RunTree(tree.GetTree(sol.p_sName, false)); 
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
        }
        #endregion

        #region Pressure
        int _nPump = 1000; 
        public int p_nPump
        {
            get { return _nPump; }
            set
            {
                _nPump = value;
                RunPump(value);
                OnPropertyChanged();
            }
        }

        string RunPump(int nPump)
        {
            if (Run(ConnectRS232())) return p_sInfo;
            m_rs232.Send("SET " + nPump.ToString());
            return "OK";
        }
        
        List<int> m_aPressure = new List<int>();
        double _fPressure = 0;
        public double p_fPressure
        {
            get { return _fPressure; }
            set
            {
                _fPressure = value;
                OnPropertyChanged();
            }
        }

        string ReadPressure()
        {
            if (Run(m_mdCounter.ReadInputRegister(m_nUnit, 221, m_aPressure))) return p_sInfo;
            p_fPressure = (((m_aPressure[0] & 0xffff) << 16) + (m_aPressure[1] & 0xffff)) / 1000.0;
            return "OK";
        }
        #endregion

        #region Set Pressure
        const double c_fPressureSet = 0.02; 
        public string RunSetPressure(bool bPumpOn)
        {
            if (Run(RunPump(p_nPump))) return p_sInfo;
            if (Run(ConnectModbus(m_mdCounter))) return p_sInfo;
            if (Run(m_mdCounter.WriteCoils(m_nUnit, 8, false))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 2, 1))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 3, 0))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 4, 0))) return p_sInfo;
            if (Run(m_mdCounter.WriteCoils(m_nUnit, 0, true))) return p_sInfo;
            Thread.Sleep(2000);
            try
            {
                int dV = 8; 
                while (EQ.IsStop() == false)
                {
                    Thread.Sleep(1000);
                    ReadPressure();
                    p_sInfo = "p_fPressure = " + p_fPressure.ToString() + ", " + p_nPump.ToString(); 
                    if (p_fPressure < 0.2) return "Pressure Too Low"; 
                    if (Math.Abs(p_fPressure - 1) < c_fPressureSet) return "OK";
                    if (p_fPressure < 1)
                    {
                        if (dV < 0) dV = -dV - 1;
                    }
                    else
                    {
                        if (dV > 0) dV = -dV;
                    }
                    p_nPump += dV;
                    if (dV == 0) return "Set Pressure Error"; 
                }
                return "EQ Stop";
            }
            finally
            {
                if (bPumpOn == false) RunPump(0); 
                m_mdCounter.WriteCoils(m_nUnit, 0, false);
            }
        }
        #endregion

        #region Modbus
        string ConnectModbus(Modbus modbus)
        {
            if (modbus.m_client.Connected) return "OK";
            p_sInfo = modbus.Connect();
            return modbus.m_client.Connected ? "OK" : "Modbus Connect Error";
        }
        #endregion

        #region Sample
        int m_secSample = 1;
        int m_secHold = 1;
        int m_nRepeat = 3;

        string SetSample()
        {
            if (Run(m_mdCounter.WriteCoils(m_nUnit, 8, false))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 2, m_secSample))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 3, m_secHold))) return p_sInfo;
            if (Run(m_mdCounter.WriteHoldingRegister(m_nUnit, 4, m_nRepeat))) return p_sInfo;
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
            for (int n = 0; n < 2 * c_lParticle; n++) m_aRead.Add(0);
            for (int n = 0; n < c_lParticle; n++) p_aCount.Add(0);
            m_aPressure.Add(0);
            m_aPressure.Add(0);
        }

        string ReadParticle()
        {
            try
            {
                if (Run(m_mdCounter.WriteCoils(m_nUnit, 0, false))) return p_sInfo;
                p_bDone = false;
                if (Run(RunPump(p_nPump))) return p_sInfo;
                if (Run(ConnectModbus(m_mdCounter))) return p_sInfo;
                if (Run(SetSample())) return p_sInfo;
                Thread.Sleep(10);
                if (Run(m_mdCounter.WriteCoils(m_nUnit, 0, true))) return p_sInfo;
                m_sw.Start();
                Thread.Sleep(1000);
                if (Run(WaitDone())) return p_sInfo;
                if (Run(m_mdCounter.ReadInputRegister(m_nUnit, 228, m_aRead))) return p_sInfo;
                for (int n = 0; n < 2* c_lParticle; n++) m_aRead[n] &= 0xffff;
                return "OK";
            }
            finally
            {
                RunPump(0);
                p_bDone = false;
            }
        }

        string StartRead(bool bStart)
        {
            if (Run(ConnectModbus(m_mdCounter))) return p_sInfo;
            if (Run(m_mdCounter.WriteCoils(m_nUnit, 0, false))) return p_sInfo;
            p_bDone = false;
            if (Run(SetSample())) return p_sInfo;
            Thread.Sleep(10);
            if (Run(m_mdCounter.WriteCoils(m_nUnit, 0, bStart))) return p_sInfo;
            m_sw.Start();
            Thread.Sleep(1000);
            if (Run(WaitDone())) return p_sInfo;
            return RunRead(); 
        }

        string RunRead()
        {
            if (Run(m_mdCounter.ReadInputRegister(m_nUnit, 228, m_aRead))) return p_sInfo;
            for (int n = 0; n < 2 * c_lParticle; n++) m_aRead[n] &= 0xffff;
            for (int n = 0; n < c_lParticle; n++)
            {
                p_aCount[n] = (m_aRead[2 * n] << 16) + m_aRead[2 * n + 1];
            }
            RunTree(Tree.eMode.Init);
            return "OK";
        }

        string RunReadFlow()
        {
            try
            {
                ConnectModbus(m_mbFlow);
                int nFlow = 0;
                return m_mbFlow.ReadInputRegister(1, 1000, ref nFlow);
            }
            catch (Exception e)
            {
                m_log.Error(e, "Read Flow");
                return e.Message; 
            }
        }

        public bool p_bDone
        {
            get
            {
                bool bDone = false;
                m_mdCounter.ReadCoils(m_nUnit, 1, ref bDone);
                return bDone;
            }
            set
            {
                m_mdCounter.WriteCoils(m_nUnit, 1, value);
            }
        }

        StopWatch m_sw = new StopWatch();
        string WaitDone()
        {
            int msTimeout = 1000 * (m_secHold + m_nRepeat * m_secSample + 15);
            while (m_sw.ElapsedMilliseconds < msTimeout)
            {
                Thread.Sleep(100);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_bDone) return "OK";
            }
            return "Read Data Timeout";
        }

        const int c_lParticle = 4; 
        void RunTreeCount(Tree tree)
        {
            for (int n = 0; n < c_lParticle; n++)
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
            for (int n = 0; n < c_lParticle; n++)
            {
                p_aCount[n] = (m_aRead[2 * n] << 16) + m_aRead[2 * n + 1]; 
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
            RunTreeSolValve(tree.GetTree("SolValve"));
            RunTreeSolValveSet(tree.GetTree("SolValveSet"));
        }
        #endregion

        public ParticleCounter(string id, IEngineer engineer)
        {
            InitSolValve(id); 
            InitCount();
            p_id = id;
            InitBase(id, engineer);
            InitBackgroundWorker();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_Pump(this), false, "Run Pump");
            AddModuleRunList(new Run_Pressure(this), false, "Run Set Pump Pressure");
            AddModuleRunList(new Run_Run(this), false, "Run Particle Counter");
            AddModuleRunList(new Run_Start(this), false, "Run Particle Counter");
            AddModuleRunList(new Run_Read(this), false, "Read Particle Counter");
            AddModuleRunList(new Run_ReadFlow(this), false, "Read Flow Sensor");
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
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)", bVisible);
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
                run.m_nPump = m_nPump; 
                run.m_secDelay = m_secDelay;
                return run;
            }

            int m_nPump = 1000; 
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nPump = tree.Set(m_nPump, m_nPump, "Pump", "Pump Power (0 ~ 1000)", bVisible); 
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Delay Time (sec)", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.RunPump(m_nPump))) return p_sInfo;
                Thread.Sleep(1000 * m_secDelay);
                if (m_module.Run(m_module.RunPump(0))) return p_sInfo;
                return "OK";
            }
        }

        public class Run_Pressure : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Pressure(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bPumpOn = false; 
            public override ModuleRunBase Clone()
            {
                Run_Pressure run = new Run_Pressure(m_module);
                run.m_bPumpOn = m_bPumpOn; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bPumpOn = tree.Set(m_bPumpOn, m_bPumpOn, "Pump On", "Pump On After Set", bVisible);
            }

            public override string Run()
            {
                return m_module.RunSetPressure(m_bPumpOn);
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
                if (m_module.Run(m_module.RunSetPressure(true))) return p_sInfo; 
                m_module.StartReadParticle();
                return m_module.WaitDone();
            }
        }

        public class Run_Start : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Start(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bStart = true; 
            public override ModuleRunBase Clone()
            {
                Run_Start run = new Run_Start(m_module);
                run.m_bStart = m_bStart; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bStart = tree.Set(m_bStart, m_bStart, "Start", "Start", bVisible);
            }

            public override string Run()
            {
                return m_module.StartRead(m_bStart);
            }
        }

        public class Run_Read : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_Read(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Read run = new Run_Read(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunRead();
            }
        }

        public class Run_ReadFlow : ModuleRunBase
        {
            ParticleCounter m_module;
            public Run_ReadFlow(ParticleCounter module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_ReadFlow run = new Run_ReadFlow(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunReadFlow();
            }
        }
        #endregion
    }
}
