using Root_ASIS.Teachs;
using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Boat : ModuleBase
    {
        #region eCleaner
        public enum eBoat
        {
            Boat0,
            Boat1
        }
        #endregion 

        #region ToolBox
        Axis m_axis;
        DIO_O m_doVacuum;
        DIO_O m_doBlow;
        DIO_O m_doWingBlow;
        DIO_O m_doCleanBlow;
        LightSet m_lightSet;
        MemoryPool m_memoryPool;
        CameraDalsa m_cam;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Boat");
            p_sInfo = m_toolBox.Get(ref m_doVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Blow");
            p_sInfo = m_toolBox.Get(ref m_doWingBlow, this, "WingBlow");
            p_sInfo = m_toolBox.Get(ref m_doCleanBlow, this, "CleanBlow");
            p_sInfo = m_toolBox.Get(ref m_lightSet, this); 
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_cam, this, "Camera");
            if (bInit) InitTools();
        }

        void InitTools()
        {
            InitPosition(); 
        }
        #endregion

        #region DIO Functions
        bool _bVacuum = false; 
        public bool p_bVacuum
        {
            get { return _bVacuum; }
            set
            {
                if (_bVacuum == value) return;
                _bVacuum = value;
                m_doVacuum.Write(value); 
                OnPropertyChanged(); 
            }
        }

        bool _bBlow = false;
        public bool p_bBlow
        {
            get { return _bBlow; }
            set
            {
                if (_bBlow == value) return;
                _bBlow = value;
                m_doBlow.Write(value);
                OnPropertyChanged();
            }
        }

        bool _bWingBlow = false;
        public bool p_bWingBlow
        {
            get { return _bWingBlow; }
            set
            {
                if (_bWingBlow == value) return;
                _bWingBlow = value;
                m_doWingBlow.Write(value);
                OnPropertyChanged();
            }
        }

        bool _bCleanBlow = false;
        public bool p_bCleanBlow
        {
            get { return _bCleanBlow; }
            set
            {
                if (_bCleanBlow == value) return;
                _bCleanBlow = value;
                m_doCleanBlow.Write(value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Axis Function
        public enum ePos
        {
            Ready,
            ReadyMGZ,
            Done,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }
        #endregion

        #region Light
        public class LightMode
        {
            List<double> _aLightPower = new List<double>();
            List<double> p_aLightPower
            {
                get
                {
                    while (_aLightPower.Count < m_boat.m_lightSet.m_aLight.Count) _aLightPower.Add(0);
                    return _aLightPower;
                }
            }

            public bool p_bLightOn
            {
                set
                {
                    for (int n = 0; n < p_aLightPower.Count; n++) m_boat.m_lightSet.m_aLight[n].p_bOn = value;
                }
            }

            public void RunTree(Tree treeParent)
            {
                Tree tree = treeParent.GetTree(m_id, false); 
                for (int n = 0; n < p_aLightPower.Count; n++)
                {
                    string id = m_boat.m_lightSet.m_aLight[n].m_sName;
                    p_aLightPower[n] = tree.Set(p_aLightPower[n], p_aLightPower[n], id, "Light Power (0 ~ 100 %%)"); 
                }
            }

            public string m_id;
            Boat m_boat; 
            public LightMode(string id, Boat boat)
            {
                m_id = id; 
                m_boat = boat; 
            }
        }

        public List<LightMode> m_aLightMode = new List<LightMode>(); 
        public int p_lLightMode
        {
            get { return m_aLightMode.Count; }
            set
            {
                while (m_aLightMode.Count < value)
                {
                    LightMode lightMode = new LightMode("LightMode " + m_aLightMode.Count.ToString("00"), this);
                    m_aLightMode.Add(lightMode);
                }
            }
        }

        public List<string> p_asLightMode
        {
            get
            {
                List<string> asLightMode = new List<string>();
                foreach (LightMode lightMode in m_aLightMode) asLightMode.Add(lightMode.m_id);
                return asLightMode; 
            }
        }

        public LightMode GetLightMode(string sLightMode)
        {
            List<string> asLightMode = p_asLightMode; 
            for (int n = 0; n < asLightMode.Count; n++)
            {
                if (asLightMode[n] == sLightMode) return m_aLightMode[n]; 
            }
            return null; 
        }

        void RunTreeLight(Tree tree)
        {
            p_lLightMode = tree.Set(p_lLightMode, p_lLightMode, "Count", "Light Mode Count");
            for (int n = 0; n < p_lLightMode; n++)
            {
                string sLightID = "Light ID " + n.ToString("00"); 
                m_aLightMode[n].m_id = tree.Set(m_aLightMode[n].m_id, m_aLightMode[n].m_id, sLightID, "Light ID"); 
            }
            for (int n = 0; n < p_lLightMode; n++) m_aLightMode[n].RunTree(tree);
        }
        #endregion

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryGrab;
        CPoint m_szGrab = new CPoint(1024, 1024);
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryGrab = m_memoryGroup.CreateMemory("Grab", 1, m_cam.p_nByte, m_szGrab);
            m_memoryPool.m_viewer.p_memoryData = m_memoryGrab;
            m_memoryPool.m_viewer.p_fZoom = 0.04;
            m_cam.SetMemoryData(m_memoryGrab);
        }

        void RunTreeMemory(Tree tree)
        {
            m_szGrab = tree.Set(m_szGrab, m_szGrab, "Grab Size", "Dalsa Grab Size (pixel)");
        }
        #endregion

        #region Grab
        double m_vGrab = 10; 
        double m_dpAcc = 10;
        public string RunGrab(string sLightMode)
        {
            StopWatch sw = new StopWatch();
            LightMode lightMode = GetLightMode(sLightMode);
            if (lightMode != null) lightMode.p_bLightOn = true; 
            p_bVacuum = true;
            p_bCleanBlow = true;
            double posStart = m_axis.m_trigger.m_aPos[0] - m_dpAcc;
            if (m_axis.p_posCommand < posStart)
            {
                m_axis.StartMove(posStart);
                if (Run(m_axis.WaitReady())) return p_sInfo; 
            }
            m_axis.RunTrigger(true);
            Axis.Trigger trigger = m_axis.m_trigger; 
            int nLine = (int)Math.Round((trigger.m_aPos[1] - trigger.m_aPos[0]) / trigger.m_dPos);
            if (Run(m_cam.StartGrab(new CPoint(), nLine))) return p_sInfo; 
            double v = m_axis.GetSpeedValue(Axis.eSpeed.Move).m_v;
            double posDone = m_axis.GetPosValue(ePos.Done); 
            m_axis.StartMoveV(v, posStart, m_vGrab, posDone);
            while (m_cam.p_bOnGrab && (m_axis.p_posCommand < posDone)) Thread.Sleep(10);
            m_axis.OverrideVelocity(v);
            if (Run(m_axis.WaitReady())) return p_sInfo;
            if (m_cam.p_bOnGrab) return "Camera Dalsa OnGrab Error";
            p_bCleanBlow = false;
            if (lightMode != null) lightMode.p_bLightOn = false;
            m_log.Info("RunGrab Done : " + (sw.ElapsedMilliseconds / 1000.0).ToString("0.00")); 
            return "OK"; 
        }

        void RunTreeGrab(Tree tree)
        {
            m_vGrab = tree.Set(m_vGrab, m_vGrab, "Grab V", "Grab Speed (" + m_axis.m_sUnit + " / sec)");
            m_dpAcc = tree.Set(m_dpAcc, m_dpAcc, "Acc", "Acceleration Width (sec)");
            m_axis.m_trigger.RunTree(tree.GetTree("Trigger"), m_axis.m_sUnit); 
        }
        #endregion

        #region InfoStrip
        InfoStrip _infoStrip = null;
        public InfoStrip p_infoStrip
        {
            get { return _infoStrip; }
            set
            {
                _infoStrip = value;
                OnPropertyChanged();
                m_reg.Write("iStrip", (value == null) ? -1 : value.p_iStrip);
                p_bReady = false;
                p_bDone = false; 
            }
        }

        void InitStrip()
        {
            int iStrip = m_reg.Read("iStrip", -1);
            if (iStrip < 0) return;
            p_infoStrip = new InfoStrip(iStrip);
        }
        #endregion

        #region Property
        public bool _bReady = false;
        public bool p_bReady
        {
            get { return _bReady; }
            set
            {
                if (_bReady == value) return;
                _bReady = value;
                OnPropertyChanged();
            }
        }

        public bool _bDone = false;
        public bool p_bDone
        {
            get { return _bDone; }
            set
            {
                if (_bDone == value) return;
                _bDone = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Teach
        public Teach m_teach; 
        void InitTeach()
        {
            m_teach = new Teach(p_id, m_memoryPool); 
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (p_bReady) return "OK";
            if (p_infoStrip != null) return "OK";
            Run_Move run = (Run_Move)m_runMove.Clone();
            run.m_ePos = Strip.p_bUseMGZ ? ePos.ReadyMGZ : ePos.Ready;
            StartRun(run); 
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeLight(tree.GetTree("Light", false)); 
            RunTreeMemory(tree.GetTree("Memory", false));
            RunTreeGrab(tree.GetTree("Grab", false));
        }

        public override void Reset()
        {
            p_bBlow = false;
            p_bWingBlow = false;
            p_bCleanBlow = false;
            base.Reset();
        }
        #endregion

        int m_nID = 0; 
        Registry m_reg;
        public Boat(string id, int nID, IEngineer engineer)
        {
            m_nID = nID; 
            m_reg = new Registry(id);
            InitStrip();
            base.InitBase(id, engineer);
            InitTeach();
        }

        public override void ThreadStop()
        {
            p_bVacuum = false;
            Reset(); 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runMove; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            m_runMove = AddModuleRunList(new Run_Move(this), false, "Move Boat");
            AddModuleRunList(new Run_Grab(this), false, "Grab LineScan");
        }

        public class Run_Delay : ModuleRunBase
        {
            Boat m_module;
            public Run_Delay(Boat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Move : ModuleRunBase
        {
            Boat m_module;
            public Run_Move(Boat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.Ready; 
            public override ModuleRunBase Clone()
            {
                Run_Move run = new Run_Move(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Position", "Boat Move Position", bVisible);
            }

            public override string Run()
            {
                if (m_module.Run(m_module.m_axis.StartMove(m_ePos))) return p_sInfo; 
                return m_module.m_axis.WaitReady();
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            Boat m_module;
            public Run_Grab(Boat module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sLightMode = ""; 
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_sLightMode = m_sLightMode; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sLightMode = tree.Set(m_sLightMode, m_sLightMode, m_module.p_asLightMode, "LightMode", "LightMode ID", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunGrab(m_sLightMode); 
            }
        }
        #endregion

    }
}
