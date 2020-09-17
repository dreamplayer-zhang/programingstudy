using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;

namespace Root_AxisMapping.Module
{
    public class AxisMapping : ModuleBase
    {
        #region ToolBox
        Axis m_axisRotate;
        AxisXY m_axisXY; 

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_axisXY, this, "Stage");
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
//            RunTreeRotate(tree.GetTree("Rotate", false));
//            RunTreeDIOWait(tree.GetTree("Timeout", false));
        }

        public override void Reset()
        {
            base.Reset();
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
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        public AxisMapping(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

    }
}
