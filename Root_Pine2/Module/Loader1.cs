using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_Pine2.Module
{
    public class Loader1 : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXZ;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXZ, this, "Loader1");
            m_picker.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }

        const string c_sTurnover = "Turnover"; 
        void InitPosition()
        {
            m_axisXZ.AddPos(c_sTurnover);
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.B));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.B));
        }

        string GetPosString(Vision.eVision eVision, Vision.eWorks eVisionWorks)
        {
            return eVision.ToString() + eVisionWorks.ToString();
        }

        public string RunMoveX(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(0);
            return bWait ? m_axisXZ.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
        public enum eLoad
        {
            Top3D,
            Top2D,
        }
        public string RunLoad(eLoad eLoad, Vision.eWorks eWorks)
        {
            Vision.eVision eVision = Vision.eVision.Top3D;
            switch (eLoad)
            {
                case eLoad.Top3D: eVision = Vision.eVision.Top3D; break;
                case eLoad.Top2D: eVision = Vision.eVision.Top2D; break;
            }
            Boats boats = m_handler.m_aBoats[eVision];
            Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Done) return "Boat not Done";
            if (m_picker.p_infoStrip != null) return "Picker has InfoStrip";
            try
            {
                boat.p_infoStrip.m_eVision = eVision;
                boat.p_infoStrip.m_eWorks = eWorks; 
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveX(eVision, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eWorks))) return p_sInfo;
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
        public string RunUnload(Vision.eWorks eVisionWorks)
        {
            Boats boats = m_handler.m_aBoats[Vision.eVision.Top2D];
            Boat boat = boats.m_aBoat[eVisionWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveUp())) return p_sInfo;
                if (Run(RunMoveX(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                boat.RunVacuum(true);
                m_picker.RunVacuum(false);
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(RunMoveUp())) return p_sInfo;
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

        #region override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (m_picker.p_infoStrip != null)
            {
                switch (m_picker.p_infoStrip.m_eVision)
                {
                    case Vision.eVision.Top3D: return StartUnloadBoat();
                    case Vision.eVision.Top2D: return StartUnloadTurnover(); 
                }
            }
            else
            {
                Boats boats3D = m_handler.m_aBoats[Vision.eVision.Top3D]; 
                foreach (Vision.eWorks eWorks in Enum.GetValues(typeof(Vision.eWorks)))
                {
                    Boat boat = m_handler.m_aBoats[Vision.eVision.Top2D].m_aBoat[eWorks];
                    switch (boat.p_eStep)
                    {
                        case Boat.eStep.Done: return StartLoad(Vision.eVision.Top2D, eWorks);
                        case Boat.eStep.Ready:
                            if (boats3D.m_aBoat[eWorks].p_eStep == Boat.eStep.Done) return StartLoad(Vision.eVision.Top3D, eWorks);
                            break; 
                    }
                }
                return "OK"; 
            }
            return "OK";
        }

        string StartUnloadBoat()
        {
            Boats boats = m_handler.m_aBoats[Vision.eVision.Top2D];
            if (boats.m_aBoat[Vision.eWorks.A].p_eStep == Boat.eStep.Ready) return StartUnloadBoat(Vision.eWorks.A);
            if (boats.m_aBoat[Vision.eWorks.B].p_eStep == Boat.eStep.Ready) return StartUnloadBoat(Vision.eWorks.B);
            return "OK";
        }

        string StartUnloadBoat(Vision.eWorks eWorks)
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

        string StartLoad(Vision.eVision eVision, Vision.eWorks eWorks)
        {
            Run_Load run = (Run_Load)m_runLoad.Clone();
            run.m_eLoad = (eVision == Vision.eVision.Top3D) ? eLoad.Top3D : eLoad.Top2D;
            run.m_eWorks = eWorks;
            return StartRun(run); 
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
            public Vision.eWorks m_eWorks = Vision.eWorks.A;
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
                m_eWorks = (Vision.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
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

            public Vision.eWorks m_eWorks = Vision.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnload(m_eWorks);
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
        #endregion
    }
}
