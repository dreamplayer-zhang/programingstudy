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
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        public Axis3D m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Loader0");
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
            Transfer8
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
        public enum eUnloadVision
        {
            Top3D,
            Top2D,
        }
        const string c_sPosLoadEV = "LoadEV";
        void InitPosition()
        {
            m_axis.AddPos(c_sPosLoadEV);
            m_axis.AddPos(Enum.GetNames(typeof(ePosTransfer)));
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, Vision2D.eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, Vision2D.eWorks.B));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, Vision2D.eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, Vision2D.eWorks.B));
            m_axis.AddPos(Enum.GetNames(typeof(ePosTray)));
        }
        string GetPosString(eUnloadVision eVision, Vision2D.eWorks eWorks)
        {
            return eVision.ToString() + eWorks.ToString(); 
        }
        #endregion

        #region AvoidX
        public const double c_lAxisX = 1557800;
        Loader3 p_loader3 { get { return m_handler.m_loader3; } }
        string StartMoveX(string sPos, double dPos)
        {
            Axis axisX = p_loader3.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(sPos) + dPos;
            while ((fPos + axisX.m_posDst) > c_lAxisX)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_loader3.IsBusy() == false)
                {
                    p_loader3.StartAvoidX(fPos);
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

        #region AxisXY
        public string RunMoveTransfer(ePosTransfer ePos, bool bWait = true)
        {
            if (Run(StartMoveX(ePos.ToString(), 0))) return p_sInfo; 
            m_axis.p_axisY.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveBoat(eUnloadVision eVision, Vision2D.eWorks eWorks, bool bWait = true)
        {
            string sPos = GetPosString(eVision, eWorks);
            if (Run(StartMoveX(sPos, 0))) return p_sInfo;
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveTray(ePosTray eTray, bool bWait = true)
        {
            if (Run(StartMoveX(eTray.ToString(), 0))) return p_sInfo;
            m_axis.p_axisY.StartMove(eTray.ToString());
            return bWait ? m_axis.WaitReady() : "OK";
        }

        int m_pulsemm = 1000; 
        public string RunMoveLoadEV(bool bWait = true)
        {
            double dPos = m_pulsemm * (95 - m_pine2.p_widthStrip);
            if (Run(StartMoveX(c_sPosLoadEV, dPos))) return p_sInfo;
            m_axis.p_axisY.StartMove(c_sPosLoadEV);
            return bWait ? m_axis.WaitReady() : "OK"; 
        }

        void RunTreeAxis(Tree tree)
        {
            m_pulsemm = tree.Set(m_pulsemm, m_pulsemm, "Pulse / mm", "Axis X (Pulse / mm)");
        }
        #endregion

        #region AxisZ
        public string RunMoveZ(string sPos, double dPos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(sPos, -dPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(eUnloadVision eVision, Vision2D.eWorks eWorks, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(GetPosString(eVision, eWorks));
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZPaper(ePosTray eTray, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(eTray);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(0);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunShakeUp(int nShake, int dzPulse)
        {
            int iShake = 0; 
            while (iShake < nShake)
            {
                if (Run(RunShakeUp(-dzPulse))) return p_sInfo; 
                if (Run(RunShakeUp(0.9 * dzPulse))) return p_sInfo;
                iShake++;
            }
            return "OK"; 
        }

        string RunShakeUp(double dzPulse)
        {
            m_axis.p_axisZ.StartShift(dzPulse);
            return m_axis.WaitReady(); 
        }
        #endregion

        #region RunLoad
        public string RunLoadEV(int nShake, int dzShakeUp)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (m_loadEV.p_bDone == false) return "LoadEV not Done";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveLoadEV())) return p_sInfo;
                if (Run(RunMoveZ(c_sPosLoadEV, 0))) return p_sInfo;
                m_loadEV.p_bBlow = true;
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                m_loadEV.p_eMove = LoadEV.eMove.Down; 
                if (Run(RunShakeUp(nShake, dzShakeUp))) return p_sInfo;
                m_loadEV.p_eMove = LoadEV.eMove.Stop;
                m_loadEV.p_bBlow = false;
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo;
                m_picker.p_infoStrip = m_loadEV.GetNewInfoStrip();
                m_loadEV.StartLoad(); 
            }
            finally
            {
                m_loadEV.p_eMove = LoadEV.eMove.Stop;
                m_loadEV.p_bBlow = false;
                RunMoveUp();
            }
            return "OK";
        }

        public string RunLoadTransfer(ePosTransfer ePos)
        {
            Transfer.Gripper gripper = m_transfer.m_gripper;
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (gripper.p_bEnable == false) return "Load from Transfer not Enable";
            try
            {
                gripper.p_bLock = true;
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveTransfer(ePos))) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo;
                m_picker.p_infoStrip = gripper.p_infoStrip;
                gripper.p_infoStrip = null;
            }
            finally
            {
                RunMoveUp();
                gripper.p_bLock = false;
            }
            return "OK"; 
        }
        #endregion

        #region RunUnload
        public string RunUnloadPaper()
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                ePosTray ePosTray = ePosTray.Tray8;
                if (Run(GetPaperTray(ref ePosTray))) return p_sInfo; 
                if (Run(RunMoveTray(ePosTray))) return p_sInfo;
                if (Run(RunMoveZPaper(ePosTray))) return p_sInfo;
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                m_picker.p_infoStrip = null;
                MagazineEV magazine = m_handler.m_magazineEV.m_aEV[(InfoStrip.eMagazine)ePosTray];
                magazine.PutInfoStrip(null); 
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveLoadEV())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        string GetPaperTray(ref ePosTray ePosTray)
        {
            MagazineEVSet magazine = m_handler.m_magazineEV;
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                if (magazine.IsEnableStack(eMagazine, InfoStrip.eResult.Paper, true))
                {
                    ePosTray = (ePosTray)eMagazine; 
                    return "OK";
                }
            }
            foreach (InfoStrip.eMagazine eMagazine in Enum.GetValues(typeof(InfoStrip.eMagazine)))
            {
                if (magazine.IsEnableStack(eMagazine, InfoStrip.eResult.Paper, false)) 
                {
                    ePosTray = (ePosTray)eMagazine;
                    return "OK";
                }
            }
            return "Paper Tray not Ready"; 
        }

        public string RunUnloadBoat(eUnloadVision eVision, Vision2D.eWorks eWorks)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            Boats boats = GetBoats(eVision);
            Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveBoat(eVision, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eWorks))) return p_sInfo;
                boat.RunVacuum(true);
                if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                boat.p_infoStrip.m_eWorks = eWorks;
                if (Run(RunMoveUp())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        Boats GetBoats(eUnloadVision eVision)
        {
            switch (eVision)
            {
                case eUnloadVision.Top3D: return m_handler.m_aBoats[Vision2D.eVision.Top3D];
                case eUnloadVision.Top2D: return m_handler.m_aBoats[Vision2D.eVision.Top2D];
            }
            return null; 
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
                string sPick = (m_pine2.p_eMode == Pine2.eRunMode.Stack) ? c_sPosLoadEV : ePosTransfer.Transfer8.ToString();
                while (true)
                {
                    if (Run(RunMoveZ(sPick, 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                    double sec = 0;
                    if (Run(m_pine2.WaitPickerSet(ref sec))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                    if (Run(RunMoveZ(sPick, m_pulsemm * m_mmPickerSetUp))) return p_sInfo;
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
                if (m_picker.p_infoStrip.m_bPaper) return StartRun(m_runUnloadPaper); //forget
                return StartUnloadBoat();
            }
            else
            {
                switch (m_pine2.p_eMode)
                {
                    case Pine2.eRunMode.Stack: return m_loadEV.p_bDone ? StartRun(m_runLoadEV) : "OK";
                    case Pine2.eRunMode.Magazine: return m_transfer.m_gripper.p_bEnable ? StartLoadTransfer() : "OK";
                }
            }
            return "OK";
        }

        string StartUnloadBoat()
        {
            Vision2D.eVision eVision = m_pine2.p_b3D ? Vision2D.eVision.Top3D : Vision2D.eVision.Top2D;
            Boats boats = m_handler.m_aBoats[eVision];
            if (boats.m_aBoat[Vision2D.eWorks.A].p_eStep == Boat.eStep.Ready) return StartUnloadBoat(eVision, Vision2D.eWorks.A);
            if (boats.m_aBoat[Vision2D.eWorks.B].p_eStep == Boat.eStep.Ready) return StartUnloadBoat(eVision, Vision2D.eWorks.B);
            return "OK"; 
        }

        string StartUnloadBoat(Vision2D.eVision eVision, Vision2D.eWorks eWorks)
        {
            Run_UnloadBoat run = (Run_UnloadBoat)m_runUnloadBoat.Clone();
            run.m_eVision = (eVision == Vision2D.eVision.Top3D) ? eUnloadVision.Top3D : eUnloadVision.Top2D;
            run.m_eWorks = eWorks;
            return StartRun(run); 
        }

        string StartLoadTransfer()
        {
            Run_LoadTransfer run = (Run_LoadTransfer)m_runLoadTransfer.Clone();
            run.m_ePos = (ePosTransfer)m_transfer.m_buffer.m_ePosDst;
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
            RunTreeAxis(tree.GetTree("Axis"));
            RunTreePickerSet(tree.GetTree("PickerSet")); 
        }
        #endregion

        public InfoStrip p_infoStrip { get { return m_picker.p_infoStrip; } }
        Picker m_picker = null;
        Pine2 m_pine2 = null;
        LoadEV m_loadEV = null;
        Transfer m_transfer = null;
        Pine2_Handler m_handler; 
        public Loader0(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_pine2 = handler.m_pine2;
            m_loadEV = handler.m_loadEV;
            m_transfer = handler.m_transfer;
            m_handler = handler; 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop(); 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoadEV;
        ModuleRunBase m_runLoadTransfer;
        ModuleRunBase m_runUnloadPaper;
        ModuleRunBase m_runUnloadBoat;
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runLoadEV = AddModuleRunList(new Run_LoadEV(this), true, "Load Strip from LoadEV");
            m_runLoadTransfer = AddModuleRunList(new Run_LoadTransfer(this), true, "Load Strip from Transfer");
            m_runUnloadPaper = AddModuleRunList(new Run_UnloadPaper(this), true, "Unload Paper to Tray");
            m_runUnloadBoat = AddModuleRunList(new Run_UnloadBoat(this), true, "Unload Paper to Boat");
            m_runAvoidX = AddModuleRunList(new Run_AvoidX(this), false, "Avoid Axis X");
            AddModuleRunList(new Run_PickerSet(this), false, "Avoid Axis X");
        }

        public class Run_LoadEV : ModuleRunBase
        {
            Loader0 m_module;
            public Run_LoadEV(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            int m_nShake = 2;
            int m_dzShakeUp = 5000; 
            public override ModuleRunBase Clone()
            {
                Run_LoadEV run = new Run_LoadEV(m_module);
                run.m_nShake = m_nShake;
                run.m_dzShakeUp = m_dzShakeUp;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_nShake = tree.Set(m_nShake, m_nShake, "Shake", "Shake Count", bVisible);
                m_dzShakeUp = tree.Set(m_dzShakeUp, m_dzShakeUp, "ShakeUp", "Shake Up (Pulse)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoadEV(m_nShake, m_dzShakeUp);
            }
        }

        public class Run_LoadTransfer : ModuleRunBase
        {
            Loader0 m_module;
            public Run_LoadTransfer(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePosTransfer m_ePos = ePosTransfer.Transfer1;
            public override ModuleRunBase Clone()
            {
                Run_LoadTransfer run = new Run_LoadTransfer(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePosTransfer)tree.Set(m_ePos, m_ePos, "Transfer", "Select Transfer", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoadTransfer(m_ePos);
            }
        }

        public class Run_UnloadPaper : ModuleRunBase
        {
            Loader0 m_module;
            public Run_UnloadPaper(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_UnloadPaper run = new Run_UnloadPaper(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadPaper(); 
            }
        }

        public class Run_UnloadBoat : ModuleRunBase
        {
            Loader0 m_module;
            public Run_UnloadBoat(Loader0 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eUnloadVision m_eVision = eUnloadVision.Top3D;
            public Vision2D.eWorks m_eWorks = Vision2D.eWorks.A; 
            public override ModuleRunBase Clone()
            {
                Run_UnloadBoat run = new Run_UnloadBoat(m_module);
                run.m_eVision = m_eVision;
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eVision = (eUnloadVision)tree.Set(m_eVision, m_eVision, "Vision", "Select Vision", bVisible);
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadBoat(m_eVision, m_eWorks); 
            }
        }

        public class Run_AvoidX : ModuleRunBase
        {
            Loader0 m_module;
            public Run_AvoidX(Loader0 module)
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
            Loader0 m_module;
            public Run_PickerSet(Loader0 module)
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
