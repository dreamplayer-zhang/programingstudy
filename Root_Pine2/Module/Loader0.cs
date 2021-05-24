using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_Pine2.Module
{
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        Axis3D m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "Loader0");
            m_picker.GetTools(m_toolBox, this, bInit); 
            if (bInit) InitPosition();
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
        public enum eUnloadVision
        {
            Top3D,
            Top2D,
        }
        const string c_sPosLoadEV = "LoadEV";
        const string c_sPosPaper = "PaperTray"; 
        void InitPosition()
        {
            m_axis.AddPos(c_sPosLoadEV);
            m_axis.AddPos(Enum.GetNames(typeof(ePosTransfer)));
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, Vision.eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top3D, Vision.eWorks.B));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, Vision.eWorks.A));
            m_axis.AddPos(GetPosString(eUnloadVision.Top2D, Vision.eWorks.B));
            m_axis.AddPos(c_sPosPaper); 
        }
        string GetPosString(eUnloadVision eVision, Vision.eWorks eWorks)
        {
            return eVision.ToString() + eWorks.ToString(); 
        }
        #endregion

        #region AxisXY
        public string RunMoveTransfer(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisX.StartMove(ePos);
            m_axis.p_axisY.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveBoat(eUnloadVision eVision, Vision.eWorks eWorks, bool bWait = true)
        {
            string sPos = GetPosString(eVision, eWorks);
            m_axis.p_axisX.StartMove(sPos);
            m_axis.p_axisY.StartMove(sPos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMovePaperTray(bool bWait = true)
        {
            m_axis.p_axisX.StartMove(c_sPosPaper);
            m_axis.p_axisY.StartMove(c_sPosPaper);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        int m_pulsemm = 1000; 
        public string RunMoveLoadEV(bool bWait = true)
        {
            double dPos = m_pulsemm * (77 - m_pine2.p_widthStrip);
            m_axis.p_axisX.StartMove(c_sPosLoadEV, dPos);
            m_axis.p_axisY.StartMove(c_sPosLoadEV);
            return bWait ? m_axis.WaitReady() : "OK"; 
        }

        void RunTreeAxis(Tree tree)
        {
            m_pulsemm = tree.Set(m_pulsemm, m_pulsemm, "Pulse / mm", "Axis X (Pulse / mm)");
        }
        #endregion

        #region AxisZ
        public string RunMoveZLoadEV(bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(c_sPosLoadEV);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosTransfer ePos, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZ(eUnloadVision eVision, Vision.eWorks eWorks, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(GetPosString(eVision, eWorks));
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveZPaper(bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(c_sPosPaper);
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
            return RunMoveUp(); 
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
            if (Run(RunMoveUp())) return p_sInfo;
            if (Run(RunMoveLoadEV())) return p_sInfo;
            if (Run(RunMoveZLoadEV())) return p_sInfo;
            if (Run(m_picker.RunVacuum(true))) return p_sInfo;
            if (Run(RunShakeUp(nShake, dzShakeUp))) return p_sInfo;
            if (m_picker.IsVacuum() == false) return p_sInfo;
            m_picker.p_infoStrip = m_loadEV.GetNewInfoStrip();
            return "OK";
        }

        public string RunLoadTransfer(ePosTransfer ePos)
        {
            Transfer.Buffer.Gripper gripper = m_transfer.m_buffer.m_gripper;
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
                gripper.p_bLock = false; 
            }
            return "OK"; 
        }
        #endregion

        #region RunUnload
        public string RunUnloadPaper()
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            if (Run(RunMoveUp())) return p_sInfo;
            if (Run(RunMovePaperTray())) return p_sInfo;
            if (Run(RunMoveZPaper())) return p_sInfo;
            if (Run(m_picker.RunVacuum(false))) return p_sInfo;
            m_picker.p_infoStrip = null;
            if (Run(RunMoveUp())) return p_sInfo;
            if (Run(RunMoveLoadEV())) return p_sInfo;
            return "OK";
        }

        public string RunUnloadBoat(eUnloadVision eVision, Vision.eWorks eWorks)
        {
            if (m_picker.p_infoStrip != null) return "InfoStrip != null";
            Boats boats = GetBoats(eVision);
            Boats.Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boats.Boat.eStep.Ready) return "Boat not Ready";
            if (Run(RunMoveUp())) return p_sInfo;
            if (Run(RunMoveBoat(eVision, eWorks))) return p_sInfo;
            if (Run(RunMoveZ(eVision, eWorks))) return p_sInfo;
            boat.RunVacuum(true); 
            if (Run(m_picker.RunVacuum(false))) return p_sInfo;
            boat.p_infoStrip = m_picker.p_infoStrip;
            m_picker.p_infoStrip = null;
            if (Run(RunMoveUp())) return p_sInfo;
            return "OK";
        }

        Boats GetBoats(eUnloadVision eVision)
        {
            switch (eVision)
            {
                case eUnloadVision.Top3D: return m_handler.m_aBoats[Vision.eVision.Top3D];
                case eUnloadVision.Top2D: return m_handler.m_aBoats[Vision.eVision.Top2D];
            }
            return null; 
        }
        #endregion

        #region override
        public override string StateReady()
        {
            if (EQ.p_eState == EQ.eState.Run)
            {
                if (m_picker.p_infoStrip != null)
                {
                    if (m_picker.p_infoStrip.m_bPaper) return StartRun(m_runUnloadPaper);
                    return StartUnloadBoat(); 
                }
                else
                {
                    switch (m_pine2.p_eMode)
                    {
                        case Pine2.eRunMode.Stack:
                            if (m_loadEV.p_bDone) return StartRun(m_runLoadEV);
                            return "OK";
                        case Pine2.eRunMode.Magazine:
                            if (m_transfer.m_buffer.m_gripper.p_bEnable) return StartLoadTransfer();
                            return "OK"; 
                    }
                }
            }
            return "OK";
        }

        string StartUnloadBoat()
        {
            Vision.eVision eVision = m_pine2.p_b3D ? Vision.eVision.Top3D : Vision.eVision.Top2D;
            Boats boats = m_handler.m_aBoats[eVision];
            if (boats.m_aBoat[Vision.eWorks.A].p_eStep == Boats.Boat.eStep.Ready) return StartUnloadBoat(eVision, Vision.eWorks.A);
            if (boats.m_aBoat[Vision.eWorks.B].p_eStep == Boats.Boat.eStep.Ready) return StartUnloadBoat(eVision, Vision.eWorks.B);
            return "OK"; 
        }

        string StartUnloadBoat(Vision.eVision eVision, Vision.eWorks eWorks)
        {
            Run_UnloadBoat run = (Run_UnloadBoat)m_runUnloadBoat.Clone();
            run.m_eVision = (eVision == Vision.eVision.Top3D) ? eUnloadVision.Top3D : eUnloadVision.Top2D;
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
        }
        #endregion

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
        protected override void InitModuleRuns()
        {
            m_runLoadEV = AddModuleRunList(new Run_LoadEV(this), true, "Load Strip from LoadEV");
            m_runLoadTransfer = AddModuleRunList(new Run_LoadTransfer(this), true, "Load Strip from Transfer");
            m_runUnloadPaper = AddModuleRunList(new Run_UnloadPaper(this), true, "Unload Paper to Paper Tray");
            m_runUnloadBoat = AddModuleRunList(new Run_UnloadBoat(this), true, "Unload Paper to Boat");
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

            public ePosTransfer m_ePos = ePosTransfer.Transfer0;
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
            public Vision.eWorks m_eWorks = Vision.eWorks.A; 
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
                m_eWorks = (Vision.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnloadBoat(m_eVision, m_eWorks); 
            }
        }
        #endregion
    }
}
