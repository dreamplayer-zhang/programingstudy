using RootTools.Camera.BaslerPylon;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using RootTools;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Root_WIND2.Module
{
    public class Run_AutoFocus : ModuleRunBase
    {
        Vision m_module;
        Camera_Basler m_CamAutoFocus;
        public Run_AutoFocus(Vision module)
        {
            m_module = module;
            m_CamAutoFocus = m_module.CamAutoFocus;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_AutoFocus run = new Run_AutoFocus(m_module);
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
        }

        public override string Run()
        {
            ImageData img = new ImageData(2000, 2000, 3);
            //img.OpenFile(@"C:\Users\cgkim\Desktop\image\test.bmp",new CPoint(0,0));
            //IntPtr intPtr = img.GetPtr();
            img.LoadImageSync(@"C:\Users\cgkim\Desktop\image\test.bmp", new CPoint(0, 0));
            for (int i = 0; i < int.MaxValue; i++) ;
            Mat matSrc = new Mat(new Size(2000, 2000), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
            Mat matLeft = new Mat(matSrc, new Rectangle(0,0,1000,2000));
            Mat matRight = new Mat(matSrc, new Rectangle(1000, 0, 1000, 2000));
            Mat matHsv = new Mat();
            Mat matRedMask = new Mat();
            Mat matRedImg = new Mat();
            // Mat matHsv = new Mat(new Size(2000, 2000), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
            // matSrc.CopyTo(matHsv);

            CvInvoke.CvtColor(matSrc, matHsv, ColorConversion.Bgr2Hsv);
            MCvScalar lower_Red = new MCvScalar(0,90, 90);
            MCvScalar Upper_Red = new MCvScalar(10, 255, 255);
            
            CvInvoke.InRange(matHsv, new ScalarArray(lower_Red), new ScalarArray(Upper_Red), matRedMask);
            CvInvoke.BitwiseAnd(matSrc, matSrc, matRedImg, matRedMask);
             Emgu.CV.UI.ImageViewer.Show(matSrc);
            Emgu.CV.UI.ImageViewer.Show(matRedImg);
            //Emgu.CV.UI.ImageViewer.Show(matLeft);
            //Emgu.CV.UI.ImageViewer.Show(matRight);

            return "OK";
        }
    }
}
