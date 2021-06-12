using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Loader3 : ModuleBase
    {
        #region ToolBox
        public Axis3D m_axis;
        public DIO_I m_diCrash; 
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Loader3");
            m_toolBox.GetDIO(ref m_diCrash, this, "Crash"); 
            m_picker.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }

        public enum ePosTransfer
        {
            Transfer1,
            Transfer2,
            Transfer3,
            Transfer4,
            Transfer5,
            Transfer6,
            Transfer7,
            Transfer8,
        }
        public enum ePosTray
        {
            Tray1,
            Tray2,
            Tray3,
            Tray4,
            Tray5,
            Tray6,
            Tray7,
            Tray8,
        }
        void InitPosition()
        {
            m_axis.AddPos(GetPosString(Vision2D.eWorks.A));
            m_axis.AddPos(GetPosString(Vision2D.eWorks.B));
            m_axis.AddPos(Enum.GetNames(typeof(ePosTransfer)));
            m_axis.AddPos(Enum.GetNames(typeof(ePosTray)));
        }
        string GetPosString(Vision2D.eWorks eWorks)
        {
            return Vision2D.eVision.Bottom.ToString() + eWorks.ToString();
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
                    p_loader0.m_axis.p_axisX.ServoOn(false); 
                }
            }
        }
        #endregion

        #region AvoidX
        Loader0 p_loader0 { get { return m_handler.m_loader0; } }
        string StartMoveX(string sPos)
        {
            Axis axisX = p_loader0.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(sPos);
            while ((fPos + axisX.m_posDst) > Loader0.c_lAxisX)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_loader0.IsBusy() == false)
                {
                    p_loader0.StartAvoidX(fPos);
                    Thread.Sleep(10);
                }
            }
            return m_axis.p_axisX.StartMove(fPos);
        }

        public string StartAvoidX(double fPos)
        {
            Run_AvoidX run = (Run_AvoidX)m_runAvoidX.Clone();
            run.m_fPos = Loader0.c_lAxisX - fPos;
            return StartRun(run); 
        }

        public string RunAvoidX(double fPos)
        {
            m_axis.p_axisX.StartMove(fPos);
            return m_axis.p_axisX.WaitReady(); 
        }
        #endregion

        #region AxisXY
        public string RunMoveBoat(Vision2D.eWorks eWorks, bool bWait = true)
        {
            string sPos = GetPosString(eWorks);
            if (Run(StartMoveX(sPos))) return p_sInfo;
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveTransfer(ePosTransfer ePos, bool bWait = true)
        {
            if (Run(StartMoveX(ePos.ToString()))) return p_sInfo;
            m_axis.p_axisY.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveTray(ePosTray ePos, bool bWait = true)
        {
            if (Run(StartMoveX(ePos.ToString()))) return p_sInfo;
            m_axis.p_axisY.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region AxisZ
        public string RunMoveZ(Vision2D.eWorks eWorks, double dPos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(GetPosString(eWorks), -dPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTray ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(0);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
        public string RunLoad(Vision2D.eWorks eWorks)
        {
            Boat boat = m_handler.m_aBoats[Vision2D.eVision.Bottom].m_aBoat[eWorks]; 
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (boat.p_eStep != Boat.eStep.Done) return "Boat not Done";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveBoat(eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eWorks, 0))) return p_sInfo;
                boat.RunVacuum(false);
                boat.RunBlow(true);
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                boat.RunBlow(false);
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo;
                m_picker.p_infoStrip = boat.p_infoStrip;
                boat.p_infoStrip = null;
                boat.p_eStep = Boat.eStep.RunReady;
            }
            finally
            {
                RunMoveUp(); 
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnloadTransfer(ePosTransfer ePos)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (m_transfer.m_pusher.p_bEnable == false) return "Buffer Pusher not Enable";
            try
            {
                m_transfer.m_pusher.p_bLock = true;
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTransfer(ePos))) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(RunMoveUp())) return p_sInfo;
                m_transfer.m_pusher.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                m_transfer.m_pusher.p_bLock = false;
            }
            finally
            {
                RunMoveUp(); 
                m_transfer.m_pusher.p_bLock = false;
            }
            return "OK";
        }

        public string RunUnloadTray(ePosTray ePos)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTray(ePos))) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                m_picker.p_infoStrip = null;
                MagazineEV magazine = m_handler.m_magazineEV.m_aEV[(InfoStrip.eMagazine)ePos];
                magazine.PutInfoStrip(m_picker.p_infoStrip);
                if (Run(RunMoveUp())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }
        #endregion


        #region PickerSet
        double m_mmPickerSetUp = 10;
        double m_secPickerSet = 7;
        public string RunPickerSet()
        {
            StopWatch sw = new StopWatch();
            long msPickerSet = (long)(1000 * m_secPickerSet);
            try
            {
                Vision2D.eWorks eWorks = Vision2D.eWorks.A; 
                while (true)
                {
                    if (Run(RunMoveZ(eWorks, 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                    double sec = 0;
                    if (Run(m_pine2.WaitPickerSet(ref sec))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                    if (Run(RunMoveZ(eWorks, 1000 * m_mmPickerSetUp))) return p_sInfo;
                    Thread.Sleep(200);
                    m_pine2.p_diPickerSet = false;
                    if (m_picker.IsVacuum())
                    {
                        sw.Start();
                        while (sw.ElapsedMilliseconds < msPickerSet)
                        {
                            Thread.Sleep(10);
                            if (EQ.IsStop()) return "EQ Stop";
                            if (m_pine2.p_diPickerSet) return "OK";
                        }
                    }
                }
            }
            finally
            {
                RunMoveUp();
            }
        }

        void RunTreePickerSet(Tree tree)
        {
            m_mmPickerSetUp = tree.Set(m_mmPickerSetUp, m_mmPickerSetUp, "Picker Up", "Picker Up (mm)");
            m_secPickerSet = tree.Set(m_secPickerSet, m_secPickerSet, "Done", "PickerSet Done Time (sec)");
        }
        #endregion
        #region override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (m_picker.p_infoStrip != null)
            {
                switch (m_pine2.p_eMode)
                {
                    case Pine2.eRunMode.Stack: return StartUnloadTray();
                    case Pine2.eRunMode.Magazine: return m_transfer.m_pusher.p_bEnable ? StartUnloadTransfer() : "OK";
                }
            }
            else return StartLoadBoat();
            return "OK";
        }

        string StartUnloadTray()
        {
            MagazineEVSet magazine = m_handler.m_magazineEV;
            InfoStrip.eResult eResult = m_picker.p_infoStrip.p_eResult; 
            foreach (ePosTray ePosTray in Enum.GetValues(typeof(ePosTray)))
            {
                if (magazine.IsEnableStack((InfoStrip.eMagazine)ePosTray, eResult, true)) return StartUnloadTray(ePosTray); 
            }
            foreach (ePosTray ePosTray in Enum.GetValues(typeof(ePosTray)))
            {
                if (magazine.IsEnableStack((InfoStrip.eMagazine)ePosTray, eResult, false)) return StartUnloadTray(ePosTray);
            }
            return "OK";
        }

        string StartUnloadTray(ePosTray ePosTray)
        {
            Run_UnloadTray run = (Run_UnloadTray)m_runUnloadTray.Clone();
            run.m_ePos = ePosTray;
            return StartRun(run); 
        }

        string StartUnloadTransfer()
        {
            Run_UnloadTransfer run = (Run_UnloadTransfer)m_runUnloadTransfer.Clone();
            run.m_ePos = (ePosTransfer)m_transfer.m_buffer.m_ePosDst;
            return StartRun(run); 
        }

        string StartLoadBoat()
        {
            Boats boats = m_handler.m_aBoats[Vision2D.eVision.Bottom];
            if (boats.m_aBoat[Vision2D.eWorks.A].p_eStep == Boat.eStep.Done) return StartLoadBoat(Vision2D.eWorks.A);
            if (boats.m_aBoat[Vision2D.eWorks.B].p_eStep == Boat.eStep.Done) return StartLoadBoat(Vision2D.eWorks.B);
            return "OK";
        }

        string StartLoadBoat(Vision2D.eWorks eWorks)
        {
            Run_LoadBoat run = (Run_LoadBoat)m_runLoadBoat.Clone();
            run.m_eWorks = eWorks;
            return StartRun(run); 
        }


        public override void Reset()
        {
            m_picker.m_dioVacuum.Write(false);
            m_picker.p_infoStrip = null;
            base.Reset();
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_picker.RunTreeVacuum(tree.GetTree("Vacuum"));
            RunTreePickerSet(tree.GetTree("PickerSet"));
        }
        #endregion

        public InfoStrip p_infoStrip { get { return m_picker.p_infoStrip; } }
        Picker m_picker = null;
        Pine2 m_pine2 = null;
        Transfer m_transfer = null;
        Pine2_Handler m_handler;
        public Loader3(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_pine2 = handler.m_pine2;
            m_transfer = handler.m_transfer;
            m_handler = handler;
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
            m_picker.ThreadStop();
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoadBoat;
        ModuleRunBase m_runUnloadTransfer;
        ModuleRunBase m_runUnloadTray;
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runLoadBoat = AddModuleRunList(new Run_LoadBoat(this), true, "Load Strip from Boat");
            m_runUnloadTransfer = AddModuleRunList(new Run_UnloadTransfer(this), true, "Unload Strip from Transfer");
            m_runUnloadTray = AddModuleRunList(new Run_UnloadTray(this), true, "Unload Strip to Paper Tray");
            m_runAvoidX = AddModuleRunList(new Run_AvoidX(this), true, "Avoid Axis X");
            AddModuleRunList(new Run_PickerSet(this), false, "Picker Set");
        }

        public class Run_LoadBoat : ModuleRunBase
        {
            Loader3 m_module;
            public Run_LoadBoat(Loader3 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Vision2D.eWorks m_eWorks = Vision2D.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_LoadBoat run = new Run_LoadBoat(m_module);
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eWorks);
            }
        }

        public class Run_UnloadTransfer : ModuleRunBase
        {
            Loader3 m_module;
            public Run_UnloadTransfer(Loader3 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePosTransfer m_ePos = ePosTransfer.Transfer1;
            public override ModuleRunBase Clone()
            {
                Run_UnloadTransfer run = new Run_UnloadTransfer(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePosTransfer)tree.Set(m_ePos, m_ePos, "Transfer", "Select Transfer", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadTransfer(m_ePos);
            }
        }

        public class Run_UnloadTray : ModuleRunBase
        {
            Loader3 m_module;
            public Run_UnloadTray(Loader3 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePosTray m_ePos = ePosTray.Tray1; 
            public override ModuleRunBase Clone()
            {
                Run_UnloadTray run = new Run_UnloadTray(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePosTray)tree.Set(m_ePos, m_ePos, "Tray", "Select Tray", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadTray(m_ePos);
            }
        }

        public class Run_AvoidX : ModuleRunBase
        {
            Loader3 m_module;
            public Run_AvoidX(Loader3 module)
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

        public class Run_PickerSet : ModuleRunBase
        {
            Loader3 m_module;
            public Run_PickerSet(Loader3 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_PickerSet run = new Run_PickerSet(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunPickerSet();
            }
        }
        #endregion
    }

}
