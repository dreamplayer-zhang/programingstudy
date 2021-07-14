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
using System.Threading;

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

        public string RunMoveSnapStart(Vision2D.eWorks eWorks, Vision2D.Recipe.Snap snapData, int xLine, bool bWait = true)
        {
            double xp = m_xCamScale * snapData.m_dpAxis.X + ((m_aCamOffset != null) ? 10 * m_aCamOffset[xLine].X : 0);
            double yp = m_pine2.m_thicknessDefault - m_pine2.p_thickness; 
            m_axisCam.StartMove(eWorks, new RPoint(xp, yp));
            int yOffset = (m_aCamOffset != null) ? (int)(10 * m_aCamOffset[xLine].Y) : 0;
            if (Run(m_aBoat[eWorks].RunMoveSnapStart(snapData, yOffset, bWait))) return p_sInfo;
            return bWait ? m_axisCam.p_axisX.WaitReady() : "OK";
        }

        double m_xCamScale = 10000; 
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

        public string RunMoveReady(Vision2D.eWorks eWorks)
        {
            if (Run(m_aBoat[eWorks].RunMove(p_ePosLoad))) return p_sInfo;
            m_aBoat[eWorks].p_eStep = Boat.eStep.Ready;
            return "OK";
        }

        public string StartBoatDone(Vision2D.eWorks eWorks)
        {
            Run_RunBoat run = (Run_RunBoat)m_runBoat.Clone();
            run.m_eWorks = eWorks;
            run.m_eRun = Run_RunBoat.eRun.Done; 
            return StartRun(run);
        }

        public string RunMoveDone(Vision2D.eWorks eWorks)
        {
            m_aBoat[eWorks].p_eStep = Boat.eStep.Run;
            if (Run(m_aBoat[eWorks].RunMove(p_ePosUnload))) return p_sInfo;
            m_aBoat[eWorks].p_eStep = Boat.eStep.Done;
            if (m_aBoat[eWorks].p_infoStrip != null) m_aBoat[eWorks].p_infoStrip.SetResult(m_vision.m_eVision, InfoStrip.eResult.POS.ToString(), "1", "1", "4"); 
            return "OK";
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
                m_aBoat[Vision2D.eWorks.A].m_doTriggerSwitch.Write(false);
                m_aBoat[Vision2D.eWorks.B].m_doTriggerSwitch.Write(false);
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
            bool bSnap = m_vision.m_remote.p_bEnable; 
            Vision2D.eWorks eWorks = Vision2D.eWorks.A; 
            if (m_aBoat[eWorks].IsEnableRun()) return bSnap ? StartSnap(eWorks, false) : StartBoatDone(eWorks);
            eWorks = Vision2D.eWorks.B;
            if (m_aBoat[eWorks].IsEnableRun()) return bSnap ? StartSnap(eWorks, false) : StartBoatDone(eWorks);
            return "OK"; 
        }
        #endregion

        #region Snap
        public string StartSnap(Vision2D.eWorks eWorks, bool bReadRecipe)
        {
            Run_Snap run = (Run_Snap)m_runSnap.Clone();
            run.m_eWorks = eWorks;
            run.m_bReadRecipe = bReadRecipe; 
            return StartRun(run);
        }

        RPoint[] m_aCamOffset = null; 
        bool m_bSnapReady = false; 
        public string RunSnap(Vision2D.eWorks eWorks, bool bReadRecipe)
        {
            StopWatch sw = new StopWatch();
            try
            {
                while (m_aBoat[eWorks].p_inspectStrip != null)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
                m_aBoat[eWorks].p_inspectStrip = m_aBoat[eWorks].p_infoStrip;
                if(m_aBoat[eWorks].p_inspectStrip != null)  // Manual Snap 시
                    m_aBoat[eWorks].p_inspectStrip.StartInspect(m_vision.m_eVision); 
                if (bReadRecipe)
                {
                    string sRecipe = m_aBoat[eWorks].p_sRecipe;
                    m_aBoat[eWorks]._sRecipe = "";
                    m_aBoat[eWorks].p_sRecipe = sRecipe; 
                }
                m_vision.SendSnapInfo(m_aBoat[eWorks].GetSnapInfo());
                m_aBoat[eWorks].p_eStep = Boat.eStep.Run;
                m_aBoat[eWorks].m_doTriggerSwitch.Write(true);
                int xLine = m_aBoat[eWorks].m_recipe.m_aSnap.Count;
                if (m_aBoat[eWorks].m_recipe.p_eSnapMode == Vision2D.Recipe.eSnapMode.ALL) xLine /= 2;
                switch (xLine)
                {
                    case 2: m_aCamOffset = m_aBoat[eWorks].m_aCam2Offset; break;
                    case 3: m_aCamOffset = m_aBoat[eWorks].m_aCam3Offset; break;
                    default: m_aCamOffset = null; break; 
                }
                int iSnap = 0;
                for(int i = 0; i < m_aBoat[eWorks].m_recipe.m_aSnap.Count; i++)
                {
                    Vision2D.Recipe.Snap snap = m_aBoat[eWorks].m_recipe.m_aSnap[i];
                    m_vision.RunLight(snap.m_lightPower);
                    m_bSnapReady = false;
                    m_vision.StartSnap(snap, eWorks, iSnap);
                    if (Run(RunMoveSnapStart(eWorks, snap, i % xLine))) return p_sInfo;
                    while (m_bSnapReady == false)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return "EQ Stop";
                    }

                    if (Run(m_aBoat[eWorks].RunSnap())) return p_sInfo;
                    if (i < m_aBoat[eWorks].m_recipe.m_aSnap.Count-1)
                    {
                        SendChangeUserSet();
                        if (Run(RunMoveSnapStart(eWorks, m_aBoat[eWorks].m_recipe.m_aSnap[i + 1], i % xLine))) return p_sInfo;
                    }
                    if (m_vision.IsBusy()) EQ.p_bStop = true;
                    iSnap++;

                }
                /*
                foreach (Vision2D.Recipe.Snap snap in m_aBoat[eWorks].m_recipe.m_aSnap)
                {
                    m_vision.RunLight(snap.m_lightPower);
                    m_bSnapReady = false; 
                    m_vision.StartSnap(snap, eWorks, iSnap);
                    if (Run(RunMoveSnapStart(eWorks, snap, iSnap % xLine))) return p_sInfo;
                    while (m_bSnapReady == false)
                    {
                        Thread.Sleep(10);
                        if (EQ.IsStop()) return "EQ Stop"; 
                    }
                   
                    if (Run(m_aBoat[eWorks].RunSnap())) return p_sInfo;
                  
                    if (m_vision.IsBusy()) EQ.p_bStop = true;
                    iSnap++; 
                }*/
                m_vision.RunLightOff();
                m_aBoat[eWorks].p_eStep = Boat.eStep.Done;
            }
            catch (Exception e) { p_sInfo = e.Message; }
            finally
            {
                m_axisCam.StartMove((Vision2D.eWorks)(1 - (int)eWorks));
                m_aBoat[eWorks].RunMove(p_ePosUnload);
                m_aBoat[eWorks].m_doTriggerSwitch.Write(false);
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
                if (!m_vision.m_remote.p_bEnable) return;
                _sRecipe = value;
                m_aBoat[Vision2D.eWorks.A].p_sRecipe = value;
                m_aBoat[Vision2D.eWorks.B].p_sRecipe = value;
                string sRun = m_vision.SendRecipe(value); 
                if (sRun != "OK")
                {
                    p_sInfo = sRun;
                    p_eState = eState.Error;
                }
            }
        }
        #endregion

        #region Request
        TCPIPServer m_tcpRequest;
        private void M_tcpRequest_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sRead = Encoding.Default.GetString(aBuf, 0, nSize);
            if (sRead.Length <= 0) return;
            ReadRequest(sRead); 
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
                m_aBoat[eWorks].p_sRecipe = ""; 
                m_aBoat[eWorks].p_sRecipe = sRecipe; 
                StartSnap(eWorks, true);
                m_tcpRequest.Send(sRead);
            }
            if (asRead[1] == Works2D.eProtocol.SnapReady.ToString())
            {
                m_bSnapReady = true;
                m_tcpRequest.Send(sRead);
            }
            if (asRead[1] == Works2D.eProtocol.InspDone.ToString())
            {
                string sStripID = asRead[2];
                string sStripResult = asRead[3];
                string sX = asRead[4];
                string sY = asRead[5];
                string sMapResult = asRead[6];
                string sWork = asRead[7];
                foreach (Vision2D.eWorks eWorks in Enum.GetValues(typeof(Vision2D.eWorks)))
                {
                    if (sWork == eWorks.ToString()) m_aBoat[eWorks].InspectDone(m_vision.m_eVision, sStripID, sStripResult, sX, sY, sMapResult);
                }
                m_tcpRequest.Send(sRead);
            }
            if (asRead[1] == Works2D.eProtocol.WorksConnect.ToString())
            {
                Vision2D.eWorks eWorks = (asRead[2] == "A") ? Vision2D.eWorks.A : Vision2D.eWorks.B;
                bool bConnect = (asRead[3] == "1");
                m_aBoat[eWorks].p_bWorksConnect = bConnect; 
            }
        }

        int m_nReq = 0;
        public string SendChangeUserSet()
        {
            string sSend = m_nReq.ToString("000") + "," + Works2D.eProtocol.ChangeUserset.ToString() ;
            m_tcpRequest.Send(sSend);
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeCamAxis(tree.GetTree("Camera Axis"));
            m_aBoat[Vision2D.eWorks.A].RunTreeCamOffset(tree.GetTree("Camera X Offset A"));
            m_aBoat[Vision2D.eWorks.B].RunTreeCamOffset(tree.GetTree("Camera X Offset B"));
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

        protected override void RunThreadStop()
        {
            m_aBoat[Vision2D.eWorks.A].RunMove(Boat.ePos.Handler, false);
            m_aBoat[Vision2D.eWorks.B].RunMove(Boat.ePos.Handler);
            m_aBoat[Vision2D.eWorks.A].RunMove(Boat.ePos.Handler);
            base.RunThreadStop();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runSnap;
        ModuleRunBase m_runBoat;
        protected override void InitModuleRuns()
        {
            m_runSnap = AddModuleRunList(new Run_Snap(this), false, "Run Snap");
            AddModuleRunList(new Run_MoveBoat(this), false, "Move Boat");
            m_runBoat = AddModuleRunList(new Run_RunBoat(this), false, "Run Boat");
        }

        public class Run_Snap : ModuleRunBase
        {
            Boats m_module;
            public Run_Snap(Boats module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public bool m_bReadRecipe = true; 
            public Vision2D.eWorks m_eWorks; 
            public override ModuleRunBase Clone()
            {
                Run_Snap run = new Run_Snap(m_module);
                run.m_eWorks = m_eWorks;
                run.m_bReadRecipe = m_bReadRecipe; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "Select Boat", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunSnap(m_eWorks, m_bReadRecipe); 
            }
        }

        public class Run_MoveBoat : ModuleRunBase
        {
            Boats m_module;
            public Run_MoveBoat(Boats module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public Boat.ePos m_ePos = Boat.ePos.Handler; 
            public Vision2D.eWorks m_eWorks;
            public override ModuleRunBase Clone()
            {
                Run_MoveBoat run = new Run_MoveBoat(m_module);
                run.m_eWorks = m_eWorks;
                run.m_ePos = m_ePos; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "Select Boat", bVisible);
                m_ePos = (Boat.ePos)tree.Set(m_ePos, m_ePos, "Position", "Boat Position", bVisible); 
            }

            public override string Run()
            {
                return m_module.m_aBoat[m_eWorks].RunMove(m_ePos);
            }
        }

        public class Run_RunBoat : ModuleRunBase
        {
            Boats m_module;
            public Run_RunBoat(Boats module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public enum eRun
            {
                Ready,
                Done,
            }
            public eRun m_eRun = eRun.Ready; 
            public Vision2D.eWorks m_eWorks;
            public override ModuleRunBase Clone()
            {
                Run_RunBoat run = new Run_RunBoat(m_module);
                run.m_eWorks = m_eWorks;
                run.m_eRun = m_eRun;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (Vision2D.eWorks)tree.Set(m_eWorks, m_eWorks, "eWorks", "Select Boat", bVisible);
                m_eRun = (eRun)tree.Set(m_eRun, m_eRun, "Run", "Boat Run", bVisible);
            }

            public override string Run()
            {
                switch (m_eRun)
                {
                    case eRun.Ready: return m_module.RunMoveReady(m_eWorks);
                    case eRun.Done: return m_module.RunMoveDone(m_eWorks); 
                }
                return "OK";
            }
        }
        #endregion
    }
}
