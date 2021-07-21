using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
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
            if (bInit)
            {
                InitPosition();
                InitALID(); 
            }
        }

        ALID m_alidCrash;
        ALID m_alidSorter;
        void InitALID()
        {
            m_alidCrash = m_gaf.GetALID(this, "Crash", "Crash with Loader0");
            m_alidSorter = m_gaf.GetALID(this, "Sorter", "Sorter Bin not Enable");
            m_alidSorter.p_bEQError = false; 
        }

        public enum ePosTransfer
        {
            Transfer0,
            Transfer1,
            Transfer2,
            Transfer3,
            Transfer4,
            Transfer5,
            Transfer6,
            Transfer7,
        }
        public enum ePosTray
        {
            Tray0,
            Tray1,
            Tray2,
            Tray3,
            Tray4,
            Tray5,
            Tray6,
            Tray7,
        }
        const string c_sPosUp = "Up";
        void InitPosition()
        {
            m_axis.AddPos(GetPosString(eWorks.A));
            m_axis.AddPos(GetPosString(eWorks.B));
            m_axis.AddPos(ePosTransfer.Transfer0.ToString());
            m_axis.AddPos(ePosTransfer.Transfer7.ToString());
            m_axis.AddPos(ePosTray.Tray0.ToString());
            m_axis.AddPos(ePosTray.Tray7.ToString());
            m_axis.p_axisZ.AddPos(c_sPosUp);
        }
        string GetPosString(eWorks eWorks)
        {
            return eVision.Bottom.ToString() + eWorks.ToString();
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
                m_alidCrash.p_bSet = m_diCrash.p_bIn; 
            }
        }
        #endregion

        #region AvoidX
        Loader0 p_loader0 { get { return m_handler.m_loader0; } }
        string StartMoveX(string sPos, double xOffset)
        {
            Axis axisX = p_loader0.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(sPos) + xOffset;
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
        public string RunMoveBoat(eWorks eWorks, bool bWait = true)
        {
            string sPos = GetPosString(eWorks);
            if (Run(StartMoveX(sPos, 0))) return p_sInfo;
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        double GetXOffset(InfoStrip.eMagazine ePos)
        {
            double xScale = m_transfer.m_buffer.GetXScale(ePos);
            double p0 = m_axis.p_axisX.GetPosValue(ePosTransfer.Transfer0.ToString());
            double p7 = m_axis.p_axisX.GetPosValue(ePosTransfer.Transfer7.ToString());
            return xScale * (p7 - p0);
        }

        public string RunMoveTransfer(ePosTransfer ePos, double xOffset, bool bWait = true)
        {
            xOffset += GetXOffset((InfoStrip.eMagazine)ePos); 
            if (Run(StartMoveX(ePosTransfer.Transfer0.ToString(), xOffset))) return p_sInfo;
            m_axis.p_axisY.StartMove(ePosTransfer.Transfer0);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveTray(ePosTray ePos, bool bWait = true)
        {
            double xOffset = GetXOffset((InfoStrip.eMagazine)ePos);
            if (Run(StartMoveX(ePosTray.Tray0.ToString(), xOffset))) return p_sInfo;
            m_axis.p_axisY.StartMove(ePosTray.Tray0);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region AxisZ
        public string RunMoveZ(eWorks eWorks, double dPos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(GetPosString(eWorks), -dPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePosTransfer.Transfer7);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTray ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePosTray.Tray7);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            try
            {
                m_axis.p_axisZ.StartMove(c_sPosUp);
                return bWait ? m_axis.WaitReady() : "OK";
            }
            finally { m_axis.p_axisZ.m_bCheckStop = true; }
        }
        #endregion

        #region RunLoad
        string StartLoadBoat()
        {
            Boats boats = m_handler.m_aBoats[eVision.Bottom];
            eWorks eWork = (1 - m_eWorksLoad); 
            if (boats.m_aBoat[eWork].p_eStep == Boat.eStep.Done) return StartLoadBoat(eWork);
            if (boats.m_aBoat[m_eWorksLoad].p_eStep == Boat.eStep.Done) return StartLoadBoat(m_eWorksLoad);
            return "OK";
        }

        string StartLoadBoat(eWorks eWorks)
        {
            Run_LoadBoat run = (Run_LoadBoat)m_runLoadBoat.Clone();
            run.m_eWorks = eWorks;
            return StartRun(run);
        }

        eWorks m_eWorksLoad = eWorks.A;
        public string RunLoad(eWorks eWorks)
        {
            Boats boats = m_handler.m_aBoats[eVision.Bottom];
            Boat boat = boats.m_aBoat[eWorks]; 
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (boat.p_eStep != Boat.eStep.Done) return "Boat not Done";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(boats.RunMoveDone(eWorks))) return p_sInfo;
                if (Run(RunMoveBoat(eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eWorks, 0))) return p_sInfo;
                boat.RunVacuum(false);
                boat.RunBlow(true);
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                Thread.Sleep(500);
                boat.RunBlow(false);
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false)
                {
                    m_picker.RunVacuum(false); 
                    return p_sInfo;
                }
                m_picker.p_infoStrip = boat.p_infoStrip;
                boat.p_infoStrip = null;
                boat.p_eStep = Boat.eStep.RunReady;
                m_eWorksLoad = eWorks; 
            }
            finally
            {
                RunMoveUp(); 
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string StartUnloadStrip()
        {
            return StartRun(m_runUnloadStrip); 
        }

        public string RunUnloadStrip()
        {
            if (p_infoStrip == null) return "OK";
            Boats boats = m_handler.m_aBoats[eVision.Bottom];
            if (boats.IsBusy()) return "OK";
            if (boats.p_eState != eState.Ready) return "OK";
            Boat boat = boats.m_aBoat[p_infoStrip.m_eWorks];
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(boats.m_aBoat[p_infoStrip.m_eWorks].RunMove(Boat.ePos.Handler))) return p_sInfo;
                if (Run(RunMoveBoat(p_infoStrip.m_eWorks))) return p_sInfo;
                if (Run(RunMoveZ(p_infoStrip.m_eWorks, 0))) return p_sInfo;
                boat.RunVacuum(true);
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                Thread.Sleep(500);
                if (Run(RunMoveUp())) return p_sInfo;
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                boat.p_eStep = Boat.eStep.Done;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        string StartUnloadTransfer() 
        {
            Run_UnloadTransfer run = (Run_UnloadTransfer)m_runUnloadTransfer.Clone();
            return StartRun(run);
        }

        public string RunUnloadTransfer() 
        {
            if (m_picker.p_infoStrip == null) return "InfoStrip == null";
            if (m_transfer.m_pusher.p_bEnable == false) return "Buffer Pusher not Enable";
            try
            {
                ePosTransfer ePos = (ePosTransfer)m_transfer.m_buffer.m_ePosDst;
                double xOffset = m_transfer.m_buffer.m_xOffset;
                m_transfer.m_pusher.p_bLock = true;
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTransfer(ePos, xOffset))) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(RunMoveUp())) return p_sInfo;
                m_transfer.m_pusher.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                m_transfer.m_pusher.p_bLock = false;
                if (Run(RunMoveBoat(1- m_eWorksLoad))) return p_sInfo;
            }
            finally
            {
                RunMoveUp(); 
                m_transfer.m_pusher.p_bLock = false;
            }
            return "OK";
        }

        string StartUnloadTray()
        {
            if (m_picker.p_infoStrip.p_bInspect) return "OK";
            Run_UnloadTray run = (Run_UnloadTray)m_runUnloadTray.Clone();
            return StartRun(run);
        }

        public string RunUnloadTray()
        {
            if (m_picker.p_infoStrip == null) return "InfoStrip == null";
            try
            {
                ePosTray ePosTray = ePosTray.Tray0;
                if (CalcTrayPos(ref ePosTray) != "OK")
                {
                    EQ.p_eState = EQ.eState.Ready;
                    m_pine2.m_buzzer.RunBuzzer(Pine2.eBuzzer.Warning);
                    m_alidSorter.p_bSet = true; 
                    Thread.Sleep(1000); 
                    return "OK";
                }
                m_alidSorter.p_bSet = false;
                foreach (MagazineEV magazineEV in m_handler.m_magazineEVSet.m_aEV.Values) magazineEV.m_conveyor.m_bInv = false;
                MagazineEV magazine = m_handler.m_magazineEVSet.m_aEV[(InfoStrip.eMagazine)ePosTray];
                magazine.m_conveyor.m_bInv = true; 
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTray(ePosTray))) return p_sInfo;
                if (Run(RunMoveZ(ePosTray))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                if (Run(RunMoveUp())) return p_sInfo;
                m_picker.p_infoStrip.m_iBundle = magazine.m_stack.p_iBundle;
                m_handler.SendSortInfo(m_picker.p_infoStrip); 
                m_picker.p_infoStrip = null;
                magazine.PutInfoStrip(m_picker.p_infoStrip);
                if (Run(RunMoveBoat(1 - m_eWorksLoad))) return p_sInfo;
                m_handler.CheckFinish(); 
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        string CalcTrayPos(ref ePosTray eTray)
        {
            MagazineEVSet magazine = m_handler.m_magazineEVSet;
            while (m_picker.p_infoStrip.p_bInspect)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            InfoStrip.eResult eResult = m_picker.p_infoStrip.GetResult(); 
            foreach (ePosTray ePosTray in Enum.GetValues(typeof(ePosTray)))
            {
                if (magazine.IsEnableStack((InfoStrip.eMagazine)ePosTray, eResult, true))
                {
                    eTray = ePosTray;
                    return "OK";
                }
            }
            foreach (ePosTray ePosTray in Enum.GetValues(typeof(ePosTray)))
            {
                if (magazine.IsEnableStack((InfoStrip.eMagazine)ePosTray, eResult, false))
                {
                    eTray = ePosTray;
                    return "OK";
                }
            }
            return "Error";
        }

        #endregion

        #region PickerSet
        double m_mmPickerSetUp = 10;
        double m_secPickerSet = 7;
        public string RunPickerSet(eWorks eWorks)
        {
            StopWatch sw = new StopWatch();
            long msPickerSet = (long)(1000 * m_secPickerSet);
            try
            {
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
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome(m_axis.p_axisZ);
            if (p_sInfo != "OK") return p_sInfo;
            RunMoveUp(); 
            p_sInfo = base.StateHome(m_axis.p_axisX, m_axis.p_axisY);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

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

        public override void Reset()
        {
            m_picker.m_dioVacuum.Write(false);
            m_picker.p_infoStrip = null;
            RunMoveUp(false); 
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
        ModuleRunBase m_runUnloadStrip;
        ModuleRunBase m_runUnloadTransfer;
        ModuleRunBase m_runUnloadTray;
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runLoadBoat = AddModuleRunList(new Run_LoadBoat(this), true, "Load Strip from Boat");
            m_runUnloadStrip = AddModuleRunList(new Run_UnloadStrip(this), false, "Unload Strip to GetPosition");
            m_runUnloadTransfer = AddModuleRunList(new Run_UnloadTransfer(this), true, "Unload Strip to Transfer");
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

            public eWorks m_eWorks = eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_LoadBoat run = new Run_LoadBoat(m_module);
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eWorks);
            }
        }

        public class Run_UnloadStrip : ModuleRunBase
        {
            Loader3 m_module;
            public Run_UnloadStrip(Loader3 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_UnloadStrip run = new Run_UnloadStrip(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadStrip();
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

            public override ModuleRunBase Clone()
            {
                Run_UnloadTransfer run = new Run_UnloadTransfer(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadTransfer();
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

            public override ModuleRunBase Clone()
            {
                Run_UnloadTray run = new Run_UnloadTray(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadTray();
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

            eWorks m_eWorks = eWorks.B;
            public override ModuleRunBase Clone()
            {
                Run_PickerSet run = new Run_PickerSet(m_module);
                run.m_eWorks = m_eWorks; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "Select Boat", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunPickerSet(m_eWorks);
            }
        }
        #endregion
    }

}
