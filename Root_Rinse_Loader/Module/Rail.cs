using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Rinse_Loader.Module
{
    public class Rail : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisWidth, this, "Width");
            foreach (Line line in m_aLine) line.GetTools(m_toolBox); 
            if (bInit) 
            {
                InitPosWidth(); 
            }
        }
        #endregion

        #region Line
        public class Line
        {
            DIO_I[] m_diCheck = new DIO_I[3]; 
            public void GetTools(ToolBox toolBox)
            {
                m_rail.p_sInfo = toolBox.Get(ref m_diCheck[0], m_rail, m_id + ".Check0");
                m_rail.p_sInfo = toolBox.Get(ref m_diCheck[1], m_rail, m_id + ".Check1");
                m_rail.p_sInfo = toolBox.Get(ref m_diCheck[2], m_rail, m_id + ".Check2");
            }

            string m_id;
            Rail m_rail; 
            public Line(string id, Rail rail)
            {
                m_id = id;
                m_rail = rail; 
            }
        }

        List<Line> m_aLine = new List<Line>(); 
        void initILines()
        {
            for (int n = 0; n < 4; n++) m_aLine.Add(new Line("Line" + n.ToString(), this)); 
        }
        #endregion

        #region Width
        Axis m_axisWidth;
        public enum ePos
        {
            W70,
            W100
        }
        void InitPosWidth()
        {
            m_axisWidth.AddPos(Enum.GetNames(typeof(ePos))); 
        }

        public string RunMoveWidth(double fWidth)
        {
            double fW70 = m_axisWidth.GetPosValue(ePos.W70);
            double fW100 = m_axisWidth.GetPosValue(ePos.W100);
            double dPos = (fW100 - fW70) * (fWidth - 70) / 30;
            m_axisWidth.StartMove(dPos);
            return m_axisWidth.WaitReady(); 
        }
        #endregion

        #region Rotate
        double m_fJogScale = 1;
        Axis m_axisRotate;

        public string RunRotate(bool bRotate)
        {
            m_axisRotate.Jog(m_fJogScale);
            return "OK"; 
        }

        void RunTreeRotate(Tree tree)
        {
            m_fJogScale = tree.Set(m_fJogScale, m_fJogScale, "Speed", "Rotate Speed (Scale)"); 
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            m_axisRotate.ServoOn(true); 
            p_sInfo = base.StateHome(m_axisWidth);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeRotate(tree.GetTree("Rotate", false));
        }
        #endregion

        public Rail(string id, IEngineer engineer)
        {
            p_id = id;
            initILines(); 
            InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region StartRun
        public void StartRun()
        {
            switch (Rinse.p_eMode)
            {
                case Rinse.eMode.Magazine:
                    RunMoveWidth(Rinse.p_widthStrip);
                    RunRotate(true); 
                    break;
                case Rinse.eMode.Stack:
                    RunRotate(false); 
                    break; 
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_MoveWidth(this), false, "Move Rail Width");
            AddModuleRunList(new Run_Rotate(this), false, "Rail Rotate");
        }

        public class Run_MoveWidth : ModuleRunBase
        {
            Rail m_module;
            public Run_MoveWidth(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fWidth = 77; 
            public override ModuleRunBase Clone()
            {
                Run_MoveWidth run = new Run_MoveWidth(m_module);
                run.m_fWidth = m_fWidth; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fWidth = tree.Set(m_fWidth, m_fWidth, "Width", "Rail Width (mm)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunMoveWidth(m_fWidth); 
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            Rail m_module;
            public Run_Rotate(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bRotate = false; 
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_bRotate = m_bRotate;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRotate = tree.Set(m_bRotate, m_bRotate, "Rotate", "Rotate Rail", bVisible);
            }

            public override string Run()
            {
                return m_module.RunRotate(m_bRotate);
            }
        }
        #endregion
    }
}
