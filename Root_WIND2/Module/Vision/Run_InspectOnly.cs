using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Control;
using RootTools.Database;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using RootTools_Vision.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2.Module
{
    public class Run_InspectOnly : ModuleRunBase
    {
        #region [Klarf]
        private static KlarfData_Lot klarfData = new KlarfData_Lot();


        private static void LotStart(string klarfPath, RecipeBase recipe, InfoWafer infoWafer, GrabModeBase grabMode)
        {
            if (klarfData == null)
            {
                klarfData = new KlarfData_Lot();
            }

            klarfData.SetModuleName("Front");

            if (Directory.Exists(klarfPath)) Directory.CreateDirectory(klarfPath);


            klarfData.LotStart(klarfPath, infoWafer, recipe.WaferMap, grabMode);
        }

        private void CreateKlarf(RecipeBase recipe, InfoWafer infoWafer, List<Defect> defectList, bool useTDIReview = false, bool useVrsReview = false)
        {
            //klarfData.SetResolution((float)camInfo.RealResX, (float)camInfo.RealResY);
            // Product 정보 셋팅

            klarfData.WaferStart(recipe.WaferMap, infoWafer);
            klarfData.AddSlot(recipe.WaferMap, defectList, recipe.GetItem<OriginRecipe>(), useTDIReview, useVrsReview);
            klarfData.SaveKlarf();
        }

        private void LotEnd(InfoWafer infoWafer)
        {
            klarfData.CreateLotEnd();
        }

        #endregion

        Vision m_module;

        string m_sRecipeName = string.Empty;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        //bool m_bInvDir = false;
        public GrabModeFront m_grabMode = null;

        #region [Getter Setter]
        public string RecipeName
        {
            get => m_sRecipeName;
            set => m_sRecipeName = value;
        }

        //public string p_sGrabMode
        //{
        //    get { return m_sGrabMode; }
        //    set
        //    {
        //        m_sGrabMode = value;
        //        m_grabMode = m_module.GetGrabMode(value);
        //    }
        //}
        #endregion

        public Run_InspectOnly(Vision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_InspectOnly run = new Run_InspectOnly(m_module);
            //run.p_sGrabMode = p_sGrabMode;
            run.RecipeName = this.RecipeName;

            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
        }

        public override string Run()
        {

            Settings settings = new Settings();
            SettingItem_SetupFrontside settings_frontside = settings.GetItem<SettingItem_SetupFrontside>();


            // 여기 현장버전으로 수정되야함
            InfoWafer infoWafer = m_module.GetInfoWafer(0);

            RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

            if (recipe.Read(this.m_sRecipeName) == false)
                return "Recipe Read Fail";

            m_grabMode = m_module.GetGrabMode(recipe.CameraInfoIndex);

            // Check Lot Start
            if ((infoWafer != null) && (
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer) ||
                (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstWafer)))
            {
                LotStart(settings_frontside.KlarfSavePath, recipe, infoWafer, m_grabMode);
            }



            StopWatch inspectionTimeWatcher = new StopWatch();
            inspectionTimeWatcher.Start();

            //레시피에 GrabMode 저장하고 있어야함

            RootTools_Vision.WorkManager3.WorkManager workManager = GlobalObjects.Instance.GetNamed<RootTools_Vision.WorkManager3.WorkManager>("frontInspection");

            if (workManager == null)
            {
                throw new ArgumentException("WorkManager가 초기화되지 않았습니다(null)");
            }
            workManager.Stop();

            //InspectionManagerFrontside inspectionFront = GlobalObjects.Instance.Get<InspectionManagerFrontside>();
            //inspectionFront.Stop();

            if (m_grabMode == null) return "Grab Mode == null";

            if (EQ.IsStop() == false)
            {
                if (workManager.OpenRecipe(m_sRecipeName) == false)
                    return "Recipe Open Fail";

                workManager.Start();

            }
            else
            {
                workManager.Stop();
            }

            try
            { 
                if (workManager.WaitWorkDone(ref EQ.m_EQ.StopToken(), 60 * 3 /*3 minutes*/) == false)
                {
                    // Time out!!


                    // Save Result
                    //workManager.Start(false, true);
                    inspectionTimeWatcher.Stop();

                    RootTools_Vision.TempLogger.Write("Inspection", "Time out!!!");
                    return "OK";
                } // 5 minutes
                else
                {
                    #region [Klarf]
                    if (settings_frontside.UseKlarf && (infoWafer != null))
                    {
                        DataTable table = DatabaseManager.Instance.SelectCurrentInspectionDefect();
                        List<Defect> defects = Tools.DataTableToDefectList(table);

                        WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();

                        CreateKlarf(recipe, infoWafer, defects, settings_frontside.UseTDIReview, settings_frontside.UseVrsReview);

                        //KlarfData_Lot klarfData = new KlarfData_Lot();
                        //Directory.CreateDirectory(settings_frontside.KlarfSavePath);
                        //klarfData.SetResolution((float)camInfo.RealResX, (float)camInfo.RealResY);
                        //klarfData.WaferStart(recipe.WaferMap, DateTime.Now);
                        //klarfData.SetResultTimeStamp();
                        //klarfData.AddSlot(recipe.WaferMap, defects, recipe.GetItem<OriginRecipe>(), settings_frontside.UseTDIReview, settings_frontside.UseVrsReview);
                        //klarfData.SaveKlarf(settings_frontside.KlarfSavePath, false);


                        //***************** VRS Capture Sequence *********************//
                        ConcurrentQueue<byte[]> vrsImageQueue = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();
                        Camera_Basler camVrs = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision.p_CamVRS;
                        Rect vrsRect = new Rect(0, 0, camVrs.p_ImageData.p_Size.X, camVrs.p_ImageData.p_Size.Y);



                        if (settings_frontside.UseVrsReview)
                        {
                            AxisXY axisXY = m_module.AxisXY;
                            Axis axisZ = m_module.AxisZ;

                            for (int i = 0; i < defects.Count; i++)
                            {

                                CPoint vrsPoint = PositionConverter.ConvertImageToVRS(m_module, new CPoint((int)defects[i].m_fAbsX, (int)defects[i].m_fAbsY));

                                // XY 찍는 위치로 이동
                                if (m_module.Run(axisXY.WaitReady()))
                                    return p_sInfo;
                                if (m_module.Run(axisXY.StartMove(new RPoint(vrsPoint.X, vrsPoint.Y))))
                                    return p_sInfo;
                                if (m_module.Run(axisXY.WaitReady()))
                                    return p_sInfo;

                                if (!m_module.p_CamVRS.m_ConnectDone)
                                {
                                    m_module.p_CamVRS.FunctionConnect();
                                }
                                else
                                {
                                    if (m_module.p_CamVRS.p_CamInfo._IsGrabbing == false)
                                    {
                                        m_module.p_CamVRS.GrabContinuousShot();
                                    }
                                }

                                // Capture
                                Thread.Sleep(1000); // 나중에 줄이기
                                byte[] copyBuffer = null;
                                camVrs.CopyToBuffer(out copyBuffer, vrsRect);
                                vrsImageQueue.Enqueue(copyBuffer);
                            }
                        }

                        //************************************************************//

                        Thread.Sleep(2000);

                        if (settings_frontside.UseTDIReview && settings_frontside.UseVrsReview)
                        {
                            klarfData.SaveTiffImageBoth(defects, workManager.SharedBuffer, new Size(160, 120), vrsImageQueue, new Size(vrsRect.Width, vrsRect.Height));
                        }
                        else if (settings_frontside.UseTDIReview)
                        {
                            klarfData.SaveTiffImageOnlyTDI(defects, workManager.SharedBuffer, new Size(160, 120));
                        }
                        else if (settings_frontside.UseVrsReview)
                        {
                            klarfData.SaveTiffImageOnlyVRS(defects, vrsImageQueue, new Size(vrsRect.Width, vrsRect.Height));
                        }

                        if (settings_frontside.UseKlarfWholeWaferImage)
                        {
                            klarfData.SaveImageJpg(workManager.SharedBuffer,
                            new Rect(settings_frontside.WholeWaferImageStartX, settings_frontside.WholeWaferImageStartY, settings_frontside.WholeWaferImageEndX, settings_frontside.WholeWaferImageEndY),
                            (long)(settings_frontside.WholeWaferImageCompressionRate * 100),
                            settings_frontside.OutputImageSizeX,
                            settings_frontside.OutputImageSizeY);
                        }
                    }

                    #endregion
                }

                inspectionTimeWatcher.Stop();
                RootTools_Vision.TempLogger.Write("Inspection", string.Format("{0:F3}", (double)inspectionTimeWatcher.ElapsedMilliseconds / (double)1000));



                // LotEnd Check
                if ((infoWafer != null) && (
                    (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.FirstLastWafer) ||
                    (infoWafer.p_eWaferOrder == InfoWafer.eWaferOrder.LastWafer)))
                {
                    LotEnd(infoWafer);
                }


                return "OK";
            }


            finally
            {
                m_grabMode.SetLight(false);
            }
        }
    }
}
