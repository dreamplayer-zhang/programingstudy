using Root_JEDI.Engineer;
using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI.Module
{
    public class FlipperIn : ModuleBase
    {
        #region ToolBox
        public AxisXZ m_axis;
        public DIO_I m_diCrash;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "XZ");
            m_flipper.GetTools(m_toolBox, this, bInit);
            m_toolBox.GetDIO(ref m_diCrash, this, "Crash");
            if (bInit)
            {
                InitPosition();
                InitALID();
            }
        }

        ALID m_alidCrash;
        void InitALID()
        {
            m_alidCrash = m_gaf.GetALID(this, "Crash", "Crash with Loader0");
        }
        #endregion

        #region Crash
        bool m_bThreadCrash = false;
        Thread m_threadCrash;
        void InitThreadCrash()
        {
            m_threadCrash = new Thread(new ThreadStart(RunThreadCrash));
            m_threadCrash.Start();
        }

        void RunThreadCrash()
        {
            m_bThreadCrash = true;
            Thread.Sleep(5000);
            while (m_bThreadCrash)
            {
                Thread.Sleep(10);
                if (m_diCrash.p_bIn)
                {
                    m_axis.p_axisX.ServoOn(false);
                    p_flipperOut.m_axis.p_axisX.ServoOn(false);
                }
                m_alidCrash.p_bSet = m_diCrash.p_bIn;
            }
        }
        #endregion

        #region AvoidX
        public double c_lAxisX = 1557800;
        FlipperOut p_flipperOut { get { return m_handler.m_flipperOut; } }
        string StartMoveX(ePos ePos)
        {
            Axis axisX = p_flipperOut.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(ePos);
            while ((fPos + axisX.m_posDst) > c_lAxisX)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_flipperOut.IsBusy() == false)
                {
                    p_flipperOut.StartAvoidX(fPos);
                    Thread.Sleep(10);
                }
            }
            return m_axis.p_axisX.StartMove(fPos);
        }

        public string StartAvoidX(double fPos)
        {
            Run_AvoidX run = (Run_AvoidX)m_runAvoidX.Clone();
            run.m_fPos = c_lAxisX - fPos;
            return StartRun(run);
        }

        public string RunAvoidX(double fPos)
        {
            m_axis.p_axisX.StartMove(fPos);
            return m_axis.p_axisX.WaitReady();
        }
        #endregion

        #region Axis
        public enum ePos
        {
            Metal,
            Empty,
            TrayInL,
            TrayInR,
        }
        const string c_sUp = "Up";
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            m_axis.p_axisZ.AddPos(c_sUp); 
        }

        public string MoveX(ePos ePos, bool bWait = true)
        {
            if (Run(StartMoveX(ePos))) return p_sInfo;
            return bWait ? m_axis.p_axisX.WaitReady() : "OK";
        }

        public string MoveZ(ePos ePos, double mmOffset, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos, -1000 * mmOffset);
            return bWait ? m_axis.p_axisZ.WaitReady() : "OK";
        }

        public string MoveUp(bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(c_sUp);
            return bWait ? m_axis.p_axisZ.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
        TrayStack p_stackMetal { get { return m_handler.m_stack[TrayStack.eStack.Metal]; } }
        public string RunLoadMatal()
        {
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Down).CheckTray(false))) return p_sInfo;
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Up).CheckTray(false))) return p_sInfo;
            int nMetal = p_stackMetal.CheckTrayCount();
            if (nMetal <= 0) return "No Metal Tray";
            if (Run(MoveUp())) return p_sInfo;
            if (Run(MoveX(ePos.Metal, false))) return p_sInfo;
            if (Run(m_flipper.RunFlip(false))) return p_sInfo;
            if (Run(m_flipper.RunGrip(false))) return p_sInfo;
            if (Run(m_axis.p_axisX.WaitReady())) return p_sInfo;
            try
            {
                if (Run(MoveZ(ePos.Metal, Tray.m_thickTray * (nMetal - 1)))) return p_sInfo;
                if (Run(m_flipper.RunGrip(true))) return p_sInfo;
                Flipper.Floor floor = m_flipper.GetFloor(Flipper.eFloor.Down);
                if (floor.IsCheck(true) == false) return "Check Down Sensor Error";
                if (Run(MoveUp())) return p_sInfo;
                if (floor.IsCheck(true) == false) return "Check Down Sensor Error"; 
                floor.p_infoTray = p_stackMetal.m_stackTray.Pop(); 
            }
            finally { MoveUp(); }
            return "OK";
        }

        public string RunLoadTrayIn(TrayIn.eIn eIn)
        {
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Down).CheckTray(true))) return p_sInfo;
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Up).CheckTray(false))) return p_sInfo;
            TrayIn trayIn = m_handler.m_trayIn[eIn];
            if (trayIn.m_stage.p_infoTray == null) return "InfoTray == null at " + eIn.ToString(); 
            if (trayIn.m_stage.IsCheck(true) == false) return "Check Tray Sensor at" + eIn.ToString();
            if (trayIn.IsInPos(TrayIn.ePos.Flipper) == false) return eIn.ToString() + " Position not Flipper";
            if (Run(MoveUp())) return p_sInfo;
            ePos ePos = (eIn == TrayIn.eIn.TrayInL) ? ePos.TrayInL : ePos.TrayInR; 
            if (Run(MoveX(ePos))) return p_sInfo;
            try
            {
                if (Run(MoveZ(ePos, Tray.m_thickTray))) return p_sInfo;
                if (Run(m_flipper.RunGrip(false))) return p_sInfo;
                if (Run(MoveZ(ePos, 0))) return p_sInfo;
                if (Run(m_flipper.RunGrip(true))) return p_sInfo;
                Flipper.Floor floorD = m_flipper.GetFloor(Flipper.eFloor.Down);
                Flipper.Floor floorU = m_flipper.GetFloor(Flipper.eFloor.Up);
                if (floorD.IsCheck(true) == false) return "Check Down Sensor Error";
                if (floorU.IsCheck(true) == false) return "Check Up Sensor Error";
                if (Run(MoveUp())) return p_sInfo;
                if (floorD.IsCheck(true) == false) return "Check Down Sensor Error";
                if (floorU.IsCheck(true) == false) return "Check Up Sensor Error";
                floorU.p_infoTray = floorD.p_infoTray;
                floorD.p_infoTray = trayIn.m_stage.p_infoTray;
                trayIn.m_stage.p_infoTray = null; 
            }
            finally { MoveUp(); }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnloadTrayIn()
        {
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Down).CheckTray(true))) return p_sInfo;
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Up).CheckTray(true))) return p_sInfo;
            TrayIn trayIn = m_handler.m_trayIn[TrayIn.eIn.TrayInR];
            if (trayIn.m_stage.p_infoTray != null) return "InfoTray != null at TrainInR";
            if (trayIn.m_stage.IsCheck(false) == false) return "Check Tray Sensor at TrayInR";
            if (trayIn.IsInPos(TrayIn.ePos.Flipper) == false) return "TrayInR Position not Flipper";
            if (Run(MoveUp())) return p_sInfo;
            ePos ePos = ePos.TrayInR; 
            if (Run(MoveX(ePos, false))) return p_sInfo;
            if (Run(m_flipper.RunFlip(true))) return p_sInfo;
            if (Run(m_axis.p_axisX.WaitReady())) return p_sInfo;
            try
            {
                if (Run(MoveZ(ePos, 0))) return p_sInfo;
                if (Run(m_flipper.RunGrip(false))) return p_sInfo;
                if (Run(MoveZ(ePos, Tray.m_thickTray))) return p_sInfo;
                if (Run(m_flipper.RunGrip(true))) return p_sInfo;
                Flipper.Floor floorD = m_flipper.GetFloor(Flipper.eFloor.Down);
                Flipper.Floor floorU = m_flipper.GetFloor(Flipper.eFloor.Up);
                if (floorD.IsCheck(true) == false) return "Check Down Sensor Error";
                if (floorU.IsCheck(false) == false) return "Check Up Sensor Error";
                if (Run(MoveUp())) return p_sInfo;
                if (floorD.IsCheck(true) == false) return "Check Down Sensor Error";
                if (floorU.IsCheck(false) == false) return "Check Up Sensor Error";
                trayIn.m_stage.p_infoTray = floorD.p_infoTray;
                floorD.p_infoTray = floorU.p_infoTray;
                floorU.p_infoTray = null; 
            }
            finally { MoveUp(); }
            return "OK";
        }

        TrayStack p_stackEmpty { get { return m_handler.m_stack[TrayStack.eStack.Empty]; } }
        public string RunUnloadEmpty()
        {
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Down).CheckTray(true))) return p_sInfo;
            if (Run(m_flipper.GetFloor(Flipper.eFloor.Up).CheckTray(false))) return p_sInfo;
            int nEmpty = p_stackMetal.CheckTrayCount();
            if (nEmpty >= 4) return "Empty Tray Full";
            if (Run(MoveUp())) return p_sInfo;
            if (Run(MoveX(ePos.Empty))) return p_sInfo;
            try
            {
                if (Run(MoveZ(ePos.Empty, Tray.m_thickTray * nEmpty))) return p_sInfo;
                if (Run(m_flipper.RunGrip(false))) return p_sInfo;
                if (Run(MoveUp())) return p_sInfo;
                Flipper.Floor floor = m_flipper.GetFloor(Flipper.eFloor.Down);
                if (floor.IsCheck(false) == false) return "Check Down Sensor Error";
                p_stackEmpty.m_stackTray.Push(floor.p_infoTray);
                floor.p_infoTray = null; 
            }
            finally { MoveUp(); }
            return "OK";
        }
        #endregion

        Flipper m_flipper;
        JEDI_Handler m_handler; 
        public FlipperIn(string id, IEngineer engineer)
        {
            p_id = id;
            m_flipper = new Flipper(id);
            m_handler = (JEDI_Handler)engineer.ClassHandler(); 
            base.InitBase(id, engineer);
            InitThreadCrash();
        }

        public override void ThreadStop()
        {
            if (m_bThreadCrash)
            {
                m_bThreadCrash = false;
                m_threadCrash.Join();
            }
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runAvoidX = AddModuleRunList(new Run_AvoidX(this), false, "Avoid Axis X");
        }

        public class Run_AvoidX : ModuleRunBase
        {
            FlipperIn m_module;
            public Run_AvoidX(FlipperIn module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public double m_fPos = 0;
            public override ModuleRunBase Clone()
            {
                Run_AvoidX run = new Run_AvoidX(m_module);
                run.m_fPos = m_fPos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fPos = tree.Set(m_fPos, m_fPos, "Position", "Axis X Avoid Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunAvoidX(m_fPos);
            }
        }
        #endregion
    }
}
