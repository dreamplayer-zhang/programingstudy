using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_ASIS.Module
{
    public class Turnover : ModuleBase
    {
        #region ToolBox
        Axis m_axisTurn;
        DIO_I2O m_dioUp;
        DIO_O[] m_doVac = new DIO_O[2];
        DIO_O[] m_doBlow = new DIO_O[2];

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisTurn, this, "Turn");
            p_sInfo = m_toolBox.Get(ref m_dioUp, this, "Up", "Down", "Up");
            p_sInfo = m_toolBox.Get(ref m_doVac[0], this, "Vacuum 0");
            p_sInfo = m_toolBox.Get(ref m_doVac[1], this, "Vacuum 1");
            p_sInfo = m_toolBox.Get(ref m_doBlow[0], this, "Blow 0");
            p_sInfo = m_toolBox.Get(ref m_doBlow[1], this, "Blow 1");
            if (bInit) InitTools();
        }

        void InitTools()
        {
            InitPosition();
        }
        #endregion

        #region DIO Functions
        double m_secBlow0 = 0.1;
        int m_msBlow0 = 100; 
        bool _bVacuum0 = false;
        public bool p_bVacuum0
        {
            get { return _bVacuum0; }
            set
            {
                if (_bVacuum0 == value) return;
                _bVacuum0 = value;
                m_doVac[0].Write(value);
                if (value == false) m_doBlow[0].DelayOff(m_msBlow0); 
                OnPropertyChanged();
            }
        }

        double m_secBlow1 = 0.1;
        int m_msBlow1 = 100;
        bool _bVacuum1 = false;
        public bool p_bVacuum1
        {
            get { return _bVacuum1; }
            set
            {
                if (_bVacuum1 == value) return;
                _bVacuum1 = value;
                m_doVac[1].Write(value);
                if (value == false) m_doBlow[1].DelayOff(m_msBlow1);
                OnPropertyChanged();
            }
        }

        double m_secUp = 2;
        bool _bUp = false; 
        public bool p_bUp
        {
            get { return _bUp; }
            set
            {
                if (_bUp == value) return;
                m_dioUp.Write(true); 
                _bUp = value;
                OnPropertyChanged(); 
            }
        }

        void RunTreeDIO(Tree tree)
        {
            m_secBlow0 = tree.Set(m_secBlow0, m_secBlow0, "Blow 0", "Blow Time (sec)");
            m_secBlow1 = tree.Set(m_secBlow1, m_secBlow1, "Blow 1", "Blow Time (sec)");
            m_secUp = tree.Set(m_secUp, m_secUp, "UpDown", "Up Down Timeour (sec)");
            m_msBlow0 = (int)(1000 * m_secBlow0);
            m_msBlow1 = (int)(1000 * m_secBlow1);
        }
        #endregion

        #region Axis Function
        public enum ePos
        {
            Ready,
            Turn,
        }
        void InitPosition()
        {
            m_axisTurn.AddPos(Enum.GetNames(typeof(ePos)));
        }

        string AxisTurn(ePos ePos)
        {
            if (Run(m_axisTurn.StartMove(ePos))) return p_sInfo;
            return m_axisTurn.WaitReady();
        }
        #endregion

        #region RunTurn
        public string RunTurn()
        {
            try
            {
                p_bVacuum0 = true;
                p_bVacuum1 = false;
                p_bUp = false;
                if (Run(AxisTurn(ePos.Turn))) return p_sInfo;
                p_bUp = true;
                if (Run(m_dioUp.WaitDone(m_secUp))) return p_sInfo;
                p_bVacuum0 = false;
                p_bVacuum1 = true; 
                return "OK";
            }
            finally
            {
                p_bUp = false;
                Run(AxisTurn(ePos.Ready));
                p_bVacuum1 = false; 
                p_infoStrip1 = p_infoStrip0;
                p_infoStrip0 = null; 
            }
        }
        #endregion

        #region InfoStrip
        InfoStrip _infoStrip0 = null;
        public InfoStrip p_infoStrip0
        {
            get { return _infoStrip0; }
            set
            {
                _infoStrip0 = value;
                OnPropertyChanged();
                m_reg.Write("iStrip0", (value == null) ? -1 : value.p_iStrip);
            }
        }

        InfoStrip _infoStrip1 = null;
        public InfoStrip p_infoStrip1
        {
            get { return _infoStrip1; }
            set
            {
                _infoStrip1 = value;
                OnPropertyChanged();
                m_reg.Write("iStrip1", (value == null) ? -1 : value.p_iStrip);
            }
        }

        void InitStrip()
        {
            int iStrip0 = m_reg.Read("iStrip0", -1);
            if (iStrip0 >= 0) p_infoStrip0 = new InfoStrip(iStrip0);
            int iStrip1 = m_reg.Read("iStrip1", -1);
            if (iStrip1 >= 0) p_infoStrip1 = new InfoStrip(iStrip1);
        }
        #endregion

        #region Override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (p_infoStrip0 == null) return "OK";
            if (p_infoStrip1 != null) return "OK";
            StartRun(m_runTurn); 
            return "OK";
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            RunTreeDIO(tree.GetTree("DIO", false));
        }

        public override void Reset()
        {
            AxisTurn(ePos.Ready);
            p_bUp = false; 
            m_doBlow[0].Write(false);
            m_doBlow[1].Write(false);
            p_bVacuum0 = false;
            p_bVacuum1 = false;
            base.Reset();
        }
        #endregion

        Registry m_reg;
        public Turnover(string id, IEngineer engineer)
        {
            m_reg = new Registry(id);
            InitStrip();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            Reset(); 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runTurn;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), false, "Time Delay");
            m_runTurn = AddModuleRunList(new Run_Turnover(this), false, "Run Turnover");
        }

        public class Run_Delay : ModuleRunBase
        {
            Turnover m_module;
            public Run_Delay(Turnover module)
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

        public class Run_Turnover : ModuleRunBase
        {
            Turnover m_module;
            public Run_Turnover(Turnover module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Turnover run = new Run_Turnover(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunTurn();
            }
        }
        #endregion

    }
}
