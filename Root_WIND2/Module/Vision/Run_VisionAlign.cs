﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_VisionAlign : ModuleRunBase
    {
        Vision m_module;
        public RPoint m_firstPointPulse = new RPoint();
        public RPoint m_secondPointPulse = new RPoint();
        public int m_focusPosZ = 0;
        public Camera_Basler m_CamAlign;

        public bool m_saveAlignFailImage = false;
        public string m_saveAlignFailImagePath = "D:\\";

        public int m_searchRangeX = 100;
        public int m_searchRangeY = 100;
        public int m_score = 80;

        public int m_repeatCnt = 1;
        public int m_failMovePulse = 10000; // 1mm

        public double m_AlignCamResolution = 5.5f;

        const int PULSE_TO_UM = 10;
        

        public Run_VisionAlign(Vision module)
        {
            m_module = module;
            m_CamAlign = m_module.CamAlign;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_VisionAlign run = new Run_VisionAlign(m_module);
            run.m_firstPointPulse = m_firstPointPulse;
            run.m_secondPointPulse = m_secondPointPulse;
            run.m_focusPosZ = m_focusPosZ;
            run.m_searchRangeX = m_searchRangeX;
            run.m_searchRangeY = m_searchRangeY;
            run.m_score = m_score;
            run.m_saveAlignFailImage = m_saveAlignFailImage;
            run.m_AlignCamResolution = m_AlignCamResolution;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_firstPointPulse = tree.Set(m_firstPointPulse, m_firstPointPulse, "First Align Point", "First Align Point (pulse)", bVisible);
            m_secondPointPulse = tree.Set(m_secondPointPulse, m_secondPointPulse, "Second Align Point", "Second Align Point (pulse)", bVisible);
            m_focusPosZ = tree.Set(m_focusPosZ, m_focusPosZ, "Focus Position Z", "Focus Position Z", bVisible);
            m_searchRangeX = tree.Set(m_searchRangeX, m_searchRangeX, "CC Search Range X", "CC Saerch Range X", bVisible);
            m_searchRangeY = tree.Set(m_searchRangeY, m_searchRangeY, "CC Search Range Y", "CC Saerch Range Y", bVisible);
            m_score = tree.Set(m_score, m_score, "Matching Score", "Matching Score", bVisible);

            //m_saveAlignFailImage = tree.Set(m_saveAlignFailImage, m_saveAlignFailImage, "Save Align Fail Image", "Save Align Fail Image", bVisible);
            m_saveAlignFailImagePath = tree.SetFolder(m_saveAlignFailImagePath, m_saveAlignFailImagePath, "Align Feature Path", "Align Feature Path", bVisible);
            m_AlignCamResolution = tree.Set(m_AlignCamResolution, m_AlignCamResolution, "Align Cam Resolution", "Align Cam Resolution", bVisible);
        }

        public override string Run()
        {
            //AxisXY axisXY = m_module.AxisXY;
            //Axis axisZ = m_module.AxisZ;

            //if (m_module.Run(axisZ.StartMove(m_focusPosZ)))
            //    return p_sInfo;  
            //if (m_module.Run(axisXY.StartMove(m_firstPointPulse)))
            //    return p_sInfo;

            //if (m_module.Run(axisZ.WaitReady()))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;


            //string strVRSImageDir = "D:\\"; 

            //ImageData img = m_CamAlign.p_ImageViewer.p_ImageData;
            //if (m_CamAlign.Grab() == "OK")
            //{
            //    //strVRSImageFullPath = string.Format(strVRSImageDir + "VRSImage_{0}.bmp", 0);
            //    //img.SaveImageSync(strVRSImageFullPath);
            //    //Grab error
            //}


            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;
            
            ImageData img = new ImageData(2600, 2625, 1);
            //m_CamAlign.GetRoiSize().X
            img.SetBackGroundWorker();
            img.OpenFile(@"C:\Users\cgkim\Desktop\image\src.bmp", new CPoint(0, 0));

            for (int i = 0; i < int.MaxValue; i++) ;
            IntPtr src = img.GetPtr();

            //Mat matSrc = new Mat(new Size(2600, 2625), DepthType.Cv8U, 3, img.GetPtr(), (int)img.p_Stride);
            //Emgu.CV.UI.ImageViewer.Show(matSrc);

            //ImageData temp = new ImageData(632, 381, 3);
            //temp.OpenFile(@"C:\Users\cgkim\Desktop\image\temp.bmp", new CPoint(0, 0));
            //temp.SetBackGroundWorker();
            
            //for (int i = 0; i < int.MaxValue; i++) ;
            //IntPtr temp2 = temp.GetPtr();
            //Mat t = new Mat(new Size(637, 377), DepthType.Cv8U, 1, temp.GetPtr(), (int)temp.p_Stride);
            //Emgu.CV.UI.ImageViewer.Show(t);

            //? 카메라 5.5um / 11um 

            DirectoryInfo di = new DirectoryInfo(@"C:\Users\cgkim\Desktop\image\template");

            int resPosX = 0;
            int resPosY = 0;
            float result = 0.0f;
            float maxScore = 0;
            foreach (FileInfo file in di.GetFiles())
            {
                string filename = file.Name.Substring(0, file.Name.Length - 4);
                string fullname = file.FullName;


                int posX = 0, posY = 0;
                unsafe
                {
                    //byte* rawdata = null;
                    int width = 0, height = 0;
                    //int* pWidth = &width;
                    //int* pHeight = &height;
                    byte[] rawdata = Tools.LoadBitmapToRawdata(fullname, &width, &height);
                    
                    result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), rawdata, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
                }
                if(maxScore < result)
                {
                    maxScore = result;
                    resPosX = posX;
                    resPosY = posY;
                }

                //String FileNameOnly = File.Name.Substring(0, File.Name.Length - 4);
                //String FullFileName = File.FullName;

                //MessageBox.Show(FullFileName + " " + FileNameOnly);
            }
            if (maxScore < m_score)
                return "First Point Align Fail [Score : " + maxScore.ToString() + "]";

         
           
          
            camWidth = 2200;
            camHeight = 2200;
            //resPosX = 0;
            //resPosY = 2200;
            m_firstPointPulse = new RPoint(100000, 100000);
            double cx = m_firstPointPulse.X / PULSE_TO_UM - ((camWidth / 2) + resPosX) * 5.5;
            double cy = m_firstPointPulse.Y / PULSE_TO_UM - ((camHeight / 2) + resPosY) * 5.5; 


            int posX2 = 0, posY2 = 0;

            m_secondPointPulse = new RPoint(150000, 100000);
            double cx2 = m_secondPointPulse.X / PULSE_TO_UM - ((camWidth / 2) + posX2) * 5.5;
            double cy2 = m_secondPointPulse.Y / PULSE_TO_UM - ((camHeight / 2) + posY2) * 5.5;

            //cx2 - cx  cy2 - cy1
            double v = Math.Atan2(cy2 - cy, cx2 - cx);

            int t = 10;
            //double x = (m_secondPointPulse.X - m_firstPointPulse.X) * 10 / 5.5;

            //int dX = posX - posX2;

            //float result = 0.0f;

            //////? 현재 이미지 가져옴

            ////int startX = (absPos.X - this.parameter.SearchRangeX) < 0 ? 0 : (absPos.X - this.parameter.SearchRangeX);
            ////int startY = (absPos.Y - this.parameter.SearchRangeY) < 0 ? 0 : (absPos.Y - this.parameter.SearchRangeY);
            ////int endX = (absPos.X + feature.Width + this.parameter.SearchRangeX) >= this.workplace.SharedBufferWidth ? this.workplace.SharedBufferWidth : (absPos.X + feature.Width + this.parameter.SearchRangeX);
            ////int endY = (absPos.Y + feature.Height + this.parameter.SearchRangeY) >= this.workplace.SharedBufferHeight ? this.workplace.SharedBufferHeight : (absPos.Y + feature.Height + this.parameter.SearchRangeY);

            ////unsafe
            ////{
            ////    result = CLR_IP.Cpp_TemplateMatching(src, temp2, posX, posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, temp.GetBitMapSource().PixelWidth, temp.GetBitMapSource().PixelHeight, );
            ////}

            //if (result < m_score)
            //{
            //    //if (m_saveAlignFailImage)
            //    //{
            //    //    string strVRSImageFullPath = "";

            //    //    strVRSImageFullPath = m_saveAlignFailImagePath + m_module.p_infoWafer.p_sCarrierID +"_"+ m_module.p_infoWafer.p_sSlotID +"_"+ m_module.p_infoWafer.p_sWaferID + "FirstPoint.bmp";
            //    //    img.SaveImageSync(strVRSImageFullPath);
            //    //}
            //    return "Align Fail";
            //}

            //if (m_module.Run(axisXY.StartMove(m_secondPointPulse)))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;

            //if (m_CamAlign.Grab() == "OK")
            //{
            //    string strVRSImageFullPath = "";
            //    strVRSImageFullPath = m_saveAlignFailImagePath + "FirstPoint.bmp";
            //    img.SaveImageSync(strVRSImageFullPath);
            //    //Grab error
            //}



            //result = 0.0f;
            //unsafe
            //{
            //    result = CLR_IP.Cpp_TemplateMatching(src, temp2, posX, posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, temp.GetBitMapSource().PixelWidth, temp.GetBitMapSource().PixelHeight, );
            //}


            //if (result < m_score)
            //{
            //    return "Align Fail Error";
            //}


            return "OK";
        }
    }
}