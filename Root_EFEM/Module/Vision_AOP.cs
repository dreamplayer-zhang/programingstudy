using RootTools;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_EFEM.Module
{
    public class Vision_AOP : ModuleBase, IWTRChild
    {
        #region ToolBox
        MemoryPool m_memoryPool;

        AxisXY m_axisXY;
        public AxisXY p_axisXY
        {
            get
            {
                return m_axisXY;
            }
        }
        Axis m_axistheta;
        public Axis p_axistheta
        {
            get
            {
                return m_axistheta;
            }
        }

        //Axis m_axisZ;
        //public Axis p_axisZ
        //{
        //    get
        //    {
        //        return m_axisZ;
        //    }
        //}
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Stage");
            p_sInfo = m_toolBox.Get(ref m_axistheta, this, "Theta");
            //p_sInfo = m_toolBox.Get(ref m_axisZ, this, "StageZ");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory", 1);
            if (bInit)
            {
                InitMemory();
            }
        }

        
        #endregion

        #region Memory
        void InitMemory()
        {
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region InfoWafer UI
        InfoWaferChild_UI m_ui;
        void InitInfoWaferUI()
        {
            m_ui = new InfoWaferChild_UI();
            m_ui.Init(this);
            m_aTool.Add(m_ui);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot { get { return null; } }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false) return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            return "OK";
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return "OK";
        }

        public string AfterGet(int nID)
        {
            return "OK";
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        enum eCheckWafer
        {
            InfoWafer,
            Sensor
        }
        eCheckWafer m_eCheckWafer = eCheckWafer.InfoWafer;
        public bool IsWaferExist(int nID)
        {
            switch (m_eCheckWafer)
            {
                case eCheckWafer.Sensor: return false; // m_diWaferExist.p_bIn;
                default: return (p_infoWafer != null);
            }
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_eCheckWafer = (eCheckWafer)tree.Set(m_eCheckWafer, m_eCheckWafer, "CheckWafer", "CheckWafer");
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        public Vision_AOP(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitInfoWaferUI();

        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Ready(this), true, "Go to ReadyPosition");
        }

        public class Run_Ready : ModuleRunBase
        {
            Vision_AOP m_module;
            public RPoint m_rpReadyPos_pulse = new RPoint();    // Vision Stage XY Ready
            public double m_dReadyTheta_pulse = 0;              // Vision Theta

            public bool m_bUseInspect = false;                  // 검사 유무
            public Run_Ready(Vision_AOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_Ready run = new Run_Ready(m_module);
                run.m_rpReadyPos_pulse = m_rpReadyPos_pulse;
                run.m_dReadyTheta_pulse = m_dReadyTheta_pulse;

                run.m_bUseInspect = m_bUseInspect;

                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)//
            {
                m_rpReadyPos_pulse = tree.Set(m_rpReadyPos_pulse, m_rpReadyPos_pulse, " X,Y Axis Ready Position [Pulse]", "X,Y Axis Ready Position [Pulse]", bVisible);
                m_dReadyTheta_pulse = tree.Set(m_dReadyTheta_pulse, m_dReadyTheta_pulse, "Theta Axis Ready Position [Pulse]", "Theta Axis Ready Position [Pulse]", bVisible);

                m_bUseInspect = tree.Set(m_bUseInspect, m_bUseInspect, "Use Inspection", "Use Inspection", bVisible);
            }

            public override string Run()//
            {
                AxisXY axisXY = m_module.p_axisXY;
                Axis axistheta = m_module.p_axistheta;
                double dAxisPosX = m_rpReadyPos_pulse.X;
                double dAxisPosY = m_rpReadyPos_pulse.Y;

                if (m_module.Run(axisXY.StartMove(new RPoint(dAxisPosX, dAxisPosY)))) return p_sInfo;
                if (m_module.Run(axistheta.StartMove(m_dReadyTheta_pulse))) return p_sInfo;

                return "OK";
            }
        }
        public class Run_Delay : ModuleRunBase
        {
            Vision_AOP m_module;
            public Run_Delay(Vision_AOP module)
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
        #endregion
    }
}
