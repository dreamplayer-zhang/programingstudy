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
            m_aBoat[Vision2D.eWorks.A].GetTools(m_toolBox, this, bInit);
            m_aBoat[Vision2D.eWorks.B].GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition(); 
        }
        #endregion

        #region Camera Axis
        AxisXY m_axisCam;
        const string c_sPosReady = "Ready";
        void InitPosition()
        {
            m_axisCam.AddPos(c_sPosReady);
            m_axisCam.AddPos(Enum.GetNames(typeof(Vision2D.eWorks))); 
        }

        public string RunMoveCamera(Vision2D.eWorks ePos, bool bWait = true)
        {
            m_axisCam.StartMove(ePos);
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveSnapStart(Vision2D.eWorks eWorks, Vision2D.Recipe.Snap snapData, bool bWait = true)
        {
            m_axisCam.StartMove(eWorks, new RPoint(m_xCamScale * snapData.m_dpAxis.X, 0));
            if (Run(m_aBoat[eWorks].RunMoveSnapStart(snapData, bWait))) return p_sInfo;
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
                    case Vision2D.eVision.Top3D: return Boat.ePos.Handler;
                    case Vision2D.eVision.Top2D: return m_pine2.p_b3D ? Boat.ePos.Vision : Boat.ePos.Handler;
                    case Vision2D.eVision.Bottom: return Boat.ePos.Vision; 
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
                    case Vision2D.eVision.Top3D: return Boat.ePos.Vision;
                    case Vision2D.eVision.Top2D: return Boat.ePos.Vision;
                    case Vision2D.eVision.Bottom: return Boat.ePos.Handler;
                }
                return Boat.ePos.Vision;
            } 
            set { }
        }
        #endregion

        #region Boat
        public Dictionary<Vision2D.eWorks, Boat> m_aBoat = new Dictionary<Vision2D.eWorks, Boat>(); 
        void InitBoat()
        {
            m_aBoat.Add(Vision2D.eWorks.A, new Boat(p_id + ".A", this));
            m_aBoat.Add(Vision2D.eWorks.B, new Boat(p_id + ".B", this));
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
                m_aBoat[Vision2D.eWorks.A].p_eStep = Boat.eStep.RunReady;
                m_aBoat[Vision2D.eWorks.B].p_eStep = Boat.eStep.RunReady;
            }
            else
            {
                p_eState = eState.Error;
                m_aBoat[Vision2D.eWorks.A].p_eStep = Boat.eStep.Init;
                m_aBoat[Vision2D.eWorks.B].p_eStep = Boat.eStep.Init;
            }
            return p_sInfo;
        }

        public override void Reset()
        {
            m_axisCam.StartMove(c_sPosReady); 
            m_aBoat[Vision2D.eWorks.A].Reset(p_eState);
            m_aBoat[Vision2D.eWorks.B].Reset(p_eState);
            base.Reset();
        }
        #endregion

        #region Snap
        public string StartSnap(Vision2D.Recipe snapData)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_recipe = snapData;
            return StartRun(run);
        }

        public string RunSnap(Vision2D.Recipe snapData)
        {
            StopWatch sw = new StopWatch();
            try
            {
                int iSnap = 0; 
                foreach (Vision2D.Recipe.Snap snap in snapData.m_aSnap)
                {
                    m_vision.RunLight(snap.m_lightPower);
                    if (Run(RunMoveSnapStart(snapData.m_eWorks, snap))) return p_sInfo;
                    m_vision.StartSnap(snap, snapData.m_eWorks, p_sRecipe, iSnap);
                    if (Run(m_aBoat[snapData.m_eWorks].RunSnap())) return p_sInfo;
                    if (m_vision.IsBusy()) EQ.p_bStop = true;
                    iSnap++; 
                }
            }
            catch (Exception e) { p_sInfo = e.Message; }
            finally
            {
                m_axisCam.StartMove((Vision2D.eWorks)(1 - (int)snapData.m_eWorks));
                m_aBoat[snapData.m_eWorks].RunMove(p_ePosUnload);
            }

            m_log.Info("Run Snap End : " + (sw.ElapsedMilliseconds / 1000.0).ToString("0.00") + " sec");
            return "OK";
        }
        #endregion

        #region Recipe
        string _sRecipe = ""; 
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                m_vision.SetRecipe(value); 
            }
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
        public Vision2D m_vision; 
        public Boats(Vision2D vision, IEngineer engineer, Pine2 pine2)
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
                m_recipe = new Vision2D.Recipe(module.m_vision); 
                InitModuleRun(module);
            }

            public Vision2D.Recipe m_recipe; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_recipe = m_recipe.Clone();
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_recipe.RunTree(tree, bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_recipe); 
            }
        }
        #endregion
    }
}
