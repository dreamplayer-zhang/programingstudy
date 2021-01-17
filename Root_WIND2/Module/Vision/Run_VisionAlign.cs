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

        public GrabMode m_grabMode = null;
        string m_sGrabMode = "";

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }


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
            run.p_sGrabMode = p_sGrabMode;
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
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
        }

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
            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;

            // 이미지 회득
            ImageData img = m_CamAlign.p_ImageViewer.p_ImageData;
            string strVRSImageFullPath;
            if (m_CamAlign.Grab() == "OK")
            {
                strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", 0);
                img.SaveImageSync(strVRSImageFullPath);
            }
            IntPtr src = img.GetPtr();
            byte[] rawdata;

            int firstPosX = 0, firstPosY = 0, secondPosX = 0, secondPosY = 0;
            int width = 0, height = 0;
            int posX = 0, posY = 0;

            // Feature 이미지들 저장한곳 찿
            DirectoryInfo di = new DirectoryInfo(m_saveAlignFailImagePath);

            int resPosX = 0;
            int resPosY = 0;
            float maxScore = 0;
            string matchingFeaturePath = "";
            byte[] findRawData = null;
            int findWidth = 0, findHeight = 0;
            float result;
            foreach (FileInfo file in di.GetFiles())
            {
                string fullname = file.FullName;
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
                    findRawData = rawdata;
                    findWidth = width;
                    findHeight = height;
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
                strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", 1);
                img.SaveImageSync(strVRSImageFullPath);
            }

            IntPtr src2 = img.GetPtr();

            int resPosX2 = 0;
            int resPosY2 = 0;

            unsafe
            {
                result = CLR_IP.Cpp_TemplateMatching((byte*)(src2.ToPointer()), findRawData, &resPosX2, &resPosY2, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
            }

            if (result < m_score)
                return "Second Point Align Fail [Score : " + result.ToString() + "]";

            double resAngle = CalcAngle(resPosX, resPosY, resPosX2, resPosY2);

            Axis axisRotate = m_module.AxisRotate;
            axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
            axisRotate.WaitReady();



            if (m_AlignCount > 1)
            {
                for (int cnt = 1; cnt < m_AlignCount; cnt++)
                {

                    for (int i = 0; i < 2; i++)
                    {


                        bool IsFirst;
                        if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
                        {
                            IsFirst = true;
                        }
                        else
                        {
                            IsFirst = false;
                        }

                        if (m_CamAlign.Grab() == "OK")
                        {
                            strVRSImageFullPath = string.Format(strVRSImageDir + "Repeat Img{0}.bmp", cnt + (i + 1));
                            img.SaveImageSync(strVRSImageFullPath);
                        }
                        src = img.GetPtr();
                        int PosX = 0, PosY = 0;
                        unsafe
                        {
                            result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), findRawData, &PosX, &PosY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
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
                            }
                        }

                    }
                    resAngle = CalcAngle(firstPosX, firstPosY, secondPosX, secondPosY);

                    axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
                    axisRotate.WaitReady();
                }

            }

            if (m_CamAlign.Grab() != "OK") return "Grab Error";
            RPoint pulse;
            if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
            {
                pulse = m_firstPointPulse;
            }
            else
            {
                pulse = m_secondPointPulse;
            }
            src = img.GetPtr();
            unsafe
            {
                CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), findRawData, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3);
            }

            if (m_module.Run(axisXY.p_axisX.StartMove(pulse.X - (posX + (width / 2) - camWidth / 2) * m_AlignCamResolution * 10)))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisX.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisY.StartMove(pulse.Y + (posY + (height / 2) - camHeight / 2) * m_AlignCamResolution * 10)))
                return p_sInfo;
            if (m_module.Run(axisXY.p_axisY.WaitReady()))
                return p_sInfo;


            m_grabMode.m_ptXYAlignData = new RPoint(-(posX + (width / 2) - camWidth / 2) * m_AlignCamResolution * 10, (posY + (height / 2) - camHeight / 2) * m_AlignCamResolution * 10);
            m_module.RunTree(Tree.eMode.RegWrite);
            m_module.RunTree(Tree.eMode.Init);


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
            double resAngle;
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
