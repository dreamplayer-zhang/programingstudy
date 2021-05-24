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

        public enum ePos
        {
            Ready,
            Turnover
        }
        void InitPosition()
        {
            m_axisXZ.AddPos(Enum.GetNames(typeof(ePos)));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.B));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.B));
        }

        string GetPosString(Vision.eVision eVision, Vision.eWorks eVisionWorks)
        {
            return eVision.ToString() + eVisionWorks.ToString();
        }

        public string RunMoveX(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
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
            Boats.Boat boat = boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boats.Boat.eStep.Done) return "Boat not Done";
            if (m_picker.p_infoStrip != null) return "Picker has InfoStrip";
            try
            {
                boat.p_infoStrip.m_eVision = eVision;
                boat.p_infoStrip.m_eWorks = eWorks; 
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(eVision, eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eWorks))) return p_sInfo;
                boat.RunVacuum(false);
                boat.RunBlow(true); 
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                boat.RunBlow(false);
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (m_picker.IsVacuum() == false) return "Pick Strip Error";
                m_picker.p_infoStrip = boat.p_infoStrip;
                boat.p_infoStrip = null;
                boat.p_eStep = Boats.Boat.eStep.RunReady; 
            }
            finally
            {
                boat.RunBlow(false);
                RunMoveZ(ePos.Ready);
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnload(Vision.eWorks eVisionWorks)
        {
            Boats boats = m_handler.m_aBoats[Vision.eVision.Top2D];
            Boats.Boat boat = boats.m_aBoat[eVisionWorks];
            if (boat.p_eStep != Boats.Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                boat.RunVacuum(true);
                m_picker.RunVacuum(false);
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
            }
            finally
            {
                RunMoveZ(ePos.Ready);
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
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(ePos.Turnover))) return p_sInfo;
                if (Run(RunMoveZ(ePos.Turnover))) return p_sInfo;
                loader2.RunVacuum(true);
                m_picker.RunVacuum(false);
                loader2.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(Vision.eVision.Top2D, Vision.eWorks.B))) return p_sInfo;
            }
            finally
            {
                RunMoveZ(ePos.Ready);
                RunMoveX(Vision.eVision.Top2D, Vision.eWorks.B);
            }
            return "OK";
        }
        #endregion

        #region Run Loader1
        public string StartLoader1()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            StartRun(m_runLoader1);
            return "OK";
        }

        public string RunLoader1()
        {
            Boats boats3D = m_handler.m_aBoats[Vision.eVision.Top3D];
            Boats boats2D = m_handler.m_aBoats[Vision.eVision.Top2D];
            while ((EQ.p_eState == EQ.eState.Run) && (EQ.IsStop() == false))
            {
                Thread.Sleep(10);
                if (m_picker.p_infoStrip != null)
                {
                    Vision.eWorks eWorks = m_picker.p_infoStrip.m_eWorks; 
                    switch (m_picker.p_infoStrip.m_eVision)
                    {
                        case Vision.eVision.Top3D:
                            if (boats2D.m_aBoat[eWorks].p_eStep == Boats.Boat.eStep.Ready)
                            {
                                if (Run(RunUnload(eWorks))) return p_sInfo;
                            }
                            break;
                        case Vision.eVision.Top2D:
                            if (m_handler.m_loader2.p_eState == eState.Ready)
                            {
                                if (Run(RunUnloadTurnover())) return p_sInfo;
                            }
                            break; 
                    }
                }
                else
                {
                    foreach (Vision.eWorks eWorks in Enum.GetValues(typeof(Vision.eWorks)))
                    {
                        Boats.Boat boat = boats2D.m_aBoat[eWorks]; 
                        switch (boat.p_eStep)
                        {
                            case Boats.Boat.eStep.Done:
                                if (Run(RunLoad(eLoad.Top2D, eWorks))) return p_sInfo;
                                break;
                            case Boats.Boat.eStep.Ready:
                                if (boats3D.m_aBoat[eWorks].p_eStep == Boats.Boat.eStep.Done)
                                {
                                    if (Run(RunLoad(eLoad.Top3D, eWorks))) return p_sInfo; 
                                }
                                break; 
                        }
                    }
                }
            }
            return "OK";
        }
        #endregion

        #region override
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
        ModuleRunBase m_runLoader1;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Load(this), false, "Load Strip from Boat");
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip to Boat");
            AddModuleRunList(new Run_UnloadTurnover(this), false, "Unload Strip to Turnover");
            m_runLoader1 = AddModuleRunList(new Run_Loader1(this), false, "Run Loader1");
        }

        public class Run_Load : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Load(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            eLoad m_eLoad = eLoad.Top3D; 
            Vision.eWorks m_ePos = Vision.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                run.m_eLoad = m_eLoad;
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eLoad = (eLoad)tree.Set(m_eLoad, m_eLoad, "Vision", "Select Vision", bVisible); 
                m_ePos = (Vision.eWorks)tree.Set(m_ePos, m_ePos, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunLoad(m_eLoad, m_ePos);
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

            Vision.eWorks m_ePos = Vision.eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (Vision.eWorks)tree.Set(m_ePos, m_ePos, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunUnload(m_ePos);
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

        public class Run_Loader1 : ModuleRunBase
        {
            Loader1 m_module;
            public Run_Loader1(Loader1 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Loader1 run = new Run_Loader1(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoader1();
            }
        }
        #endregion
    }
}
