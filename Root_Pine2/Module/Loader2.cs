using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Loader2 : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXZ;
        DIO_I2O m_dioTurnUp;
        DIO_O m_doVacuum;
        public DIO_I m_diCrash;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXZ, this, "Loader2");
            m_toolBox.GetDIO(ref m_dioTurnUp, this, "Turn", "Down", "Up");
            m_toolBox.GetDIO(ref m_doVacuum, this, "Vacuum");
            m_toolBox.GetDIO(ref m_diCrash, this, "Crash");
            if (bInit)
            {
                InitPosition();
                InitALID();
            }
        }

        ALID m_alidCrash;
        void InitALID()
        {
            m_alidCrash = m_gaf.GetALID(this, "Crash", "Crash with Loader1");
        }

        const string c_sReady = "Ready";
        void InitPosition()
        {
            m_axisXZ.AddPos(c_sReady);
            m_axisXZ.AddPos(GetPosString(eWorks.A));
            m_axisXZ.AddPos(GetPosString(eWorks.B));
        }

        string GetPosString(eWorks eVisionWorks)
        {
            return eVision.Bottom.ToString() + eVisionWorks.ToString(); 
        }

        public string RunMoveX(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(double fPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(fPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        double p_dZ
        {
            get { return 0;/* (m_pine2.m_thicknessDefault - m_pine2.p_thickness) * 10;*/ }
        }

        public string RunMoveZ(eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(ePos), p_dZ);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMove(string sPos, bool bWait = true)
        {
            m_axisXZ.StartMove(sPos);
            return bWait ? m_axisXZ.WaitReady() : "OK"; 
        }

        public string RunTurnUp(bool bUp)
        {
            if (m_axisXZ.p_axisX.p_posCommand == m_axisXZ.p_axisX.GetPosValue(c_sReady))
            {
                m_dioTurnUp.Write(bUp);
                return m_dioTurnUp.WaitDone();
            }
            double zPos = m_axisXZ.p_axisY.p_posCommand;
            RunMoveZ((double)0);
            m_dioTurnUp.Write(bUp);
            if (Run(m_dioTurnUp.WaitDone())) return p_sInfo;
            RunMoveZ(zPos);
            return "OK";
        }

        public string RunVacuum(bool bOn)
        {
            m_doVacuum.Write(bOn);
            return "OK";
        }

        double m_secVacuum = 0.3;
        void RunTreeVacuum(Tree tree)
        {
            m_secVacuum = tree.Set(m_secVacuum, m_secVacuum, "Delay", "Vacuum On & Off Delay (sec)"); 
        }
        #endregion

        #region Crash
        Loader1 p_loader1 { get { return m_handler.m_loader1; } }
        bool m_bThreadCrash = false;
        Thread m_threadCrash;
        void InitThreadCrash()
        {
            m_threadCrash = new Thread(new ThreadStart(RunThreadCrash));
            m_threadCrash.Start();
        }

        void RunThreadCrash()
        {
            m_bThreadCrash = true;
            Thread.Sleep(5000);
            while (m_bThreadCrash)
            {
                Thread.Sleep(10);
                if (m_diCrash.p_bIn)
                {
                    m_axisXZ.p_axisX.ServoOn(false);
                    p_loader1.m_axisXZ.p_axisX.ServoOn(false);
                }
                m_alidCrash.p_bSet = m_diCrash.p_bIn; 
            }
        }
        #endregion

        #region Run
        public string StartUnloadStrip()
        {
            return "OK";
        }

        public string RunUnload(eWorks eWorks)
        {
            Boat boat = m_boats.m_aBoat[eWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                m_doVacuum.Write(true);
                if (Run(RunTurnUp(false))) return p_sInfo;
                if (Run(m_boats.RunMoveReady(eWorks))) return p_sInfo;
                if (Run(RunMoveX(eWorks))) return p_sInfo;
                if (Run(RunMoveZ(eWorks))) return p_sInfo;
                m_doVacuum.Write(false);
                boat.RunVacuum(true);
                Thread.Sleep((int)(1000 * m_secVacuum));
                if (Run(RunMoveZ(c_sReady))) return p_sInfo;
                boat.StartClean();
                boat.p_infoStrip = p_infoStrip;
                p_infoStrip = null;
                if (Run(RunMoveX(c_sReady))) return p_sInfo;
                if (Run(RunTurnUp(true))) return p_sInfo;
            }
            finally
            {
                m_axisXZ.p_axisY.m_bCheckStop = false;
                RunMove(c_sReady);
                RunTurnUp(true);
                m_axisXZ.p_axisY.m_bCheckStop = true;
            }
            return "OK";
        }

        public string RunMoveReady()
        {
            RunTurnUp(true);
            RunMove(c_sReady);
            return "OK";
        }
        #endregion

        #region override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (p_infoStrip == null) return "OK";
            if ((m_boats.m_aBoat[p_infoStrip.m_eWorks].p_eStep != Boat.eStep.Ready) || (m_boats.m_aBoat[p_infoStrip.m_eWorks].p_infoStrip != null)) return "OK"; 
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            run.m_eWorks = p_infoStrip.m_eWorks; 
            return StartRun(run);
        }

        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            Thread.Sleep(1000); 
            p_sInfo = base.StateHome(m_axisXZ.p_axisY);
            if (p_sInfo != "OK") return p_sInfo;
            p_sInfo = base.StateHome(m_axisXZ.p_axisX);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            if (p_sInfo == "OK") RunMoveReady();
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
            p_infoStrip = null;
            RunMove(c_sReady);
            RunTurnUp(true);
            RunVacuum(false); 
        }
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeVacuum(tree.GetTree("Vacuum")); 
        }
        #endregion

        public InfoStrip p_infoStrip { get; set; }
        Pine2 m_pine2 = null;
        Pine2_Handler m_handler; 
        Boats m_boats; 
        public Loader2(string id, IEngineer engineer, Pine2_Handler handler)
        {
            p_infoStrip = null;
            m_handler = handler; 
            m_pine2 = handler.m_pine2;
            m_boats = handler.m_aBoats[eVision.Bottom];
            base.InitBase(id, engineer);
            InitThreadCrash();
        }

        public override void ThreadStop()
        {
            if (m_bThreadCrash)
            {
                m_bThreadCrash = false;
                m_threadCrash.Join();
            }
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runUnload; 
        protected override void InitModuleRuns()
        {
            m_runUnload = AddModuleRunList(new Run_Unload(this), true, "Unload Strip to Boat");
            AddModuleRunList(new Run_MoveReady(this), true, "Loader2 Move to Ready Position");
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader2 m_module;
            public Run_Unload(Loader2 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eWorks m_eWorks = eWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_eWorks = m_eWorks; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eWorks = (eWorks)tree.Set(m_eWorks, m_eWorks, "Boat", "Select Boat", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunUnload(m_eWorks);
            }
        }

        public class Run_MoveReady : ModuleRunBase
        {
            Loader2 m_module;
            public Run_MoveReady(Loader2 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MoveReady run = new Run_MoveReady(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunMoveReady();
            }
        }
        #endregion
    }
}
