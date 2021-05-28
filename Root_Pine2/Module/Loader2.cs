using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Loader2 : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXZ;
        DIO_I2O m_dioTurn;
        DIO_O m_doVacuum; 
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXZ, this, "Loader2");
            m_toolBox.GetDIO(ref m_dioTurn, this, "Turn", "Up", "Down");
            m_toolBox.GetDIO(ref m_doVacuum, this, "Vacuum");
            if (bInit) InitPosition();
        }

        const string c_sReady = "Ready"; 
        void InitPosition()
        {
            m_axisXZ.AddPos(c_sReady);
            m_axisXZ.AddPos(GetPosString(Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eWorks.B));
        }

        string GetPosString(Vision.eWorks eVisionWorks)
        {
            return Vision.eVision.Bottom.ToString() + eVisionWorks.ToString(); 
        }

        public string RunMoveX(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(string sPos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(sPos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(ePos));
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunTurn(bool bTurn)
        {
            m_dioTurn.Write(bTurn); 
            return m_dioTurn.WaitDone();
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

        #region RunUnload
        public string RunUnload(Vision.eWorks eVisionWorks)
        {
            Boat boat = m_boats.m_aBoat[eVisionWorks];
            if (boat.p_eStep != Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                m_doVacuum.Write(true);
                if (Run(RunTurn(true))) return p_sInfo;
                if (Run(RunMoveX(eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVisionWorks))) return p_sInfo;
                m_doVacuum.Write(false);
                boat.RunVacuum(true);
                Thread.Sleep((int)(1000 * m_secVacuum));
                boat.p_infoStrip = p_infoStrip;
                p_infoStrip = null;
                if (Run(RunMoveZ(c_sReady))) return p_sInfo;
                if (Run(RunMoveX(c_sReady))) return p_sInfo;
                if (Run(RunTurn(false))) return p_sInfo;
            }
            finally
            {
                RunMoveZ(c_sReady);
                RunMoveX(c_sReady);
                RunTurn(false);
            }
            return "OK";
        }
        #endregion

        #region override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            if (p_infoStrip == null) return "OK";
            Run_Unload run = (Run_Unload)m_runUnload.Clone();
            run.m_eWorks = p_infoStrip.m_eWorks; 
            return StartRun(run);
        }

        public override void Reset()
        {
            base.Reset();
            p_infoStrip = null;
            RunMoveZ(c_sReady);
            RunMoveX(c_sReady);
            RunTurn(false); 
        }
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeVacuum(tree.GetTree("Vacuum")); 
        }
        #endregion

        public InfoStrip p_infoStrip { get; set; }
        Pine2 m_pine2 = null;
        Boats m_boats; 
        public Loader2(string id, IEngineer engineer, Pine2_Handler handler)
        {
            p_infoStrip = null; 
            m_pine2 = handler.m_pine2;
            m_boats = handler.m_aBoats[Vision.eVision.Bottom];
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runUnload; 
        protected override void InitModuleRuns()
        {
            m_runUnload = AddModuleRunList(new Run_Unload(this), true, "Unload Strip to Boat");
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader2 m_module;
            public Run_Unload(Loader2 module)
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
        #endregion
    }
}
