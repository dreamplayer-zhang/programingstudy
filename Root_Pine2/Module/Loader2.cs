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

        public enum ePos
        {
            Ready,
        }
        void InitPosition()
        {
            m_axisXZ.AddPos(Enum.GetNames(typeof(ePos)));
            m_axisXZ.AddPos(GetPosString(Vision.eVisionWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVisionWorks.B));
        }

        string GetPosString(Vision.eVisionWorks eVisionWorks)
        {
            return Vision.eVision.Bottom.ToString() + eVisionWorks.ToString(); 
        }

        public string RunMoveX(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision.eVisionWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eVisionWorks ePos, bool bWait = true)
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

        #region InfoStrip
        public InfoStrip p_infoStrip { get; set; }
        #endregion

        #region RunUnload
        public string RunUnload(Vision.eVisionWorks eVisionWorks)
        {
            if (m_boats.m_aBoat[eVisionWorks].p_eStep != Boats.Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                m_doVacuum.Write(true);
                if (Run(RunTurn(true))) return p_sInfo;
                if (Run(RunMoveX(eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVisionWorks))) return p_sInfo;
                m_doVacuum.Write(false);
                m_boats.m_aBoat[eVisionWorks].RunVacuum(true);
                Thread.Sleep((int)(1000 * m_secVacuum));
                m_boats.m_aBoat[eVisionWorks].p_infoStrip = p_infoStrip;
                p_infoStrip = null;
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(ePos.Ready))) return p_sInfo;
                if (Run(RunTurn(false))) return p_sInfo;
            }
            finally
            {
                RunMoveZ(ePos.Ready);
                RunMoveX(ePos.Ready);
                RunTurn(false);
            }
            return "OK";
        }
        #endregion

        #region Run Loader2
        public string StartLoader2()
        {
            StartRun(m_runLoader2);
            return "OK";
        }

        public string RunLoader2()
        {
            while (EQ.p_eState == EQ.eState.Run)
            {
                Thread.Sleep(10); 
                if (p_infoStrip != null)
                {
                    if (m_boats.m_aBoat[p_infoStrip.m_eVisionWorks].p_eStep == Boats.Boat.eStep.Ready)
                    {
                        if (Run(RunUnload(p_infoStrip.m_eVisionWorks))) return p_sInfo; 
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
            p_infoStrip = null; 
        }
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeVacuum(tree.GetTree("Vacuum")); 
        }
        #endregion

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
        ModuleRunBase m_runLoader2; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Unload(this), false, "Unload Strip to Boat");
            m_runLoader2 = AddModuleRunList(new Run_Run(this), true, "Run Loader2");
        }

        public class Run_Unload : ModuleRunBase
        {
            Loader2 m_module;
            public Run_Unload(Loader2 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            Vision.eVisionWorks m_ePos = Vision.eVisionWorks.A;
            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
                run.m_ePos = m_ePos; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (Vision.eVisionWorks)tree.Set(m_ePos, m_ePos, "Boat", "Select Boat", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunUnload(m_ePos);
            }
        }

        public class Run_Run : ModuleRunBase
        {
            Loader2 m_module;
            public Run_Run(Loader2 module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoader2();
            }
        }
        #endregion
    }
}
