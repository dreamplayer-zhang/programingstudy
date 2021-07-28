using Root_VEGA_P.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_VEGA_P.Module
{
    public class EIP_Cover : ModuleBase, IRTRChild
    {
        #region ToolBox
        DIO_Os m_doCover;
        DIO_Is[] m_diCover = new DIO_Is[2] { null, null };
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_doCover, this, p_id + ".Cover", Enum.GetNames(typeof(eCover)));
            p_sInfo = m_toolBox.GetDIO(ref m_diCover[0], this, p_id + ".Cover Open", new string[] { "0", "1" });
            p_sInfo = m_toolBox.GetDIO(ref m_diCover[1], this, p_id + ".Cover Close", new string[] { "0", "1" });
            m_particleCounterSet.GetTools(m_toolBox, bInit); 
            if (bInit) { }
        }
        #endregion

        #region CoverDown
        public enum eCover
        {
            Open,
            Close
        }
        double m_secCover = 3;
        public string RunCover(bool bClose)
        {
            m_doCover.Write(bClose ? eCover.Close : eCover.Open);
            m_doCover.Write(bClose ? eCover.Close : eCover.Open);
            StopWatch sw = new StopWatch();
            int msCover = (int)(1000 * m_secCover);
            while (sw.ElapsedMilliseconds < msCover)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (IsCoverClose(bClose)) return "OK";
            }
            return "Run Cover Timeout";
        }

        public bool IsCoverClose(bool bClose)
        {
            for (int n = 0; n < 2; n++)
            {
                if (m_diCover[0].ReadDI(n) == bClose) return false;
                if (m_diCover[1].ReadDI(n) == !bClose) return false;
            }
            return true;
        }

        void RunTreeCover(Tree tree)
        {
            m_secCover = tree.Set(m_secCover, m_secCover, "Timeout", "Run Cover Timeout (sec)");
        }
        #endregion

        #region InfoPod
        InfoPod _infoPod = null;
        public InfoPod p_infoPod
        {
            get { return _infoPod; }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1;
                _infoPod = value;
                m_reg.Write("InfoPod", nPod);
                value?.WriteReg();
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            m_reg = new Registry("InfoPod");
            int nPod = m_reg.Read(p_id, -1);
            if (nPod < 0) return;
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }

        public bool IsEnableRecovery()
        {
            return p_infoPod != null;
        }
        #endregion

        #region IRTRChild
        public bool p_bLock { get; set; }

        public string IsGetOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            switch (infoPod.p_ePod)
            {
                case InfoPod.ePod.EOP_Door:
                case InfoPod.ePod.EOP_Dome:
                case InfoPod.ePod.EIP_Cover:
                    return p_id + " Invalid Pod Type";
            }
            return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
        }

        public string BeforeGet()
        {
            return RunCover(false);
        }

        public string BeforePut(InfoPod infoPod)
        {
            return RunCover(false);
        }

        public string AfterGet()
        {
            return RunCover(true);
        }

        public string AfterPut()
        {
            return RunCover(true);
        }

        public bool IsPodExist(InfoPod.ePod ePod)
        {
            return (p_infoPod != null);
        }
        #endregion

        #region Teach RTR
        int m_teach = -1;
        public int GetTeachRTR(InfoPod infoPod)
        {
            return m_teach;
        }

        public void RunTreeTeach(Tree tree)
        {
            m_teach = tree.GetTree("Particle Counter").Set(m_teach, m_teach, p_id, "RND RTR Teach");
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            string sHome = base.StateHome();
            p_eState = (sHome == "OK") ? eState.Ready : eState.Error;
            Reset();
            return sHome;
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCover(tree.GetTree("Cover"));
            m_particleCounterSet.RunTree(tree.GetTree("Particle Counter")); 
        }
        #endregion

        
        public ParticleCounterSet m_particleCounterSet;
        public EIP_Cover(string id, IEngineer engineer)
        {
            p_id = id; 
            VEGA_P vegaP = ((VEGA_P_Handler)engineer.ClassHandler()).m_VEGA;
            m_particleCounterSet = new ParticleCounterSet(this, vegaP);
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_particleCounterSet.ThreadStop(); 
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            AddModuleRunList(new Run_RunCover(this), false, "Run Cover Sol Test");
            m_particleCounterSet.InitModuleRuns(true); 
        }

        public class Run_Delay : ModuleRunBase
        {
            EIP_Cover m_module;
            public Run_Delay(EIP_Cover module)
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
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }

        public class Run_RunCover : ModuleRunBase
        {
            EIP_Cover m_module;
            public Run_RunCover(EIP_Cover module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bClose = false;
            public override ModuleRunBase Clone()
            {
                Run_RunCover run = new Run_RunCover(m_module);
                run.m_bClose = m_bClose;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bClose = tree.Set(m_bClose, m_bClose, "Cover Close", "Cover Close", bVisible);
            }

            public override string Run()
            {
                return m_module.RunCover(m_bClose);
            }
        }
        #endregion
    }
}
