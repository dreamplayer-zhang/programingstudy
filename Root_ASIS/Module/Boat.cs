using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;

namespace Root_ASIS.Module
{
    public class Boat : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        DIO_O m_doVacuum;
        DIO_O m_doBlow;
        DIO_O m_doWingBlow;
        DIO_O m_doCleanBlow; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Boat");
            p_sInfo = m_toolBox.Get(ref m_doVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Blow");
            p_sInfo = m_toolBox.Get(ref m_doWingBlow, this, "WingBlow");
            p_sInfo = m_toolBox.Get(ref m_doCleanBlow, this, "CleanBlow");
            if (bInit) InitTools();
        }

        void InitTools()
        {
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
        #endregion

        #region Grab
        double m_dpAcc = 10; 
        public string RunGrab()
        {
            StopWatch sw = new StopWatch();
            p_bVacuum = true;
            p_bCleanBlow = true;
            double posStart = m_axis.m_trigger.m_aPos[0] - m_dpAcc;
            if (m_axis.p_posCommand < posStart)
            {
                m_axis.StartMove(posStart);
                if (Run(m_axis.WaitReady())) return m_sRun; 
            }
            m_axis.RunTrigger(true); //forget
            return "OK"; 
        }

        string m_sRun = ""; 
        bool Run(string sRun)
        {
            m_sRun = sRun;
            return (sRun != "OK"); 
        }

        void RunTreeGrab(Tree tree)
        {
            m_dpAcc = tree.Set(m_dpAcc, m_dpAcc, "Acc", "Acceleration Width (" + m_axis.m_sUnit + ")"); 
            m_axis.m_trigger.RunTree(tree.GetTree("Trigger"), m_axis.m_sUnit); 
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
            RunTreeGrab(tree.GetTree("Grab", false));
//            RunTreeDIOWait(tree.GetTree("Timeout", false));
        }

        public override void Reset()
        {
            p_bBlow = false;
            p_bWingBlow = false;
            p_bCleanBlow = false;
            base.Reset();
        }
        #endregion

        public Boat(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            p_bVacuum = false;
            p_bBlow = false;
            p_bWingBlow = false;
            p_bCleanBlow = false; 
            base.ThreadStop();
        }

    }
}
