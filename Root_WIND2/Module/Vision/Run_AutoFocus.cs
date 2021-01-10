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
using Emgu.CV.Util;
using RootTools.Control;

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
            Axis axisZ = m_module.AxisZ;
                int ab = 0;
            //while (true)
            //{
            //    axisZ.StartMove(1000 * ab);

            //}
            
            ImageData img = new ImageData(656, 492, 3);
            //img.OpenFile(@"C:\Users\cgkim\Desktop\image\test.bmp",new CPoint(0,0));
            //IntPtr intPtr = img.GetPtr();
            img.LoadImageSync(@"C:\Users\cgkim\Desktop\VRS\Image__2021-01-08__09-10-16.bmp", new CPoint(0, 0));
            //for (int i = 0; i < int.MaxValue; i++) ;
            Mat matSrc = new Mat(new Size(656, 492), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
            Mat matLeft = new Mat(matSrc, new Rectangle(0, 0, 328, 492));
            Mat matRight = new Mat(matSrc, new Rectangle(328, 0, 328, 492));
            //Mat matHsv = new Mat();
            //Mat matRedMask = new Mat();
            //Mat matRedImg = new Mat();
            // Mat matHsv = new Mat(new Size(2000, 2000), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
            // matSrc.CopyTo(matHsv);
            VectorOfUMat rgb = new VectorOfUMat();// (656, 492, DepthType.Cv8U,1);


            CvInvoke.Split(matLeft, rgb);

            for(int i = 0; i < 3; i++)
            {
                //Emgu.CV.UI.ImageViewer.Show(rgb[i]);
            }
            CvInvoke.Threshold(rgb[2], rgb[2], 30, 255, ThresholdType.Binary);
            //Emgu.CV.UI.ImageViewer.Show(rgb[2]);
            //IntPtr ptr = rgb[2].Ptr;
            //ImageData d = new ImageData(656,492);
            //d.SetData(,ptr.scan0));
            //d.SetData(,);
            Bitmap a = rgb[0].Bitmap;
            int whitePixel = 0;
            //Array data = rgb[2].Data;Image<Gray , byte> grayImage;
            Image<Gray, byte> image = rgb[2].ToImage<Gray,byte>();
            for(int i = 0; i < image.Height; i++)
            {
                for(int j = 0; j < image.Width; j++)
                {
                    byte af = image.Data[i, j, 0];
                    if(af == 255)
                    {
                        whitePixel++;
                    }
                    //byte b = image.Data[i, j, 1];
                    //byte c = image.Data[i, j, 2];
                }
            }
            //for (int i = 0; i < rgb[2].Cols; i++)
            //{
            //    for(int j = 0; j < rgb[2].Rows; j++)
            //    {
            //        //if(rgb[2].Data[0,1,1])
            //    }
            //}
            Emgu.CV.UI.ImageViewer.Show(rgb[2]);
            CvInvoke.Split(matRight, rgb);

            for (int i = 0; i < 3; i++)
            {
                //Emgu.CV.UI.ImageViewer.Show(rgb[i]);
            }
            CvInvoke.Threshold(rgb[2], rgb[2], 30, 255, ThresholdType.Binary);
            Emgu.CV.UI.ImageViewer.Show(rgb[2]);

            int whitePixel2 = 0;
            image = rgb[2].ToImage<Gray, byte>();
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    byte af = image.Data[i, j, 0];
                    if (af == 255)
                    {
                        whitePixel2++;
                    }
                    //byte b = image.Data[i, j, 1];
                    //byte c = image.Data[i, j, 2];
                }
            }
            ////CvInvoke.CvtColor(matSrc, matHsv, ColorConversion.Bgr2Hsv);

            //MCvScalar lower_Red = new MCvScalar(0,0, 0);
            //MCvScalar Upper_Red = new MCvScalar(110, 254, 254);

            //CvInvoke.InRange(matHsv, new ScalarArray(lower_Red), new ScalarArray(Upper_Red), matRedMask);
            ////CvInvoke.BitwiseAnd(matSrc, matSrc, matRedImg, matRedMask);

            ////Mat matLeft = new Mat(matRedMask, new Rectangle(0, 0, 320, 480));
            ////Mat matRight = new Mat(matRedMask, new Rectangle(320, 0, 320, 480));
            //Emgu.CV.UI.ImageViewer.Show(matHsv);
            //Emgu.CV.UI.ImageViewer.Show(matSrc);
            //Emgu.CV.UI.ImageViewer.Show(matRedMask);
            //Emgu.CV.UI.ImageViewer.Show(matLeft);
            //Emgu.CV.UI.ImageViewer.Show(matRight);

            return "OK";
        }
    }
}
