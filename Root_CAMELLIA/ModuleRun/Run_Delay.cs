using Emgu.CV;
using Root_CAMELLIA.Module;
using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Module
{
    public class Run_Delay : ModuleRunBase
    {
        Module_Camellia m_module;
        double m_secDelay = 2;
        RPoint pt = new RPoint();
        double ptZ = 0;
        public Run_Delay(Module_Camellia module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_Delay run = new Run_Delay(m_module);
            run.pt = pt;
            run.m_secDelay = m_secDelay;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            pt = tree.Set(pt, pt, "test", "test", bVisible);
            ptZ = tree.Set(ptZ, ptZ, "testZ", "testZ", bVisible);
            m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
        }

        public override string Run()
        {
            Thread.Sleep((int)(1000 * m_secDelay));
            //if (m_module.LifterDown() != "OK")
            //{
            //    return p_sInfo;
            //}


            //Camera_Basler VRS = m_module.p_CamVRS;
            //ImageData img = VRS.p_ImageViewer.p_ImageData;

            //Axis axisZ = m_module.p_axisZ;
            //if (m_module.Run(axisZ.StartMove(ptZ)))
            //{
            //    return p_sInfo;
            //}
            //if (m_module.Run(axisZ.WaitReady()))
            //    return p_sInfo;

            //AxisXY axisXY = m_module.p_axisXY;

            //if (m_module.Run(axisXY.StartMove(pt)))
            //{
            //    return p_sInfo;
            //}
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;

            //if (VRS.Grab() == "OK")
            //{
            //    Mat mat = new Mat(new Size(VRS.GetRoiSize().X, VRS.GetRoiSize().Y), Emgu.CV.CvEnum.DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride * 3);
            //    mat.Save(@"C:\Users\ATI\Desktop\repeatTest\" + DateTime.Now.ToString("HHmmss") + ".bmp");
            //}

            return "OK";
        }
    }
}
