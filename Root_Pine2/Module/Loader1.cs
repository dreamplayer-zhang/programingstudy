﻿using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Loader1 : ModuleBase
    {
        #region ToolBox
        public AxisXY m_axisXZ;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXZ, this, "Loader1");
            m_picker.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }

        const string c_sTurnover = "Turnover";
        const string c_sUp = "Up";
        void InitPosition()
        {
            m_axisXZ.AddPos(c_sTurnover);
            m_axisXZ.AddPos(GetPosString(Vision2D.eVision.Top3D, Vision2D.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision2D.eVision.Top3D, Vision2D.eWorks.B));
            m_axisXZ.AddPos(GetPosString(Vision2D.eVision.Top2D, Vision2D.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision2D.eVision.Top2D, Vision2D.eWorks.B));
            m_axisXZ.p_axisY.AddPos(c_sUp);
        }

        string GetPosString(Vision2D.eVision eVision, Vision2D.eWorks eVisionWorks)
        {
            return eVision.ToString() + eVisionWorks.ToString();
        }

        public string RunMoveX(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision2D.eVision eVision, Vision2D.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision2D.eVision eVision, Vision2D.eWorks ePos, double dPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(eVision, ePos), -dPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(c_sUp);
            return bWait ? m_axisXZ.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
        public enum eLoad
        {
            Top3D,
            Top2D,
        }
        public string RunLoad(eLoad eLoad, Vision2D.eWorks eWorks)
        {
            Vision2D.eVision eVision = Vision2D.eVision.Top3D;
            switch (eLoad)
            {
                case eLoad.Top3D: eVision = Vision2D.eVision.Top3D; break;
                case eLoad.Top2D: eVision = Vision2D.eVision.Top2D; break;
            }
            Boats boats = m_handler.m_aBoats[eVision];
            Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Done) return "Boat not Done";
            if (m_picker.p_infoStrip != null) return "Picker has InfoStrip";
            try
            {
                boat.p_infoStrip.m_eVisionLoad = eVision;
                boat.p_infoStrip.m_eWorks = eWorks; 
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(boats.RunMoveReady(eWorks))) return p_sInfo;
                if (Run(RunMoveX(eVision, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eWorks, 0))) return p_sInfo;
                boat.RunVacuum(false);
                boat.RunBlow(true); 
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                boat.RunBlow(false);
                if (Run(RunMoveUp())) return p_sInfo;
                if (m_picker.IsVacuum() == false) return "Pick Strip Error";
                m_picker.p_infoStrip = boat.p_infoStrip;
                boat.p_infoStrip = null;
                boat.p_eStep = Boat.eStep.RunReady; 
            }
            finally
            {
                boat.RunBlow(false);
                RunMoveUp();
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnload()
        {
            Boats boats = m_handler.m_aBoats[Vision2D.eVision.Top2D];
            Vision2D.eWorks eWorks = p_infoStrip.m_eWorks; 
            Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(boats.RunMoveReady(eWorks))) return p_sInfo;
                if (Run(RunMoveX(Vision2D.eVision.Top2D, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(Vision2D.eVision.Top2D, eWorks, 0))) return p_sInfo;
                boat.RunVacuum(true);
                m_picker.RunVacuum(false);
                if (Run(RunMoveUp(false))) return p_sInfo;
                Thread.Sleep(200); 
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(m_axisXZ.WaitReady())) return p_sInfo;
            }
            finally
            {
                RunMoveUp();
            }
            return "OK";
        }

        public string RunUnloadTurnover()
        {
            Loader2 loader2 = m_handler.m_loader2;
            if (loader2.p_eState != eState.Ready) return "Loader1 is not Ready";
            if (loader2.m_qModuleRun.Count > 0) return "Loader1 is not Ready";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveX(c_sTurnover))) return p_sInfo;
                if (Run(RunMoveZ(c_sTurnover))) return p_sInfo;
                loader2.RunVacuum(true);
                m_picker.RunVacuum(false);
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveX(Vision2D.eVision.Top2D, Vision2D.eWorks.B))) return p_sInfo; 
                loader2.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
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
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveX(Vision2D.eVision.Top3D, Vision2D.eWorks.A))) return p_sInfo; 
                while (true)
                {
                    if (Run(RunMoveZ(Vision2D.eVision.Top3D, Vision2D.eWorks.A, 0))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(false))) return p_sInfo;
                    double sec = 0;
                    if (Run(m_pine2.WaitPickerSet(ref sec))) return p_sInfo;
                    if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                    if (Run(RunMoveZ(Vision2D.eVision.Top3D, Vision2D.eWorks.A, 1000 * m_mmPickerSetUp))) return p_sInfo;
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
            p_sInfo = base.StateHome(m_axisXZ.p_axisY);
            if (p_sInfo != "OK") return p_sInfo;
            RunMoveUp(); 
            p_sInfo = base.StateHome(m_axisXZ.p_axisX);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (m_picker.p_infoStrip != null)
            {
                switch (m_picker.p_infoStrip.m_eVisionLoad)
                {
                    case Vision2D.eVision.Top3D: return StartUnloadBoat();
                    case Vision2D.eVision.Top2D: return StartUnloadTurnover(); 
                }
            }
            else
            {
                Boats boats2D = m_handler.m_aBoats[Vision2D.eVision.Top2D];
                Boats boats3D = m_handler.m_aBoats[Vision2D.eVision.Top3D];
                foreach (Vision2D.eWorks eWorks in Enum.GetValues(typeof(Vision2D.eWorks)))
                {
                    if (boats3D.m_aBoat[eWorks].IsDone())
                    {
                        if (boats2D.m_aBoat[eWorks].p_infoStrip == null) return StartLoad(Vision2D.eVision.Top3D, eWorks);
                    }
                }
                foreach (Vision2D.eWorks eWorks in Enum.GetValues(typeof(Vision2D.eWorks)))
                {
                    if (boats2D.m_aBoat[eWorks].IsDone()) return StartLoad(Vision2D.eVision.Top2D, eWorks);
                }
            }
            return "OK";
        }

        public override void Reset()
        {
            base.Reset();
            m_picker.p_infoStrip = null;
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_picker.RunTreeVacuum(tree.GetTree("Vacuum"));
            RunTreePickerSet(tree.GetTree("PickerSet"));
        }
        #endregion

        #region Start Run
        string StartUnloadBoat()
        {
            if (p_infoStrip == null) return "p_infoStrip == null";
            Vision2D.eWorks eWorks = p_infoStrip.m_eWorks; 
            Boats boats = m_handler.m_aBoats[Vision2D.eVision.Top2D];
            if (boats.m_aBoat[eWorks].p_eStep == Boat.eStep.Ready) return StartUnloadBoat(eWorks);
            return "OK";
        }

        string StartUnloadBoat(Vision2D.eWorks eWorks)
        {
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            run.m_eWorks = eWorks;
            return StartRun(run);
        }

        string StartUnloadTurnover()
        {
            if (m_handler.m_loader2.p_eState != eState.Ready) return "OK";
            return StartRun(m_runUnloadTurnover);
        }

        string StartLoad(Vision2D.eVision eVision, Vision2D.eWorks eWorks)
        {
            Run_Load run = (Run_Load)m_runLoad.Clone();
            run.m_eLoad = (eVision == Vision2D.eVision.Top3D) ? eLoad.Top3D : eLoad.Top2D;
            run.m_eWorks = eWorks;
            return StartRun(run);
        }

        #endregion

        public InfoStrip p_infoStrip { get { return m_picker.p_infoStrip; } }
        Picker m_picker = null;
        Pine2 m_pine2 = null;
        Pine2_Handler m_handler = null; 
        public Loader1(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_handler = handler; 
            m_pine2 = handler.m_pine2;
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop();
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnload;
        ModuleRunBase m_runUnloadTurnover;
        protected override void InitModuleRuns()
        {
            m_runLoad = AddModuleRunList(new Run_Load(this), true, "Load Strip from Boat");
            m_runUnload = AddModuleRunList(new Run_Unload(this), true, "Unload Strip to Boat");
            m_runUnloadTurnover = AddModuleRunList(new Run_UnloadTurnover(this), true, "Unload Strip to Turnover");
            AddModuleRunList(new Run_PickerSet(this), false, "Picker Set");
        }

        public class Run_Load : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Load(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eLoad m_eLoad = eLoad.Top3D; 
            public Vision2D.eWorks m_eWorks = Vision2D.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_eLoad = m_eLoad;
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eLoad = (eLoad)tree.Set(m_eLoad, m_eLoad, "Vision", "Select Vision", bVisible); 
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eLoad, m_eWorks);
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Unload(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Vision2D.eWorks m_eWorks = Vision2D.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnload();
            }
        }

        public class Run_UnloadTurnover : ModuleRunBase
        {
            Loader1 m_module;
            public Run_UnloadTurnover(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_UnloadTurnover run = new Run_UnloadTurnover(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnloadTurnover(); 
            }
        }

        public class Run_PickerSet : ModuleRunBase
        {
            Loader1 m_module;
            public Run_PickerSet(Loader1 module)
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
