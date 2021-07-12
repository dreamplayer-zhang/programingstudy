using Emgu.CV;
using RootTools;
using RootTools.Camera;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Memory;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_CLR;
using System;

namespace Root_EFEM.Module.FrontsideVision
{
    public class Run_VisionAlign : ModuleRunBase
    {
        Vision_Frontside m_module;
        //public RPoint m_firstPointPulse = new RPoint();
        //public RPoint m_secondPointPulse = new RPoint();
        public int m_focusPosZ = 0;
        public Camera_Basler m_CamAlign;

        //public bool m_saveAlignFailImage = false;
        //public string m_saveAlignFailImagePath = "D:\\";

        public int m_score = 80;

        public int m_repeatCnt = 1;
        public int m_failMovePulse = 10000; // 1mm

        //public double m_AlignCamResolution = 5.5f;
        public int m_AlignCount = 1;

        const int PULSE_TO_UM = 10;

        public double m_dVRSToAlignOffsetZ = 0;

        public GrabModeFront m_grabMode = null;
        string m_sGrabMode = "";



        // New
        #region [Properties]
        private string m_sRecipeName = "";
        public string RecipeName
        {
            get => this.m_sRecipeName;
            set => this.m_sRecipeName = value;
        }
        #endregion


        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }


        public Run_VisionAlign(Vision_Frontside module)
        {
            m_module = module;
            m_CamAlign = m_module.CamAlign;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_VisionAlign run = new Run_VisionAlign(m_module);
            //run.m_firstPointPulse = m_firstPointPulse;
            //run.m_secondPointPulse = m_secondPointPulse;
            run.m_focusPosZ = m_focusPosZ;
            run.m_score = m_score;
            //run.m_saveAlignFailImage = m_saveAlignFailImage;
            //run.m_saveAlignFailImagePath = m_saveAlignFailImagePath;
            //run.m_AlignCamResolution = m_AlignCamResolution;
            run.m_AlignCount = m_AlignCount;
            run.p_sGrabMode = p_sGrabMode;
            run.m_dVRSToAlignOffsetZ = m_dVRSToAlignOffsetZ;

            // New
            run.m_sRecipeName = this.m_sRecipeName;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            //m_firstPointPulse = tree.Set(m_firstPointPulse, m_firstPointPulse, "First Align Point", "First Align Point (pulse)", bVisible);
            //m_secondPointPulse = tree.Set(m_secondPointPulse, m_secondPointPulse, "Second Align Point", "Second Align Point (pulse)", bVisible);
            m_focusPosZ = tree.Set(m_focusPosZ, m_focusPosZ, "Focus Position Z", "Focus Position Z", bVisible);
            m_score = tree.Set(m_score, m_score, "Matching Score", "Matching Score", bVisible);
            //m_saveAlignFailImagePath = tree.SetFolder(m_saveAlignFailImagePath, m_saveAlignFailImagePath, "Align Feature Path", "Align Feature Path", bVisible);
            //m_AlignCamResolution = tree.Set(m_AlignCamResolution, m_AlignCamResolution, "Align Cam Resolution", "Align Cam Resolution", bVisible);
            m_AlignCount = tree.Set(m_AlignCount, m_AlignCount, "Align count", "Align Count", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dVRSToAlignOffsetZ = tree.Set(m_dVRSToAlignOffsetZ, m_dVRSToAlignOffsetZ, "VRS To Align Offset Z Pos", "VRS To Align Offset Z Pos", bVisible);

            // New
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
        }

        public override string Run()
        {
            // 210603 New
            StopWatch timer = new StopWatch();
            timer.Start();


            // Camera Connection
            if (m_CamAlign == null) return "Cam == null";

            m_CamAlign.Connect();
            while (!m_CamAlign.m_ConnectDone)
            {
                if (EQ.IsStop()) return "OK";
            }

            // Init
            m_module.p_bStageVac = true;
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;

            if (m_grabMode == null) return "Grab Mode == null";

            m_grabMode.SetLight(true);

            //Recipe Open
            RecipeAlign recipe = GlobalObjects.Instance.Get<RecipeAlign>();
            FrontAlignRecipe alignRecipe = recipe.GetItem<FrontAlignRecipe>();

            if(alignRecipe == null)
            {
                return "FrontAlignRecipe == null";
            }

            if (!recipe.Read(m_sRecipeName))
            {
                return "Recipe Open Fail";
            }

            if (alignRecipe.AlignFeatureList.Count == 0)
                return "Align Featur Count == 0";

            // Move Z
            double dPos = m_focusPosZ;
            if (m_grabMode.m_dVRSFocusPos != 0)
            {
                dPos = m_grabMode.m_dVRSFocusPos + m_dVRSToAlignOffsetZ;
            }

            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

            if (m_module.Run(axisZ.StartMove(dPos)))
                return p_sInfo;

            if (m_module.Run(axisZ.WaitReady()))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;



            RPoint firstPoint = new RPoint(alignRecipe.FirstSearchPointX, alignRecipe.FirstSearchPointY);
            RPoint secondPoint = new RPoint(alignRecipe.SecondSearchPointX, alignRecipe.SecondSearchPointY);

            // First Point
            if (m_module.Run(axisXY.StartMove(firstPoint)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;

            long outX, outY;
            int featureIndex;

            double score = TemplateMatching(alignRecipe, out outX, out outY, out featureIndex);
            if (score < m_score)
            {
                return "First Point Align Fail [Score : " + score.ToString() + "]";
            }

            // Second Point
            if (m_module.Run(axisXY.StartMove(secondPoint)))
                return p_sInfo;
            if (m_module.Run(axisXY.WaitReady()))
                return p_sInfo;


            // 두번째는 첫번째에서 Score가 가장 높은 feature로 함
            long outX2, outY2;
            double score2 = TemplateMatchingWithSelectedFeature(alignRecipe, featureIndex, out outX2, out outY2);


            if (score2 < m_score)
                return "Second Point Align Fail [Score : " + score2.ToString() + "]";


            double dAngle = CalcAngle(outX, outY, outX2, outY2);

            Axis axisRotate = m_module.AxisRotate;
            axisRotate.StartMove(axisRotate.p_posActual - dAngle * 1000);
            axisRotate.WaitReady();


            // 정확도를 위해 반복성 수행
            for (int cnt = 1; cnt < m_AlignCount; cnt++)
            {
                // First Point
                if (m_module.Run(axisXY.StartMove(firstPoint)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                score = TemplateMatching(alignRecipe, out outX, out outY, out featureIndex);
                if (score < m_score)
                {
                    return "First Point Align Fail [Score : " + score.ToString() + "]";
                }

                // Second Point
                if (m_module.Run(axisXY.StartMove(secondPoint)))
                    return p_sInfo;
                if (m_module.Run(axisXY.WaitReady()))
                    return p_sInfo;

                score2 = TemplateMatchingWithSelectedFeature(alignRecipe, featureIndex, out outX2, out outY2);


                if (score2 < m_score)
                    return "Second Point Align Fail [Score : " + score2.ToString() + "]";


                dAngle = CalcAngle(outX, outY, outX2, outY2);

                axisRotate.StartMove(axisRotate.p_posActual - dAngle * 1000);
                axisRotate.WaitReady();
            }


            RecipeType_ImageData feature = alignRecipe.AlignFeatureList[featureIndex];
            int featureWidth = feature.Width;
            int featureHeight = feature.Height;
            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;


            m_grabMode.m_ptXYAlignData = new RPoint(-(outX + (featureWidth / 2) - camWidth / 2) * m_grabMode.m_dRealResX_um * 10, (outY + (featureHeight / 2) - camHeight / 2) * m_grabMode.m_dRealResX_um * 10);

            m_module.RunTree(Tree.eMode.RegWrite);
            m_module.RunTree(Tree.eMode.Init);

            m_grabMode.SetLight(false);

            timer.Stop();
            System.Diagnostics.Debug.WriteLine(timer.ElapsedMilliseconds);

            // Old
            //StopWatch sw = new StopWatch();

            //sw.Start();
            //if (m_CamAlign == null) return "Cam == null";

            //m_CamAlign.Connect();
            //while (!m_CamAlign.m_ConnectDone)
            //{
            //    if (EQ.IsStop()) return "OK";
            //}


            //m_module.p_bStageVac = true;
            //AxisXY axisXY = m_module.AxisXY;
            //Axis axisZ = m_module.AxisZ;

            //if (m_grabMode == null) return "Grab Mode == null";

            //m_grabMode.SetLight(true);

            //double dPos = m_focusPosZ;
            //if(m_grabMode.m_dVRSFocusPos != 0)
            //{
            //    dPos = m_grabMode.m_dVRSFocusPos + m_dVRSToAlignOffsetZ;
            //}

            //if (m_module.Run(axisZ.WaitReady()))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;

            //if (m_module.Run(axisZ.StartMove(dPos)))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.StartMove(m_firstPointPulse)))
            //    return p_sInfo;

            //if (m_module.Run(axisZ.WaitReady()))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;


            //string strVRSImageDir = @"C:\Users\ATI\Desktop\image\";
            //int camWidth = m_CamAlign.GetRoiSize().X;
            //int camHeight = m_CamAlign.GetRoiSize().Y;

            //// 이미지 회득
            //ImageData img = m_CamAlign.p_ImageViewer.p_ImageData;
            //string strVRSImageFullPath;
            //if (m_CamAlign.Grab() == "OK")
            //{
            //    strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", 0);
            //    img.SaveImageSync(strVRSImageFullPath);
            //}
            //IntPtr src = img.GetPtr();
            //byte[] rawdata;

            //int firstPosX = 0, firstPosY = 0, secondPosX = 0, secondPosY = 0;
            //int width = 0, height = 0;
            //int posX = 0, posY = 0;

            //// Feature 이미지들 저장한곳 찿
            //DirectoryInfo di = new DirectoryInfo(m_saveAlignFailImagePath);

            //int resPosX = 0;
            //int resPosY = 0;
            //float maxScore = 0;
            //string matchingFeaturePath = "";
            //byte[] findRawData = null;
            //int findWidth = 0, findHeight = 0;
            //float result;
            //foreach (FileInfo file in di.GetFiles())
            //{
            //    string fullname = file.FullName;
            //    unsafe
            //    {

            //        rawdata = Tools.LoadBitmapToRawdata(fullname, &width, &height);

            //        result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), rawdata, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, width, height, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3, 0);
            //    }
            //    if (maxScore < result)
            //    {
            //        maxScore = result;
            //        resPosX = posX;
            //        resPosY = posY;
            //        findRawData = rawdata;
            //        findWidth = width;
            //        findHeight = height;
            //        matchingFeaturePath = fullname;
            //    }
            //}
            //if (maxScore < m_score)
            //    return "First Point Align Fail [Score : " + maxScore.ToString() + "]";



            //if (m_module.Run(axisXY.StartMove(m_secondPointPulse)))
            //    return p_sInfo;
            //if (m_module.Run(axisXY.WaitReady()))
            //    return p_sInfo;


            //if (m_CamAlign.Grab() == "OK")
            //{
            //    strVRSImageFullPath = string.Format(strVRSImageDir + "test_{0}.bmp", 1);
            //    img.SaveImageSync(strVRSImageFullPath);
            //}

            //IntPtr src2 = img.GetPtr();

            //int resPosX2 = 0;
            //int resPosY2 = 0;

            //unsafe
            //{
            //    result = CLR_IP.Cpp_TemplateMatching((byte*)(src2.ToPointer()), findRawData, &resPosX2, &resPosY2, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3, 0);
            //}

            //if (result < m_score)
            //    return "Second Point Align Fail [Score : " + result.ToString() + "]";

            //double resAngle = CalcAngle(resPosX, resPosY, resPosX2, resPosY2);

            //Axis axisRotate = m_module.AxisRotate;
            //axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
            //axisRotate.WaitReady();



            //if (m_AlignCount > 1)
            //{
            //    for (int cnt = 1; cnt < m_AlignCount; cnt++)
            //    {

            //        for (int i = 0; i < 2; i++)
            //        {


            //            bool IsFirst;
            //            if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
            //            {
            //                IsFirst = true;
            //            }
            //            else
            //            {
            //                IsFirst = false;
            //            }

            //            if (m_CamAlign.Grab() == "OK")
            //            {
            //                strVRSImageFullPath = string.Format(strVRSImageDir + "Repeat Img{0}.bmp", cnt + (i + 1));
            //                img.SaveImageSync(strVRSImageFullPath);
            //            }
            //            src = img.GetPtr();
            //            int PosX = 0, PosY = 0;
            //            unsafe
            //            {
            //                result = CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), findRawData, &PosX, &PosY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3, 0);
            //            }
            //            if (IsFirst)
            //            {
            //                if (result < m_score)
            //                    return "Align Count :" + cnt.ToString() + "First Point Align Fail [Score : " + result.ToString() + "]";

            //                firstPosX = PosX;
            //                firstPosY = PosY;
            //                if (i != 1)
            //                {
            //                    if (m_module.Run(axisXY.StartMove(m_secondPointPulse)))
            //                        return p_sInfo;
            //                    if (m_module.Run(axisXY.WaitReady()))
            //                        return p_sInfo;
            //                }

            //            }
            //            else
            //            {
            //                if (result < m_score)
            //                    return "Align Count :" + cnt.ToString() + "Second Point Align Fail [Score : " + result.ToString() + "]";

            //                secondPosX = PosX;
            //                secondPosY = PosY;

            //                if (i != 1)
            //                {
            //                    if (m_module.Run(axisXY.StartMove(m_firstPointPulse)))
            //                        return p_sInfo;
            //                    if (m_module.Run(axisXY.WaitReady()))
            //                        return p_sInfo;
            //                }
            //            }

            //        }
            //        resAngle = CalcAngle(firstPosX, firstPosY, secondPosX, secondPosY);

            //        axisRotate.StartMove((axisRotate.p_posActual - resAngle * 1000));
            //        axisRotate.WaitReady();
            //    }

            //}

            //if (m_CamAlign.Grab() != "OK") return "Grab Error";
            //RPoint pulse;
            //if (Math.Abs(axisXY.p_posActual.X - m_firstPointPulse.X) < 10)
            //{
            //    pulse = m_firstPointPulse;
            //}
            //else
            //{
            //    pulse = m_secondPointPulse;
            //}
            //src = img.GetPtr();
            //unsafe
            //{
            //    CLR_IP.Cpp_TemplateMatching((byte*)(src.ToPointer()), findRawData, &posX, &posY, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, findWidth, findHeight, 0, 0, img.GetBitMapSource().PixelWidth, img.GetBitMapSource().PixelHeight, 5, 3, 0);
            //}

            //m_grabMode.m_ptXYAlignData = new RPoint(-(posX + (width / 2) - camWidth / 2) * m_AlignCamResolution * 10, (posY + (height / 2) - camHeight / 2) * m_AlignCamResolution * 10);
            //m_module.RunTree(Tree.eMode.RegWrite);
            //m_module.RunTree(Tree.eMode.Init);

            //m_grabMode.SetLight(false);

            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            return "OK";
        }

        private double TemplateMatching(FrontAlignRecipe alignRecipe, out long maxOutX, out long maxOutY, out int featureIndex)
        {
            ImageData camImage = m_CamAlign.p_ImageViewer.p_ImageData;
            IntPtr camImagePtr = camImage.GetPtr();
            double maxScore = double.MinValue;
            maxOutX = 0;
            maxOutY = 0;
            int index = 0;
            featureIndex = 0;
            foreach (RecipeType_ImageData feature in alignRecipe.AlignFeatureList)
            {
                byte[] rawdata = feature.RawData;
                double result;
                int outX = 0, outY = 0;
                unsafe
                {
                    result = CLR_IP.Cpp_TemplateMatching(
                        (byte*)(camImagePtr.ToPointer()),
                        rawdata,
                        &outX,
                        &outY,
                        camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
                        feature.Width, feature.Height,
                        0,
                        0,
                        camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
                        5, 3, 0);
                }

                if (maxScore < result)
                {
                    maxScore = result;
                    maxOutX = outX;
                    maxOutY = outY;
                    featureIndex = index;
                }
                index++;
            }

            return maxScore;
        }



        private double TemplateMatchingWithSelectedFeature(FrontAlignRecipe alignRecipe, int featureIndex, out long maxOutX, out long maxOutY)
        {
            ImageData camImage = m_CamAlign.p_ImageViewer.p_ImageData;
            IntPtr camImagePtr = camImage.GetPtr();
            double maxScore = double.MinValue;
            maxOutX = 0;
            maxOutY = 0;
            int index = 0;
            RecipeType_ImageData feature = alignRecipe.AlignFeatureList[featureIndex];

            byte[] rawdata = feature.RawData;
            double result;
            int outX = 0, outY = 0;
            unsafe
            {
                result = CLR_IP.Cpp_TemplateMatching(
                    (byte*)(camImagePtr.ToPointer()),
                    rawdata,
                    &outX,
                    &outY,
                    camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
                    feature.Width, feature.Height,
                    0,
                    0,
                    camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
                    5, 3, 0);
            }

            if (maxScore < result)
            {
                maxScore = result;
                maxOutX = outX;
                maxOutY = outY;
            }

            index++;

            return maxScore;
        }

        private double CalcAngle(long posX, long posY, long posX2, long posY2)
        {            
            FrontAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontAlignRecipe>();

            int camWidth = m_CamAlign.GetRoiSize().X;
            int camHeight = m_CamAlign.GetRoiSize().Y;
            double cx = alignRecipe.FirstSearchPointX / PULSE_TO_UM - ((camWidth / 2) + posX) * m_grabMode.m_dRealResX_um;
            double cy = alignRecipe.FirstSearchPointY / PULSE_TO_UM - ((camHeight / 2) + posY) * m_grabMode.m_dRealResX_um;

            double cx2 = alignRecipe.SecondSearchPointX / PULSE_TO_UM - ((camWidth / 2) + posX2) * m_grabMode.m_dRealResX_um;
            double cy2 = alignRecipe.SecondSearchPointY / PULSE_TO_UM - ((camHeight / 2) + posY2) * m_grabMode.m_dRealResX_um;


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
