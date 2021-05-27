using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
        public enum ePos
        {
            Handler,
            Vision
        }
        public ePos p_ePosLoad 
        {
            get
            {
                switch (m_vision.m_eVision)
                {
                    case Vision.eVision.Top3D: return ePos.Handler;
                    case Vision.eVision.Top2D: return m_pine2.p_b3D ? ePos.Vision : ePos.Handler;
                    case Vision.eVision.Bottom: return ePos.Vision; 
                }
                return ePos.Handler; 
            }
            set { }
        }
        public ePos p_ePosUnload 
        {
            get
            {
                switch(m_vision.m_eVision)
                {
                    case Vision.eVision.Top3D: return ePos.Vision;
                    case Vision.eVision.Top2D: return ePos.Vision;
                    case Vision.eVision.Bottom: return ePos.Handler;
                }
                return ePos.Vision;
            } 
            set { }
        }
        #endregion

        #region Boat
        public class Boat : NotifyProperty
        {
            #region Step
            public enum eStep
            {
                Init,
                Ready,
                Run,
                Done,
                RunReady,
            }
            eStep _eStep = eStep.Init; 
            public eStep p_eStep 
            { 
                get { return _eStep; }
                set
                {
                    if (_eStep == value) return;
                    _eStep = value;
                    OnPropertyChanged();
                    if (value == eStep.RunReady) m_bgwRunReady.RunWorkerAsync();
                }
            }

            BackgroundWorker m_bgwRunReady = new BackgroundWorker(); 
            private void M_bgwRunReady_DoWork(object sender, DoWorkEventArgs e)
            {
                m_axis.StartMove(m_boats.p_ePosLoad);
                p_eStep = (m_axis.WaitReady() == "OK") ? eStep.Ready : eStep.Init; 
            }
            #endregion

            #region ToolBox
            Axis m_axis; 
            DIO_O m_doVacuumPump; 
            public DIO_Os m_doVacuum;
            public DIO_O m_doBlow;
            DIO_I2O m_dioRollerDown;
            DIO_O m_doRollerPusher;
            DIO_O m_doCleanerBlow;
            DIO_O m_doCleanerSuction; 
            public void GetTools(ToolBox toolBox, Boats boats, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, boats, p_id + ".Scan"); 
                toolBox.GetDIO(ref m_doVacuumPump, boats, p_id + ".Vacuum Pump");
                toolBox.GetDIO(ref m_doVacuum, boats, p_id + ".Vacuum", new string[4] { "1", "2", "3", "4" });
                toolBox.GetDIO(ref m_doBlow, boats, p_id + ".Blow");
                toolBox.GetDIO(ref m_dioRollerDown, boats, p_id + ".Roller", "Up", "Down");
                toolBox.GetDIO(ref m_doRollerPusher, boats, p_id + ".Roller Pusher");
                toolBox.GetDIO(ref m_doCleanerBlow, boats, p_id + ".Cleaner Blow");
                toolBox.GetDIO(ref m_doCleanerSuction, boats, p_id + ".Cleaner Suction");
                if (bInit) InitPosition();
            }
            #endregion

            #region Axis
            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
                m_axis.AddSpeed("Grab"); 
            }

            public string RunMove(ePos ePos, bool bWait = true)
            {
                m_axis.RunTrigger(false); 
                m_axis.StartMove(ePos);
                return bWait ? m_axis.WaitReady() : "OK"; 
            }

            public string MoveScan(double dPosAcc)
            {
                m_axis.RunTrigger(false);
                m_axis.StartMove(m_axis.m_trigger.m_aPos[0] + dPosAcc);
                return m_axis.WaitReady(); 
            }

            public string StartScan()
            {
                m_axis.RunTrigger(true); 
                return m_axis.StartMove(m_axis.m_trigger.m_aPos[1], "Grab"); 
            }
            #endregion

            #region Vacuum
            public void SetVacuum(bool[] aVacuum)
            {
                for (int n = 0; n < Math.Min(4, aVacuum.Length); n++) m_doVacuum.Write(n, aVacuum[n]); 
            }

            public void RunVacuum(bool bOn)
            {
                m_doVacuumPump.Write(bOn);
            }

            public void RunBlow(bool bBlow)
            {
                m_doBlow.Write(bBlow);
            }
            #endregion

            #region Clean Roller
            public string RunRoller(bool bDown)
            {
                m_doRollerPusher.Write(bDown);
                m_dioRollerDown.Write(bDown);
                return m_dioRollerDown.WaitDone(); 
            }
            #endregion

            #region Cleaner
            public void RunCleaner(bool bBlow, bool bSuction)
            {
                m_doCleanerBlow.Write(bBlow);
                m_doCleanerSuction.Write(bSuction); 
            }
            #endregion

            #region Scan
            public class ScanData
            {
            }
            public List<ScanData> m_aScanData = new List<ScanData>(); 
            public string RunScan()
            {
                //forget
                p_eStep = eStep.Done; 
                return "OK";  
            }
            #endregion

            public void Reset(eState eState)
            {
                p_infoStrip = null;
                if (eState == eState.Ready) p_eStep = eStep.RunReady; 
            }

            public InfoStrip p_infoStrip { get; set; }
            public string p_id { get; set; }
            Boats m_boats;
            Vision.VisionWorks m_visionWorks; 
            public Boat(string id, Boats boats, Vision.VisionWorks visionWorks)
            {
                m_bgwRunReady.DoWork += M_bgwRunReady_DoWork;
                p_id = id + visionWorks.m_eVisionWorks.ToString();
                m_boats = boats;
                m_visionWorks = visionWorks; 
            }
        }
        public Dictionary<Vision.eWorks, Boat> m_aBoat = new Dictionary<Vision.eWorks, Boat>(); 
        void InitBoat()
        {
            m_aBoat.Add(Vision.eWorks.A, new Boat(p_id, this, m_vision.m_aVisionWorks[Vision.eWorks.A]));
            m_aBoat.Add(Vision.eWorks.B, new Boat(p_id, this, m_vision.m_aVisionWorks[Vision.eWorks.B]));
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
                m_eWorks = (Vision.eWorks)tree.Set(m_eWorks, m_eWorks, "VisionWorks", "Select VisionWorks", bVisible);
            }

            public override string Run()
            {
                return m_module.RunScan(m_eWorks); 
            }
        }
        #endregion
    }
}
