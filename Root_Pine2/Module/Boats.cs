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

        public string RunMoveCamera(string sPos, bool bWait = true)
        {
            m_axisCam.StartMove(sPos);
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveCamera(Vision.eWorks ePos, bool bWait = true)
        {
            m_axisCam.StartMove(ePos);
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
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

        #region ScanData
        public class ScanData
        {
            public RPoint m_dpAxis = new RPoint();
            public Vision.ScanData m_scanData = new Vision.ScanData();

            public void RunTree(Tree tree, bool bVisible)
            {
                m_dpAxis = tree.Set(m_dpAxis, m_dpAxis, "Axis Offset", "Axis Offset (pulse)");
                m_scanData.RunTree(tree, bVisible); 
            }

            Vision m_vision; 
            public ScanData(Vision vision)
            {
                m_vision = vision; 
            }
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

        #region Scan
        public string StartScan(Vision.eWorks eWorks)
        {
            Run_Scan run = (Run_Scan)m_runScan.Clone();
            run.m_eWorks = eWorks;
            return StartRun(run); 
        }

        public string RunScan(Vision.eWorks eWorks)
        {
            try
            {
                if (Run(RunMoveCamera(eWorks))) return p_sInfo;
                if (Run(m_aBoat[eWorks].RunScan())) return p_sInfo;
            }
            finally
            {
                m_axisCam.StartMove((Vision.eWorks)(1 - (int)eWorks));
                m_aBoat[eWorks].RunMove(p_ePosUnload); 
            }
            return "OK";
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runScan;
        protected override void InitModuleRuns()
        {
            m_runScan = AddModuleRunList(new Run_Scan(this), false, "Run Scan");
        }

        public class Run_Scan : ModuleRunBase
        {
            Boats m_module;
            public Run_Scan(Boats module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Vision.eWorks m_eWorks = Vision.eWorks.A; 
            public override ModuleRunBase Clone()
            {
                Run_Scan run = new Run_Scan(m_module);
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision.eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible);
            }

            public override string Run()
            {
                return m_module.RunScan(m_eWorks); 
            }
        }
        #endregion
    }
}
