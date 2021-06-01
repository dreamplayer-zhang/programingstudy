using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Pine2.Module
{
    public class Boats : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisCam, this, "Camera"); 
            m_aBoat[Vision.eWorks.A].GetTools(m_toolBox, this, bInit);
            m_aBoat[Vision.eWorks.B].GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition(); 
        }
        #endregion

        #region Camera Axis
        AxisXY m_axisCam;
        const string c_sPosReady = "Ready";
        void InitPosition()
        {
            m_axisCam.AddPos(c_sPosReady);
            m_axisCam.AddPos(Enum.GetNames(typeof(Vision.eWorks))); 
        }

        public string RunMoveCamera(Vision.eWorks ePos, bool bWait = true)
        {
            m_axisCam.StartMove(ePos);
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveSnapStart(Vision.SnapData snapData, bool bWait = true)
        {
            m_axisCam.StartMove(snapData.m_eWorks, new RPoint(m_xCamScale * snapData.m_dpAxis.X, 0));
            if (Run(m_aBoat[snapData.m_eWorks].RunMoveSnapStart(snapData, bWait))) return p_sInfo;
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
        }

        double m_xCamScale = 1000; 
        void RunTreeCamAxis(Tree tree)
        {
            m_xCamScale = tree.Set(m_xCamScale, m_xCamScale, "X Scale", "Camera X Axis Scale (pulse / mm)"); 
        }
        #endregion

        #region Boat AxisPos
        public Boat.ePos p_ePosLoad 
        {
            get
            {
                switch (m_vision.m_eVision)
                {
                    case Vision.eVision.Top3D: return Boat.ePos.Handler;
                    case Vision.eVision.Top2D: return m_pine2.p_b3D ? Boat.ePos.Vision : Boat.ePos.Handler;
                    case Vision.eVision.Bottom: return Boat.ePos.Vision; 
                }
                return Boat.ePos.Handler; 
            }
            set { }
        }
        public Boat.ePos p_ePosUnload 
        {
            get
            {
                switch(m_vision.m_eVision)
                {
                    case Vision.eVision.Top3D: return Boat.ePos.Vision;
                    case Vision.eVision.Top2D: return Boat.ePos.Vision;
                    case Vision.eVision.Bottom: return Boat.ePos.Handler;
                }
                return Boat.ePos.Vision;
            } 
            set { }
        }
        #endregion

        #region Boat
        public Dictionary<Vision.eWorks, Boat> m_aBoat = new Dictionary<Vision.eWorks, Boat>(); 
        void InitBoat()
        {
            m_aBoat.Add(Vision.eWorks.A, new Boat(p_id, this, m_vision.m_aWorks[Vision.eWorks.A]));
            m_aBoat.Add(Vision.eWorks.B, new Boat(p_id, this, m_vision.m_aWorks[Vision.eWorks.B]));
        }
        #endregion

        #region State Home & Reset
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            if (p_sInfo == "OK")
            {
                p_eState = eState.Ready;
                m_aBoat[Vision.eWorks.A].p_eStep = Boat.eStep.RunReady;
                m_aBoat[Vision.eWorks.B].p_eStep = Boat.eStep.RunReady;
            }
            else
            {
                p_eState = eState.Error;
                m_aBoat[Vision.eWorks.A].p_eStep = Boat.eStep.Init;
                m_aBoat[Vision.eWorks.B].p_eStep = Boat.eStep.Init;
            }
            return p_sInfo;
        }

        public override void Reset()
        {
            m_axisCam.StartMove(c_sPosReady); 
            m_aBoat[Vision.eWorks.A].Reset(p_eState);
            m_aBoat[Vision.eWorks.B].Reset(p_eState);
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCamAxis(tree.GetTree("Camera Axis"));
        }
        #endregion

        Pine2 m_pine2;
        Vision m_vision; 
        public Boats(Vision vision, IEngineer engineer, Pine2 pine2)
        {
            m_vision = vision;
            p_id = "Boats " + vision.m_eVision.ToString();
            m_pine2 = pine2;
            InitBoat(); 
            InitBase(p_id, engineer); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region Snap
        public string StartSnap(Vision.SnapData snapData)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_snapData = snapData;
            return StartRun(run); 
        }

        public string RunSnap(Vision.SnapData snapData)
        {
            StopWatch sw = new StopWatch(); 
            try
            {
                m_vision.RunLight(snapData.m_lightPower);
                if (Run(RunMoveSnapStart(snapData))) return p_sInfo;
                m_vision.StartSnap(snapData); 
                if (Run(m_aBoat[snapData.m_eWorks].RunSnap())) return p_sInfo;
                if (m_vision.IsBusy()) EQ.p_bStop = true; 
            }
            catch (Exception e) { p_sInfo = e.Message; }
            finally
            {
                m_axisCam.StartMove((Vision.eWorks)(1 - (int)snapData.m_eWorks));
                m_aBoat[snapData.m_eWorks].RunMove(p_ePosUnload); 
            }

            m_log.Info("Run Snap End : " + (sw.ElapsedMilliseconds / 1000.0).ToString("0.00") + " sec"); 
            return "OK";
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runSnap;
        protected override void InitModuleRuns()
        {
            m_runSnap = AddModuleRunList(new Run_Snap(this), false, "Run Snap");
        }

        public class Run_Snap : ModuleRunBase
        {
            Boats m_module;
            public Run_Snap(Boats module)
            {
                m_module = module;
                m_snapData = new Vision.SnapData(module.m_vision); 
                InitModuleRun(module);
            }

            public Vision.SnapData m_snapData; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_snapData = m_snapData.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_snapData.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_snapData); 
            }
        }
        #endregion
    }
}
