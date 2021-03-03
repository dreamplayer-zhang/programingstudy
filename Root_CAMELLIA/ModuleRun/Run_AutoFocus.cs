using Root_CAMELLIA.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.ModuleRun
{
    public class Run_AutoFocus : ModuleRunBase
    {
        Module_Camellia m_module;

        RPoint m_ptAutoFocusPos = new RPoint();
        int m_nStartFocusPosZ = 0;


        int nFrame = 0;
        public Run_AutoFocus(Module_Camellia module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_AutoFocus run = new Run_AutoFocus(m_module);
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_ptAutoFocusPos = tree.Set(m_ptAutoFocusPos, m_ptAutoFocusPos, "Set Auto Focus Position", "Set Auto Focus Position", bVisible);
            m_nStartFocusPosZ = tree.Set(m_nStartFocusPosZ, m_nStartFocusPosZ,"Set Z Axis Start Position", "Set Z Axis Start Position", bVisible);
        }

        public override string Run()
        {
            AxisXY axisXY = m_module.p_axisXY;
            Axis axisZ = m_module.p_axisZ;

            if (m_module.Run(axisXY.StartMove(m_ptAutoFocusPos)))
                return p_sInfo;
            if (m_module.Run(axisZ.StartMove(m_nStartFocusPosZ)))
                return p_sInfo;
            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

           

            Camera_Basler VRS = m_module.p_CamVRS;

            VRS.Grabed -= OnImageGrabbed;
            VRS.Grabed += OnImageGrabbed;

            nFrame = 0;
            
            // Autofocus 진행.
            VRS.GrabContinuousShot();





            


            return "OK";
        }

        private void OnImageGrabbed(Object sender, EventArgs e)
        {
            nFrame++;
        }
    }
}
