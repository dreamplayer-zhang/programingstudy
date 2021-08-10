using Root_JEDI.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI.Module
{
    public class FlipperOut : ModuleBase
    {
        #region ToolBox
        public AxisXZ m_axis;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axis, this, "XZ");
            m_flipper.GetTools(m_toolBox, this, bInit);
            if (bInit)
            {
                InitPosition();
            }
        }
        #endregion

        #region AvoidX
        FlipperIn p_flipperIn { get { return m_handler.m_flipperIn; } }
        string StartMoveX(ePos ePos)
        {
            Axis axisX = p_flipperIn.m_axis.p_axisX;
            double fPos = m_axis.p_axisX.GetPosValue(ePos);
            while ((fPos + axisX.m_posDst) > p_flipperIn.c_lAxisX)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (p_flipperIn.IsBusy() == false)
                {
                    p_flipperIn.StartAvoidX(fPos);
                    Thread.Sleep(10);
                }
            }
            return m_axis.p_axisX.StartMove(fPos);
        }

        public string StartAvoidX(double fPos)
        {
            Run_AvoidX run = (Run_AvoidX)m_runAvoidX.Clone();
            run.m_fPos = p_flipperIn.c_lAxisX - fPos;
            return StartRun(run);
        }

        public string RunAvoidX(double fPos)
        {
            m_axis.p_axisX.StartMove(fPos);
            return m_axis.p_axisX.WaitReady();
        }
        #endregion

        #region Axis
        public enum ePos
        {
            TrayOutL,
            TrayOutR,
            Metal,
            Empty,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveX(ePos ePos, bool bWait = true)
        {
            if (Run(StartMoveX(ePos))) return p_sInfo; 
            return bWait ? m_axis.p_axisX.WaitReady() : "OK";
        }

        public string MoveZ(ePos ePos, double mmOffset, bool bWait = true)
        {
            m_axis.p_axisZ.StartMove(ePos, -1000 * mmOffset);
            return bWait ? m_axis.p_axisZ.WaitReady() : "OK";
        }
        #endregion




        Flipper m_flipper;
        JEDI_Handler m_handler;
        public FlipperOut(string id, IEngineer engineer)
        {
            p_id = id;
            m_flipper = new Flipper(id);
            m_handler = (JEDI_Handler)engineer.ClassHandler();
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runAvoidX;
        protected override void InitModuleRuns()
        {
            m_runAvoidX = AddModuleRunList(new Run_AvoidX(this), false, "Avoid Axis X");
        }

        public class Run_AvoidX : ModuleRunBase
        {
            FlipperOut m_module;
            public Run_AvoidX(FlipperOut module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public double m_fPos = 0;
            public override ModuleRunBase Clone()
            {
                Run_AvoidX run = new Run_AvoidX(m_module);
                run.m_fPos = m_fPos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fPos = tree.Set(m_fPos, m_fPos, "Position", "Axis X Avoid Position", bVisible);
            }

            public override string Run()
            {
                return m_module.RunAvoidX(m_fPos);
            }
        }
        #endregion
    }
}
