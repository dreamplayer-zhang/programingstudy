using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_AOP01_Packing
{
    public class Vision_AOP : ModuleBase
    {
        Camera_Basler m_tapeVRS;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetCamera(ref m_tapeVRS, this, "Tape VRS");
        }
        public Vision_AOP(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
            //m_tapeVRS.Connect();
            //ImageData img = m_tapeVRS.p_ImageViewer.p_ImageData;
            //while (m_tapeVRS.p_CamInfo._OpenStatus == false)
            //{
            //}
            //m_tapeVRS.GrabContinuousShot();
        }
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }
        void RunThreadCheck()
        {
            //Tape Check?
        }
        public class Run_VRSTEST : ModuleRunBase
        {
            Vision_AOP m_module;
            public Run_VRSTEST(Vision_AOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            public override ModuleRunBase Clone()
            {
                Run_VRSTEST run = new Run_VRSTEST(m_module);
                return run;
            }
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                
            }
            int i = 0;
            public override string Run()
            {
                Camera_Basler VRS = m_module.m_tapeVRS;
                ImageData img = VRS.p_ImageViewer.p_ImageData;
                
                if (VRS.Grab() == "OK")
                {
                    string strVRSImageFullPath = string.Format("D:\\VRSImage_{0}.bmp", i);
                    i++;
                    img.SaveImageSync(strVRSImageFullPath);
                    //Grab error
                }
                return "OK";
            }
        }

    }
}
