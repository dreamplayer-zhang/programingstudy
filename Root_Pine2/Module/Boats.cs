using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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
            m_toolBox.GetComm(ref m_tcpRequest, this, p_id + ".Request");
            if (bInit)
            {
                InitPosition();
                m_tcpRequest.EventReciveData += M_tcpRequest_EventReciveData;
            }
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
            m_aBoat.Add(Vision2D.eWorks.A, new Boat(p_id + ".A", this, Vision2D.eWorks.A));
            m_aBoat.Add(Vision2D.eWorks.B, new Boat(p_id + ".B", this, Vision2D.eWorks.B));
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

        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK"; 
            if (IsReady(m_aBoat[Vision2D.eWorks.A])) return StartSnap(Vision2D.eWorks.A);
            if (IsReady(m_aBoat[Vision2D.eWorks.B])) return StartSnap(Vision2D.eWorks.B);
            return "OK"; 
        }

        bool IsReady(Boat boat)
        {
            if (boat.p_eStep != Boat.eStep.Ready) return false;
            return (boat.p_infoStrip != null); 
        }
        #endregion

        #region Snap
        public string StartSnap(Vision2D.eWorks eWorks)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_eWorks = eWorks;
            return StartRun(run);
        }

        public string RunSnap(Vision2D.eWorks eWorks)
        {
            StopWatch sw = new StopWatch();
            try
            {
                m_aBoat[eWorks].p_eStep = Boat.eStep.Run; 
                int iSnap = 0; 
                foreach (Vision2D.Recipe.Snap snap in m_aBoat[eWorks].m_recipe.m_aSnap)
                {
                    //m_vision.RunLight(snap.m_lightPower);
                    //if (Run(RunMoveSnapStart(eWorks, snap))) return p_sInfo;
                    m_vision.StartSnap(snap, eWorks, p_sRecipe, iSnap);
                    //if (Run(m_aBoat[eWorks].RunSnap())) return p_sInfo;
                    if (m_vision.IsBusy()) EQ.p_bStop = true;
                    iSnap++; 
                }
            }
            catch (Exception e) { p_sInfo = e.Message; }
            finally
            {
                m_axisCam.StartMove((Vision2D.eWorks)(1 - (int)eWorks));
                m_aBoat[eWorks].RunMove(p_ePosUnload);
                m_aBoat[eWorks].p_eStep = Boat.eStep.Run;
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
                m_aBoat[Vision2D.eWorks.A].p_sRecipe = value;
                m_aBoat[Vision2D.eWorks.B].p_sRecipe = value;
                m_vision.SetRecipe(value); 
            }
        }
        #endregion

        #region Request
        TCPAsyncServer m_tcpRequest;

        private void M_tcpRequest_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
            if (sRead.Length <= 0) return;
            ReadRequest(sRead); 
            m_tcpRequest.Send(sRead); 
        }

        void ReadRequest(string sRead)
        {
            string[] asRead = sRead.Split(',');
            if (asRead.Length < 2) return;
            if (asRead[1] == Works2D.eProtocol.Snap.ToString())
            {
                if (asRead.Length < 3) return;
                string sRecipe = asRead[2];
                Vision2D.eWorks eWorks = (asRead[3] == "A") ? Vision2D.eWorks.A : Vision2D.eWorks.B;
                //m_vision.m_recipe[eWorks].RecipeOpen(sRecipe);
                m_aBoat[eWorks].m_recipe.RecipeOpen(sRecipe); 
                StartSnap(eWorks); 
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
                InitModuleRun(module);
            }

            public Vision2D.eWorks m_eWorks; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_eWorks = m_eWorks;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "Select Boat", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_eWorks); 
            }
        }
        #endregion
    }
}
