using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

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
        MemoryPool m_memoryPool;
        CameraDalsa m_cam;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Boat");
            p_sInfo = m_toolBox.Get(ref m_doVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Blow");
            p_sInfo = m_toolBox.Get(ref m_doWingBlow, this, "WingBlow");
            p_sInfo = m_toolBox.Get(ref m_doCleanBlow, this, "CleanBlow");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_cam, this, "Camera");
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

        #region Memory
        MemoryGroup m_memoryGroup;
        MemoryData m_memoryGrab;
        CPoint m_szGrab = new CPoint(1024, 1024);
        public override void InitMemorys()
        {
            m_memoryGroup = m_memoryPool.GetGroup(p_id);
            m_memoryGrab = m_memoryGroup.CreateMemory("Grab", 1, m_cam.p_nByte, m_szGrab);
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
        double m_posDone = 100; 
        public string RunGrab()
        {
            StopWatch sw = new StopWatch();
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
            m_axis.StartMoveV(v, posStart, m_vGrab, m_posDone);
            while (m_cam.p_bOnGrab && (m_axis.p_posCommand < m_posDone)) Thread.Sleep(10);
            m_axis.OverrideVelocity(v);
            if (Run(m_axis.WaitReady())) return p_sInfo;
            if (m_cam.p_bOnGrab) return "Camera Dalsa OnGrab Error";
            p_bCleanBlow = false;
            m_log.Info("RunGrab Done : " + (sw.ElapsedMilliseconds / 1000.0).ToString("0.00")); 
            return "OK"; 
        }

        void RunTreeGrab(Tree tree)
        {
            m_vGrab = tree.Set(m_vGrab, m_vGrab, "Grab V", "Grab Speed (" + m_axis.m_sUnit + " / sec)");
            m_dpAcc = tree.Set(m_dpAcc, m_dpAcc, "Acc", "Acceleration Width (sec)");
            m_posDone = tree.Set(m_posDone, m_posDone, "Done Pos", "Destination Position (" + m_axis.m_sUnit + ")");
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
            RunTreeMemory(tree.GetTree("Memory", false));
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
