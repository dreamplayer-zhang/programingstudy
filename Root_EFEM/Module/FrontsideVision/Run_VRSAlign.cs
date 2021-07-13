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
    public class Run_VRSAlign : ModuleRunBase
    {
        Vision_Frontside m_module;
        //public RPoint m_firstPointPulse = new RPoint();
        //public RPoint m_secondPointPulse = new RPoint();
        public int m_focusPosZ = 0;
        public Camera_Basler m_CamVRS;
        //public bool m_saveAlignFailImage = false;
        //public string m_saveAlignFailImagePath = "D:\\";
        public int m_score = 80;
        public int m_repeatCnt = 1;
        public int m_failMovePulse = 10000; // 10000 pulse: 1mm
        //public double m_AlignCamResolution = 5.5f;
        public int m_AlignCount = 1;
        const int PULSE_TO_UM = 10; // 10 pulse:1um
        public double m_dVRSToAlignOffsetZ = 0;
        public GrabModeFront m_grabMode = null;
        string m_sGrabMode = "";
        public int m_searchOffset = 0;
        public int m_searchInterval = 0;

        double m_diePitchX = 5718;
        double m_diePitchY = 4358;
        double m_shotOffsetX = -151.1;
        double m_shotOffsetY = -6536.9;
        double m_shotSizeX = 3;
        double m_shotSizeY = 3;
        double m_scribeLineX = 80;
        double m_scribeLineY = 80;

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

        public Run_VRSAlign(Vision_Frontside module)
        {
            m_module = module;
            m_CamVRS = m_module.CamVRS;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_VRSAlign run = new Run_VRSAlign(m_module);
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

            run.m_searchOffset = this.m_searchOffset;
            run.m_searchInterval = this.m_searchInterval;

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
            m_AlignCount = tree.Set(m_AlignCount, m_AlignCount, "Align Count", "Align Count", bVisible);
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
            m_dVRSToAlignOffsetZ = tree.Set(m_dVRSToAlignOffsetZ, m_dVRSToAlignOffsetZ, "VRS To Align Offset Z Pos", "VRS To Align Offset Z Pos", bVisible);

            // New
            m_sRecipeName = tree.Set(m_sRecipeName, m_sRecipeName, "Recipe Name", "Recipe Name", bVisible);

            m_searchOffset = tree.Set(m_searchOffset, m_searchOffset, "Search Offset", "Search Offset", bVisible);
            m_searchInterval = tree.Set(m_searchInterval, m_searchInterval, "Search Interval", "Search Interval", bVisible);
        }

        public override string Run()
        {
            // 20210712 jhan
            StopWatch timer = new StopWatch();
            timer.Start();

            // Camera Connection
            if (m_CamVRS == null)
            {
                return "Cam == null";
            }

            m_CamVRS.Connect();
            while (!m_CamVRS.m_ConnectDone)
            {
                if (EQ.IsStop())
                {
                    return "OK";
                }
            }

            // Init
            m_module.p_bStageVac = true;
            AxisXY axisXY = m_module.AxisXY;
            Axis axisZ = m_module.AxisZ;

            if (m_grabMode == null)
            {
                return "Grab Mode == null";
            }

            m_grabMode.SetLight(true);

            //Recipe Open
            RecipeAlign recipe = GlobalObjects.Instance.Get<RecipeAlign>();
            //FrontVRSAlignRecipe alignRecipe = recipe.GetItem<FrontVRSAlignRecipe>();

            //if (alignRecipe == null)
            //{
            //    return "FrontAlignVRSRecipe == null";
            //}
            ///*if (!recipe.Read(m_sRecipeName))
            //{
            //    return "Recipe Open Fail";
            //}*/
            //if (alignRecipe.AlignFeatureVRSList.Count == 0)
            //{
            //    return "Align Feature Count == 0";
            //}

            // Move Z
            double dPos = m_focusPosZ;
            if (m_grabMode.m_dVRSFocusPos != 0)
            {
                dPos = m_grabMode.m_dVRSFocusPos + m_dVRSToAlignOffsetZ;
            }

            if (m_module.Run(axisZ.WaitReady()))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisXY.WaitReady()))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.StartMove(dPos)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisZ.WaitReady()))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisXY.WaitReady()))
            {
                return p_sInfo;
            }

            RPoint centerPoint = axisXY.p_posActual;
            RPoint shotCenterPoint = new RPoint(centerPoint.X + m_shotOffsetX * PULSE_TO_UM, centerPoint.Y + m_shotOffsetY * PULSE_TO_UM);
            RPoint initShotLBPoint = new RPoint(centerPoint.X - (m_diePitchX + m_scribeLineX) * m_shotSizeX / 2 * PULSE_TO_UM, centerPoint.Y - (m_diePitchY + m_scribeLineY) * m_shotSizeY / 2 * PULSE_TO_UM);
            RPoint foundShotLBPoint = new RPoint(0, 0);

            // Shot Center Point
            /*if (m_module.Run(axisXY.StartMove(shotCenterPoint)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisXY.WaitReady()))
            {
                return p_sInfo;
            }

            // Init Shot Point
            if (m_module.Run(axisXY.StartMove(initShotLBPoint)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisXY.WaitReady()))
            {
                return p_sInfo;
            }*/

            long outX, outY;
            long maxOutX = 0;
            long maxOutY = 0;
            int featureIndex;
            double score = 0;
            double scoreMax = 0;

            for (int y = (int)centerPoint.Y - m_searchOffset; y < (int)centerPoint.Y + m_searchOffset; y+=m_searchInterval)
            {
                for (int x = (int)centerPoint.X - m_searchOffset; x < (int)centerPoint.X + m_searchOffset; x += m_searchInterval)
                {
                    if (m_module.Run(axisXY.StartMove(new RPoint(x, y))))
                    {
                        return p_sInfo;
                    }
                    if (m_module.Run(axisXY.WaitReady()))
                    {
                        return p_sInfo;
                    }

                    //score = TemplateMatching(alignRecipe, out outX, out outY, out featureIndex);
                    if (score > scoreMax)
                    {
                        scoreMax = score;
                        //maxOutX = outX;
                        //maxOutY = outY;
                        foundShotLBPoint.X = x;
                        foundShotLBPoint.Y = y;
                    }
                    //break;
                }
                //break;
            }

            /*if (scoreMax < m_score)
            {
                return "VRS Align Fail [Score : " + score.ToString() + "]";
            }*/

            // Found Shot Point
            if (m_module.Run(axisXY.StartMove(foundShotLBPoint)))
            {
                return p_sInfo;
            }
            if (m_module.Run(axisXY.WaitReady()))
            {
                return p_sInfo;
            }

            /*double dAngle = CalcAngle(, maxOutX, maxOutY);

            Axis axisRotate = m_module.AxisRotate;
            axisRotate.StartMove(axisRotate.p_posActual - dAngle * 1000);
            axisRotate.WaitReady();*/

            // 정확도를 위해 반복성 수행
            /*for (int cnt = 1; cnt < m_AlignCount; cnt++)
            {
                // Shot Point
                if (m_module.Run(axisXY.StartMove(shotPoint)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                {
                    return p_sInfo;
                }

                score = TemplateMatching(alignRecipe, out outX, out outY, out featureIndex);
                if (score < m_score)
                {
                    return "First Point Align Fail [Score : " + score.ToString() + "]";
                }

                // Map Point
                if (m_module.Run(axisXY.StartMove(mapPoint)))
                {
                    return p_sInfo;
                }
                if (m_module.Run(axisXY.WaitReady()))
                {
                    return p_sInfo;
                }

                score2 = TemplateMatchingWithSelectedFeature(alignRecipe, featureIndex, out outX2, out outY2);

                if (score2 < m_score)
                {
                    return "Second Point Align Fail [Score : " + score2.ToString() + "]";
                }

                dAngle = CalcAngle(outX, outY, outX2, outY2);

                axisRotate.StartMove(axisRotate.p_posActual - dAngle * 1000);
                axisRotate.WaitReady();
            }

            RecipeType_ImageData feature = alignRecipe.AlignFeatureVRSList[featureIndex];
            int featureWidth = feature.Width;
            int featureHeight = feature.Height;
            int camWidth = m_CamVRS.GetRoiSize().X;
            int camHeight = m_CamVRS.GetRoiSize().Y;

            //m_grabMode.m_ptXYAlignData = new RPoint(-(outX + (featureWidth / 2) - camWidth / 2) * m_grabMode.m_dRealResX_um * 10, (outY + (featureHeight / 2) - camHeight / 2) * m_grabMode.m_dRealResX_um * 10);

            m_module.RunTree(Tree.eMode.RegWrite);
            m_module.RunTree(Tree.eMode.Init);

            //m_grabMode.SetLight(false);

            timer.Stop();
            System.Diagnostics.Debug.WriteLine(timer.ElapsedMilliseconds);*/

            return "OK";
        }

        //private double TemplateMatching(FrontVRSAlignRecipe alignRecipe, out long maxOutX, out long maxOutY, out int featureIndex)
        //{
        //    ImageData camImage = m_CamVRS.p_ImageViewer.p_ImageData;
        //    IntPtr camImagePtr = camImage.GetPtr();
        //    double maxScore = double.MinValue;
        //    maxOutX = 0;
        //    maxOutY = 0;
        //    int index = 0;
        //    featureIndex = 0;
        //    foreach (RecipeType_ImageData feature in alignRecipe.AlignFeatureVRSList)
        //    {
        //        byte[] rawdata = feature.RawData;
        //        double result;
        //        int outX = 0, outY = 0;
        //        unsafe
        //        {
        //            result = CLR_IP.Cpp_TemplateMatching(
        //                (byte*)(camImagePtr.ToPointer()),
        //                rawdata,
        //                &outX,
        //                &outY,
        //                camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
        //                feature.Width, feature.Height,
        //                0,
        //                0,
        //                camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
        //                5, 3, 0);
        //        }

        //        if (maxScore < result)
        //        {
        //            maxScore = result;
        //            maxOutX = outX;
        //            maxOutY = outY;
        //            featureIndex = index;
        //        }
        //        index++;
        //    }

        //    return maxScore;
        //}

        //private double TemplateMatchingWithSelectedFeature(FrontVRSAlignRecipe alignRecipe, int featureIndex, out long maxOutX, out long maxOutY)
        //{
        //    ImageData camImage = m_CamVRS.p_ImageViewer.p_ImageData;
        //    IntPtr camImagePtr = camImage.GetPtr();
        //    double maxScore = double.MinValue;
        //    maxOutX = 0;
        //    maxOutY = 0;
        //    int index = 0;
        //    RecipeType_ImageData feature = alignRecipe.AlignFeatureVRSList[featureIndex];

        //    byte[] rawdata = feature.RawData;
        //    double result;
        //    int outX = 0, outY = 0;
        //    unsafe
        //    {
        //        result = CLR_IP.Cpp_TemplateMatching(
        //            (byte*)(camImagePtr.ToPointer()),
        //            rawdata,
        //            &outX,
        //            &outY,
        //            camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
        //            feature.Width, feature.Height,
        //            0,
        //            0,
        //            camImage.GetBitMapSource().PixelWidth, camImage.GetBitMapSource().PixelHeight,
        //            5, 3, 0);
        //    }

        //    if (maxScore < result)
        //    {
        //        maxScore = result;
        //        maxOutX = outX;
        //        maxOutY = outY;
        //    }

        //    index++;

        //    return maxScore;
        //}

        //private double CalcAngle(long posX, long posY, long posX2, long posY2)
        //{
        //    FrontVRSAlignRecipe alignRecipe = GlobalObjects.Instance.Get<RecipeAlign>().GetItem<FrontVRSAlignRecipe>();

        //    int camWidth = m_CamVRS.GetRoiSize().X;
        //    int camHeight = m_CamVRS.GetRoiSize().Y;
        //    double cx = alignRecipe.MapOffsetX / PULSE_TO_UM - ((camWidth / 2) + posX) * m_grabMode.m_dRealResX_um;
        //    double cy = alignRecipe.MapOffsetY / PULSE_TO_UM - ((camHeight / 2) + posY) * m_grabMode.m_dRealResX_um;

        //    double cx2 = alignRecipe.ShotOffsetX / PULSE_TO_UM - ((camWidth / 2) + posX2) * m_grabMode.m_dRealResX_um;
        //    double cy2 = alignRecipe.ShotOffsetY / PULSE_TO_UM - ((camHeight / 2) + posY2) * m_grabMode.m_dRealResX_um;


        //    double radian = Math.Atan2(cy2 - cy, cx2 - cx);
        //    double angle = radian * (180 / Math.PI);
        //    double resAngle;
        //    if (cy2 - cy < 0)
        //    {
        //        resAngle = angle + 180;

        //    }
        //    else
        //    {
        //        resAngle = angle - 180;
        //    }

        //    return resAngle;
        //}
    }
}