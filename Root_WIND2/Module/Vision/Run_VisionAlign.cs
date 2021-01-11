using Emgu.CV;
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
using System.Runtime.InteropServices;
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

        public int m_score = 80;

        public int m_repeatCnt = 1;
        public int m_failMovePulse = 10000; // 1mm

        public double m_AlignCamResolution = 5.5f;
        public int m_AlignCount = 1;

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
            run.m_score = m_score;
            run.m_saveAlignFailImage = m_saveAlignFailImage;
            run.m_saveAlignFailImagePath = m_saveAlignFailImagePath;
            run.m_AlignCamResolution = m_AlignCamResolution;
            run.m_AlignCount = m_AlignCount;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_firstPointPulse = tree.Set(m_firstPointPulse, m_firstPointPulse, "First Align Point", "First Align Point (pulse)", bVisible);
            m_secondPointPulse = tree.Set(m_secondPointPulse, m_secondPointPulse, "Second Align Point", "Second Align Point (pulse)", bVisible);
            m_focusPosZ = tree.Set(m_focusPosZ, m_focusPosZ, "Focus Position Z", "Focus Position Z", bVisible);
            m_score = tree.Set(m_score, m_score, "Matching Score", "Matching Score", bVisible);
            m_saveAlignFailImagePath = tree.SetFolder(m_saveAlignFailImagePath, m_saveAlignFailImagePath, "Align Feature Path", "Align Feature Path", bVisible);
            m_AlignCamResolution = tree.Set(m_AlignCamResolution, m_AlignCamResolution, "Align Cam Resolution", "Align Cam Resolution", bVisible);
            m_AlignCount = tree.Set(m_AlignCount, m_AlignCount, "Align count", "Align Count", bVisible);
        }

        int i = 1;
        public override string Run()
        {
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;

            if (m_module.Run(axisZ.StartMove(m_focusPosZ)))
                return p_sInfo;
            if (m_module.Run(axisXY.StartMove(m_firstPointPulse)))
                return p_sInfo;

            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;


            string strVRSImageDir = @"C:\Users\ATI\Desktop\image\";
            string strVRSImageFullPath = "";


            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;
            
            // 이미지 회득
            ImageData img = m_CamAlign.p_ImageViewer.p_ImageData;
            if (m_CamAlign.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", i++);
                img.SaveImageSync(strVRSImageFullPath);
            }
            IntPtr src = img.GetPtr();


            bool IsFirst = true;
            byte[] rawdata;

            int firstPosX = 0, firstPosY = 0, secondPosX = 0, secondPosY = 0;
            int width = 0, height = 0;
            int posX = 0, posY = 0;

            // Feature 이미지들 저장한곳 찿
            DirectoryInfo di = new DirectoryInfo(m_saveAlignFailImagePath);

            int resPosX = 0;
            int resPosY = 0;
            float result = 0.0f;
            float maxScore = 0;
            string matchingFeaturePath = "";
            byte[] firstRawData = null;
            int firstWidth = 0, firstHeight = 0;
            foreach (FileInfo file in di.GetFiles())
            {
                string fullname = file.FullName;
                //byte[] rawdata;
                //int width = 0, height = 0;
                unsafe
                {
                  
                    rawdata = Tools.LoadBitmapToRawdata(fullname, &width, &height);

                    result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), rawdata, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
                }
                if (maxScore < result)
                {
                    maxScore = result;
                    resPosX = posX;
                    resPosY = posY;
                    firstRawData = rawdata;
                    firstWidth = width;
                    firstHeight = height;
                    matchingFeaturePath = fullname;
                }
            }
            if (maxScore < m_score)
                return "First Point Align Fail [Score : " + maxScore.ToString() + "]";



            if (m_module.Run(axisXY.StartMove(m_secondPointPulse)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;


            if (m_CamAlign.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", i++);
                img.SaveImageSync(strVRSImageFullPath);
            }

            IntPtr src2 = img.GetPtr();

            int resPosX2 = 0;
            int resPosY2 = 0;
            float result2 = 0.0f;
            float maxScore2 = 0;
            string matchingFeaturePath2 = "";
            byte[] secondRawData = null;
            int secondWidth = 0, secondHeight = 0;
            foreach (FileInfo file in di.GetFiles())
            {
                string fullname = file.FullName;
                //byte[] rawdata;
                //int width = 0, height = 0;
                unsafe
                {
                   
                    rawdata = Tools.LoadBitmapToRawdata(fullname, &width, &height);

                    result2 = CLR_IP.Cpp_TemplateMatching((byte*)(src2.ToPointer()), rawdata, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
                }
                if (maxScore2 < result2)
                {
                    maxScore2 = result2;
                    resPosX2 = posX;
                    resPosY2 = posY;
                    secondRawData = rawdata;
                    secondWidth = width;
                    secondHeight = height;
                    matchingFeaturePath2 = fullname;
                }
            }
            if (maxScore2 < m_score)
                return "Second Point Align Fail [Score : " + maxScore2.ToString() + "]";

            double resAngle = CalcAngle(resPosX, resPosY, resPosX2, resPosY2);

            Axis axisRotate = m_module.AxisRotate;
            axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
            axisRotate.WaitReady();



            if (m_AlignCount > 1)
            {
                for (int cnt = 1; cnt < m_AlignCount; cnt++)
                {
                    //bool IsFirst = true;
                    //byte[] rawdata;

                    //int firstPosX = 0, firstPosY = 0, secondPosX = 0, secondPosY = 0;
                    //int width = 0, height = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
                        {
                            IsFirst = true;
                            rawdata = firstRawData;
                            width = firstWidth;
                            height = firstHeight;
                        }
                        else
                        {
                            IsFirst = false;
                            rawdata = secondRawData;
                            width = secondWidth;
                            height = secondHeight;
                        }

                        //if (m_CamAlign.Grab() != "OK") return "Grab Error";d
                        if (m_CamAlign.Grab() == "OK")
                        {
                            strVRSImageFullPath = string.Format(strVRSImageDir + "왜안돼{0}.bmp", i);
                            img.SaveImageSync(strVRSImageFullPath);
                        }
                        src = img.GetPtr();
                        int PosX = 0, PosY = 0;
                        unsafe
                        {
                            result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), rawdata, &PosX, &PosY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
                        }
                        if (IsFirst)
                        {
                            if (result < m_score)
                                return "Align Count :" + cnt.ToString() + "First Point Align Fail [Score : " + result.ToString() + "]";

                            firstPosX = PosX;
                            firstPosY = PosY;
                            if (i != 1)
                            {
                                if (m_module.Run(axisXY.StartMove(m_secondPointPulse)))
                                    return p_sInfo;
                                if (m_module.Run(axisXY.WaitReady()))
                                    return p_sInfo;

                                IsFirst = false;
                            }

                        }
                        else
                        {
                            if (result < m_score)
                                return "Align Count :" + cnt.ToString() + "Second Point Align Fail [Score : " + result.ToString() + "]";

                            secondPosX = PosX;
                            secondPosY = PosY;

                            if (i != 1)
                            {
                                if (m_module.Run(axisXY.StartMove(m_firstPointPulse)))
                                    return p_sInfo;
                                if (m_module.Run(axisXY.WaitReady()))
                                    return p_sInfo;

                                IsFirst = true;
                            }
                        }

                    }
                    resAngle = CalcAngle(firstPosX, firstPosY, secondPosX, secondPosY);

                    axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
                    axisRotate.WaitReady();

                    //if (m_CamAlign.Grab() != "OK") return "Grab Error";
                    //src2 = img.GetPtr();
                    //int secondPosX = 0, secondPosY = 0;
                    //unsafe
                    //{

                    //    int width = 0, height = 0;
                    //    byte[] rawdata = Tools.LoadBitmapToRawdata(matchingFeaturePath2, &width, &height);

                    //    result = CLR_IP.Cpp_TemplateMatching((byte*)(src2.ToPointer()), rawdata, &secondPosX, &secondPosY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
                    //}
                    //if (result < m_score)
                    //    return "Align Count :" + cnt.ToString() + "Second Point Align Fail [Score : " + result.ToString() + "]";


                }

            }


            //if (m_CamAlign.Grab() != "OK") return "Grab Error";
            if (m_CamAlign.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "asanwawnrnar{0}.bmp", i);
                img.SaveImageSync(strVRSImageFullPath);
            }

            RPoint pulse = new RPoint();
            if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
            {
                IsFirst = true;
                rawdata = firstRawData;
                width = firstWidth;
                height = firstHeight;
                pulse = m_firstPointPulse;
            }
            else
            {
                IsFirst = false;
                rawdata = secondRawData;
                width = secondWidth;
                height = secondHeight;
                pulse = m_secondPointPulse;
            }
            src = img.GetPtr();
            unsafe
            {
                CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), rawdata, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
            }
           
            if (m_module.Run(axisXY.p_axisX.StartMove(pulse.X - (posX + (width / 2) - camWidth / 2) * m_AlignCamResolution * 10)))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisX.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisY.StartMove(pulse.Y + (posY + (height / 2) - camHeight / 2) * m_AlignCamResolution * 10)))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisY.WaitReady()))
                return p_sInfo;

            m_module.AlignData = new RPoint(-(posX + (width / 2) - camWidth / 2) * m_AlignCamResolution * 10, (posY + (height / 2) - camHeight / 2) * m_AlignCamResolution * 10);
            return "OK";
        }

        private double CalcAngle(int resPosX, int resPosY, int resPosX2, int resPosY2)
        {
            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;
            double cx = m_firstPointPulse.X / PULSE_TO_UM - ((camWidth / 2) + resPosX) * m_AlignCamResolution;
            double cy = m_firstPointPulse.Y / PULSE_TO_UM - ((camHeight / 2) + resPosY) * m_AlignCamResolution;

            double cx2 = m_secondPointPulse.X / PULSE_TO_UM - ((camWidth / 2) + resPosX2) * m_AlignCamResolution;
            double cy2 = m_secondPointPulse.Y / PULSE_TO_UM - ((camHeight / 2) + resPosY2) * m_AlignCamResolution;


            double radian = Math.Atan2(cy2 - cy, cx2 - cx);
            double angle = radian * (180 / Math.PI);
            double resAngle = 0;
            if (cy2 - cy < 0)
            {
                resAngle = angle + 180;

            }
            else
            {
                resAngle = angle - 180;
            }

            return resAngle;
        }
    }
}
